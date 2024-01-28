using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using SkirtingBoardsCreator.Creation.Controllers;
using SkirtingBoardsCreator.Creation.Domain.Enums;
using SkirtingBoardsCreator.Creation.Domain.Interfaces;
using SkirtingBoardsCreator.Creation.Domain.Entities;

namespace SkirtingBoardsCreator.Creation.Application.UseCases
{
    internal class HostLinesCreation
    {
        private ISkBoardsController Controller { get; }

        private Stack<ModelLine> ModelLineStack { get; set; } = new Stack<ModelLine>();

        public HostLinesCreation(ISkBoardsController controller) => Controller = controller;

        public List<ILineParameters> CreateHostLines() =>
            CreateResultLinesList(
                CreateHostLines(
                    CreateLinesList(
                        GetEdgeArrayFromRoom())));

        private EdgeArray GetEdgeArrayFromRoom()
        {
            EdgeArray edgeArray = default;

            foreach (GeometryObject geoObj in Controller.SelectedRoom.ClosedShell)
            {
                if (geoObj is Solid solid)
                {
                    if (solid.Volume > 0)
                    {
                        edgeArray = solid.Edges;
                        break;
                    }
                }
            }

            return edgeArray;
        }

        #region CreateLinesList

        private bool IsModelLine(Line line)
        {
            bool result = false;
            XYZ start = line.GetEndPoint(0);
            XYZ end = line.GetEndPoint(1);
            Curve curve;
            XYZ curveStart;
            XYZ curveEnd;

            foreach (ModelLine modelLine in ModelLineStack)
            {
                curve = modelLine.GeometryCurve;
                curveStart = curve.GetEndPoint(0);
                curveEnd = curve.GetEndPoint(1);

                if (LineComparison(start, end, curveStart, curveEnd))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        private List<ILineParameters> CreateLinesList(EdgeArray edgeArray)
        {
            double pointZ;
            List<ILineParameters> lines = new List<ILineParameters>();
            Stack<double> zStack = new Stack<double>();
            foreach (object obj in edgeArray)
            {
                if (obj is Edge edge)
                {
                    if (edge.AsCurve() is Line line)
                    {
                        if (Math.Round(line.Direction.Z, 3) != 0) continue;

                        pointZ = Math.Round(line.Origin.Z, 3);
                        if (zStack.Count == 0)
                        {
                            zStack.Push(pointZ);
                        }
                        else
                        {
                            if (pointZ < zStack.Peek())
                            {
                                zStack.Clear();
                                zStack.Push(pointZ);
                                lines = new List<ILineParameters>();
                            }
                        }

                        if (pointZ == zStack.Peek())
                        {
                            if (IsModelLine(line)) continue;

                            if (!IsEqualPoints(line.GetEndPoint(0), line.GetEndPoint(1)))
                            { 
                                lines.Add(new LineParameters(lines.Count, line));
                            }
                        }
                    }
                }
            }

            return lines;
        }
       
        #endregion

        #region CreateHostLines

        #region CreateDoorPointsOnLines

        private List<int> GetIntersectIndex(ElementId[] ids, ElementId id, BuiltInCategory bic)
        {
            List<int> result = new List<int>();
            var element = Controller.DocLink.GetElement(id);
            var bb = element.get_BoundingBox(Controller.Doc.ActiveView);
            var elements = new FilteredElementCollector(Controller.DocLink)
                .OfCategory(bic)
                .WherePasses(new BoundingBoxIntersectsFilter(new Outline(bb.Min, bb.Max), false))
                .ToList();

            for (int i = 0; i < ids.Length; i++)
            {
                if (elements.FirstOrDefault(x => x.Id == ids[i]) != default)
                { 
                    result.Add(i); 
                }
            }

            return result;
        }

        private List<DependencyElement> GetDependencyElementsLocations()
        {
            List<DependencyElement> dependencyElements = new List<DependencyElement>();
            Element e;
            DependencyWindow dependencyWindow;
            FamilyInstance instance;
            Stack<ElementId> elementIds = new Stack<ElementId>();

            IList<IList<BoundarySegment>> boundarySegmentArray = Controller.SelectedRoom
                .GetBoundarySegments(new SpatialElementBoundaryOptions
                {
                    SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish
                });

            List<ElementId> windows = new List<ElementId>();

            foreach (IList<BoundarySegment> bSegments in boundarySegmentArray)
            {
                foreach (BoundarySegment bSegment in bSegments)
                {
                    e = Controller.DocLink.GetElement(bSegment.ElementId);
                    if (e is ModelLine ml)
                    {
                        ModelLineStack.Push(ml);
                        continue;
                    }
                    if (e is HostObject host)
                    {
                        foreach (ElementId elementId in host.FindInserts(true, true, true, true).ToList())
                        {
                            if (elementIds.Contains(elementId)) continue;

                            elementIds.Push(elementId);
                            instance = (Controller.DocLink.GetElement(elementId) as FamilyInstance);
                            if (instance == null) continue;
                            switch (instance.Category.Id.IntegerValue)
                            {
                                case -2000023: // Doors
                                    dependencyWindow = new DependencyWindow(
                                        instance.Id,
                                        (instance.Location as LocationPoint).Point,
                                        instance.Symbol.get_Parameter(BuiltInParameter.CASEWORK_WIDTH).AsDouble(),
                                        host);
                                    if (instance.Symbol.FamilyName.ToLower().Contains("откосы"))
                                    {
                                        dependencyWindow.SetFields(instance);
                                        windows.Add(dependencyWindow.ElementId);
                                        dependencyWindow.SetSymetricType(false);
                                    }
                                    else
                                    { 
                                        dependencyWindow.SetSymetricType(true);
                                    }
                                    dependencyElements.Add(dependencyWindow);
                                    
                                    break;
                                case -2000014: // Windows
                                    if (instance.Symbol.FamilyName.ToLower().Contains("дверь"))
                                    {
                                        dependencyWindow = new DependencyWindow(
                                            instance.Id,
                                            (instance.Location as LocationPoint).Point,
                                            instance.get_Parameter(BuiltInParameter.CASEWORK_WIDTH).AsDouble(),
                                            host);
                                        dependencyWindow.SetFields(instance);
                                        dependencyElements.Add(dependencyWindow);
                                    }
                                    break;
                                default: break;
                            }
                            dependencyWindow = default;
                        }
                    }
                }
            }

            if (dependencyElements.Count > 1)
            { 
                List<int> index = new List<int>();
                foreach (var id in windows)
                {
                    index = GetIntersectIndex(dependencyElements.Select(x => x.ElementId).ToArray(), id, BuiltInCategory.OST_Windows);
                    if (index.Count > 0)
                    {
                        index.Sort();
                        index.Reverse();
                        foreach (var i in index)
                        {
                            if (dependencyElements[i] is DependencyWindow dw)
                            {
                                if (dw.IsWindow)
                                { 
                                    dependencyElements.RemoveAt(i);
                                }
                            }
                        }
                    }
                }
            }
            return dependencyElements;
        }

        private bool IsDependencyElementInRoom(DependencyElement dElement)
        {
            bool result = false;
            double offset = 1.5d;
            XYZ hOrient = (Controller.DocLink.GetElement(dElement.ElementId) as FamilyInstance).FacingOrientation;
            XYZ point = new XYZ(hOrient.X * offset, hOrient.Y * offset, 0);
            XYZ loc = dElement.Location;

            if (dElement.HostObject is Wall)
            {
                bool isPointInRoom1 = Controller.SelectedRoom.IsPointInRoom(loc + point + new XYZ(0, 0, 0.5d));
                bool isPointInRoom2 = Controller.SelectedRoom.IsPointInRoom(loc - point + new XYZ(0, 0, 0.5d));
                result = isPointInRoom1 || isPointInRoom2;
            }

            return result;
        }

        private int SearchNearestLineIndex(List<ILineParameters> lines, DependencyElement dElement)
        {
            int index = -1;
            double distance;
            Stack<double> distancesStack = new Stack<double>();

            for (int i = 0; i < lines.Count; i++)
            {
                distance = lines[i].Line.Distance(dElement.Location);
                if (distancesStack.Count == 0)
                {
                    distancesStack.Push(distance);
                    index = i;
                }
                else if (distancesStack.Peek() < distance)
                {
                    continue;
                }
                else
                {
                    distancesStack.Clear();
                    distancesStack.Push(distance);
                    index = i;
                }
            }
            return index;
        }

        private int SearchNearestPointIndex(List<PointOnLine> pointOnLines, XYZ locSubtResult)
        {
            int index = -1;
            double distanceTemp;
            double distance = default;

            for (int i = 0; i < pointOnLines.Count; i++)
            {
                distanceTemp = locSubtResult.DistanceTo(pointOnLines[i].Point);

                if (distance == default)
                {
                    distance = distanceTemp;
                    index = i;
                }
                else if (distance != distanceTemp & distance > distanceTemp)
                {
                    distance = distanceTemp;
                    index = i;
                }
            }

            return index;
        }

        private bool IsAnyPointDefaultValue(DependencyElement dElement, ILineParameters iLine, out XYZ locSubtResult, out XYZ locAddResult)
        {
            if (dElement is DependencyWindow dWindow)
            {
                dWindow.SetPointMid(DoorWindowPoint(dWindow));

                dWindow.SetPoints(iLine.Line);
                locSubtResult = dWindow.Point0;
                locAddResult = dWindow.Point1;
            }
            else
            {
                double pointDistance;
                iLine.SetMidPoint(dElement.Location);
                pointDistance = iLine.Origin.DistanceTo(iLine.MidPoint);
                locSubtResult = iLine.Line.Evaluate(pointDistance - dElement.Width / 2, false);
                locAddResult = iLine.Line.Evaluate(pointDistance + dElement.Width / 2, false);
            }
            return false;
        }

        private XYZ DoorWindowPoint(DependencyWindow dWindow)
        {
            var eWindow = (Controller.PickedElement as RevitLinkInstance).GetLinkDocument().GetElement(dWindow.ElementId);
            var geometry = eWindow.get_Geometry(new Options() { DetailLevel = ViewDetailLevel.Fine });
            Stack<PlanarFace> stack = GetFaces(geometry);
            List<PlanarFace> pFaces = new List<PlanarFace>();
            if (stack.Count == 0)
            {
                var depElements = eWindow.GetDependentElements(new ElementClassFilter(typeof(FamilyInstance)));

                foreach (var dElement in depElements)
                {
                    eWindow = (Controller.PickedElement as RevitLinkInstance).GetLinkDocument().GetElement(dElement);
                    geometry = eWindow.get_Geometry(new Options() { DetailLevel = ViewDetailLevel.Fine });
                    stack = GetFaces(geometry);
                    if (stack.Count > 0)
                    {
                        pFaces.AddRange(stack);
                    }
                }
            }
            else
            { 
                pFaces.AddRange(stack);
            }

            XYZ temp = new XYZ();
            foreach (var line in pFaces) temp += line.Origin;

            return temp / pFaces.Count;
        }

        private Stack<PlanarFace> GetFaces(GeometryElement geometry)
        {
            Stack<PlanarFace> stack = new Stack<PlanarFace>();
            foreach (var geoObj in geometry)
            {
                foreach (var geoInstObj in (geoObj as GeometryInstance).GetInstanceGeometry())
                {
                    if (geoInstObj is Solid solid)
                    {
                        if (solid.Volume == 0) continue;

                        foreach (var obj in solid.Faces)
                        {
                            if (obj is PlanarFace face)
                            {
                                if (stack.Count == 0)
                                {
                                    stack.Push(face);
                                }
                                else
                                {
                                    if (MathRound(face.Origin.Z) < MathRound(stack.Peek().Origin.Z))
                                    {
                                        stack.Clear();
                                        stack.Push(face);
                                    }
                                    else if (MathRound(face.Origin.Z) == MathRound(stack.Peek().Origin.Z))
                                    {
                                        stack.Push(face);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return stack;
        }

        private void CreateDoorPointsOnLines(ref List<ILineParameters> lines)
        {
            int dElementIndex;
            //int pointIndex;
            XYZ locSubtResult, locAddResult, startPoint;
            List<PointOnLine> soredList;
            var qwe = GetDependencyElementsLocations();
            foreach (DependencyElement dElement in qwe)
            {
                if (Math.Abs(dElement.Location.Z - lines.First().Origin.Z) > 0.33d) continue;   // >100мм

                if (!IsDependencyElementInRoom(dElement)) continue;
                dElementIndex = SearchNearestLineIndex(lines, dElement);
                if (dElementIndex == -1) continue;

                if (IsAnyPointDefaultValue(dElement, lines[dElementIndex], out locSubtResult, out locAddResult)) continue;

                if(locSubtResult == default & locAddResult == default) continue;

                PointChecker(locSubtResult, dElementIndex, ref lines);
                PointChecker(locAddResult, dElementIndex, ref lines);

                startPoint = lines[dElementIndex].Origin;
                soredList = lines[dElementIndex].PointsOnLine.OrderBy(x => x.Point.DistanceTo(startPoint)).ToList();
                lines[dElementIndex].SetPointsOnLine(soredList);
            }
        }

        private void PointChecker(XYZ currentPoint, int index, ref List<ILineParameters> lines)
        {
            if (currentPoint != default)
            {
                if (Controller.SelectedRoom.IsPointInRoom(currentPoint + new XYZ(0, 0, 0.5d)))
                {
                    if (lines[index].PointsOnLine.FirstOrDefault(x => IsEqualPoints(x.Point, currentPoint)) == null)
                        lines[index].PointsOnLine.Add(new PointOnLine(currentPoint, EndingTypes.DoorEnding));
                }
                else
                {
                    int pointIndex;
                    pointIndex = SearchNearestPointIndex(lines[index].PointsOnLine, currentPoint);
                    if (pointIndex >= 0)
                        lines[index].PointsOnLine.RemoveAt(pointIndex);
                }
            }
        }

        #endregion

        private List<ILineParameters> CreateHostLines(List<ILineParameters> lineList)
        {
            int lastIndex;
            Stack<XYZ> stack = new Stack<XYZ>();
            List<ILineParameters> lines = new List<ILineParameters>();
            Line newLine;

            foreach (ILineParameters l in lineList)
            {
                if (stack.Count == 0)
                {
                    stack.Push(l.Direction);
                    lines.Add(l);

                    lines[0].AddToVirtualLength(l.Length);
                    lines[0].SetEndPoint();
                    lines[0].SetPointsOnLine(new List<PointOnLine>()
                    {
                        new PointOnLine(lines[0].EndPoint, EndingTypes.EMpty),
                        new PointOnLine(lines[0].Origin, EndingTypes.EMpty)
                    });
                }
                else
                {
                    if (IsEqualPoints(stack.Peek(), l.Direction))
                    {
                        lastIndex = lines.Count - 1;
                        lines[lastIndex].AddToVirtualLength(l.Length);
                        newLine = Line.CreateBound(
                            lines[lastIndex].Origin,
                            lines[lastIndex].Origin.Add(lines[lastIndex].Direction.Multiply(lines[lastIndex].VirtualLength)));
                        lines[lastIndex].SetLineParams(newLine);
                    }
                    else
                    {
                        stack.Clear();
                        stack.Push(l.Direction);
                        lines.Add(l);
                        lastIndex = lines.Count - 1;
                        lines[lastIndex].AddToVirtualLength(l.Length);
                        lines[lastIndex].SetEndPoint();
                    }

                    lines[lastIndex].SetPointsOnLine(new List<PointOnLine>()
                    {
                        new PointOnLine(lines[lastIndex].EndPoint, EndingTypes.InnerElbow),
                        new PointOnLine(lines[lastIndex].Origin, EndingTypes.EMpty)
                    });
                }
            }
            CreateDoorPointsOnLines(ref lines);
            return lines;
        }

        #endregion

        #region CreateResultLinesList

        private void ChangeEndings(List<LineForSetEndings> subLinesList, ref List<ILineParameters> linesResult)
        {
            foreach (LineForSetEndings subLine in subLinesList)
            {
                foreach (ILineParameters line in linesResult)
                {
                    if (IsEqualPoints(subLine.StartPoint, line.EndPoint))
                    {
                        switch (subLine.IsEndPointInRoom)
                        {
                            case true: line.SetEndingEnd(EndingTypes.InnerElbow); break;
                            case false: line.SetEndingEnd(EndingTypes.OuterElbow); break;
                        }
                        break;
                    }

                    if (IsEqualPoints(subLine.StartPoint, line.Origin))
                    {
                        switch (subLine.IsEndPointInRoom)
                        {
                            case true: line.SetEndingStart(EndingTypes.InnerElbow); break;
                            case false: line.SetEndingStart(EndingTypes.OuterElbow); break;
                        }
                        break;
                    }
                }
            }
        }

        private List<LineForSetEndings> CreateSubLinesForSetEndings(List<ILineParameters> linesResult, ILineParameters line1Params)
        {
            Line line1 = line1Params.Line;
            Line line2;
            XYZ line1Point0 = line1.GetEndPoint(0);
            XYZ line1Point1 = line1.GetEndPoint(1);
            XYZ line2Point0, line2Point1;
            LineForSetEndings subLine;
            List<LineForSetEndings> resultSubLines = new List<LineForSetEndings>();

            foreach (ILineParameters line2Params in linesResult)
            {
                if (line1Params.Id == line2Params.Id) continue;

                subLine = default;
                line2 = line2Params.Line;
                line2Point0 = line2.GetEndPoint(0);
                line2Point1 = line2.GetEndPoint(1);

                subLine =
                    IsEqualPoints(line2Point0, line1Point0) ? CreateSubline(line2Point0, line2Point1, line1Point0, line1Point1) :
                    IsEqualPoints(line2Point0, line1Point1) ? CreateSubline(line2Point0, line2Point1, line1Point1, line1Point0) :
                    IsEqualPoints(line2Point1, line1Point0) ? CreateSubline(line2Point1, line2Point0, line1Point0, line1Point1) :
                    IsEqualPoints(line2Point1, line1Point1) ? CreateSubline(line2Point1, line2Point0, line1Point1, line1Point0) : default;

                if (subLine != default)
                {
                    resultSubLines.Add(subLine);
                }
            }

            return resultSubLines;
        }

        private LineForSetEndings CreateSubline(XYZ line1Point1, XYZ line1Point2, XYZ _, XYZ line2Point2)
        {
            XYZ ePoint = Line.CreateBound(line1Point1, (line1Point2 + line2Point2) / 2)
                .Evaluate(1, false);
            return new LineForSetEndings(line1Point1, ePoint, Controller.SelectedRoom.IsPointInRoom(ePoint));
        }

        private List<ILineParameters> CreateResultLinesList(List<ILineParameters> lines)
        {
            int count;
            List<ILineParameters> linesResult = new List<ILineParameters>();
            foreach (ILineParameters li in lines)
            {
                count = li.PointsOnLine.Count;
                if (count <= 1) continue;

                for (int j = 1; j < count; j += 2)
                {
                    if (IsEqualPoints(li.PointsOnLine[j - 1].Point, li.PointsOnLine[j].Point)) continue;
                    
                    linesResult.Add(new LineParameters(linesResult.Count, 
                        Line.CreateBound(li.PointsOnLine[j - 1].Point, 
                        li.PointsOnLine[j].Point),
                        new EndingTypes[]
                        {
                            li.PointsOnLine[j - 1].EndingType,
                            li.PointsOnLine[j].EndingType
                        }));
                }

                if (count % 2 != 0)
                {
                    linesResult.Add(new LineParameters(linesResult.Count,
                        Line.CreateBound(li.PointsOnLine[count - 2].Point,
                        li.PointsOnLine[count - 1].Point),
                        new EndingTypes[]
                        {
                            li.PointsOnLine[count - 2].EndingType,
                            li.PointsOnLine[count - 1].EndingType
                        }));
                }
            }

            List<LineForSetEndings> subLinesList = new List<LineForSetEndings>();
            List<LineForSetEndings> newItems;
            LineForSetEndings subLine;
            foreach (ILineParameters line in linesResult)
            {
                newItems = CreateSubLinesForSetEndings(linesResult, line);
                if (newItems != default)
                {
                    if (subLinesList.Count == 0)
                    { 
                        subLinesList.AddRange(newItems);
                    }
                    foreach (LineForSetEndings item in newItems)
                    {
                        subLine = subLinesList.FirstOrDefault(x=> 
                            LineComparison(x.StartPoint, x.EndPoint, item.StartPoint, item.EndPoint));
                        if (subLine == default)
                        { 
                            subLinesList.Add(item);
                        }    
                    }
                }
            }
            ChangeEndings(subLinesList, ref linesResult);
            return linesResult;
        }

        #endregion

        #region OtherFuncs

        private bool LineComparison(XYZ line1Point0, XYZ line1Point1, XYZ line2Point0, XYZ line2Point1) =>
            (IsEqualPoints(line1Point0, line2Point0) & IsEqualPoints(line1Point1, line2Point1)) ||
            (IsEqualPoints(line1Point0, line2Point1) & IsEqualPoints(line1Point1, line2Point0));

        private bool IsEqualPoints(XYZ vector1, XYZ vector2) =>
            MathRound(vector1.X) == MathRound(vector2.X) &
            MathRound(vector1.Y) == MathRound(vector2.Y) &
            MathRound(vector1.Z) == MathRound(vector2.Z);
        
        private double MathRound(double value) => Math.Round(value, 2);

        #endregion
    }
}
