using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace SkirtingBoardsCreator.Creation.Application.UseCases
{
    internal class RoomFaceChecker
    {
        public PlanarFace CurrentPlanarFace { get; private set; }

        private Document Doc { get; }
        private Document LinkDoc { get; }

        public RoomFaceChecker(Document document, Document linkDoc)
        { 
            Doc = document;
            LinkDoc = linkDoc;
        }

        public Floor SearchFloorInRoom(Room SelectedRoom, Level lvl)
        {
            if(lvl == default || SelectedRoom == default) return default;

            CurrentPlanarFace = SearchRoomBottomFace(
                SelectedRoom.get_Geometry(new Options()));
            if (CurrentPlanarFace == default) return default;

            BoundingBoxXYZ roomBox = SelectedRoom.get_BoundingBox(Doc.ActiveView);
            IList<Element> list = new FilteredElementCollector(LinkDoc)
                .OfCategory(BuiltInCategory.OST_Floors)
                .WherePasses(new BoundingBoxIntersectsFilter(new Outline(roomBox.Min, roomBox.Max), false))
                .ToElements();

            double levelElevation = lvl.Elevation;
            double roomHeight = SelectedRoom.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET).AsDouble();
            double elevationAndHalfHeight = levelElevation + roomHeight / 2;
            Floor floorInRoom = default;
            foreach (Element element in list)
            {
                if (element is Floor floor)
                {
                    foreach (GeometryObject geomElement in floor.get_Geometry(new Options()))
                    {
                        if (geomElement is Solid solid)
                        {
                            foreach (object obj in solid.Faces)
                            {
                                if (obj is PlanarFace pFace)
                                {
                                    if (Math.Round(pFace.FaceNormal.Z,3) == 1)
                                    {
                                        if (CurrentPlanarFace == default)
                                        {
                                            CurrentPlanarFace = pFace;
                                        }
                                        else
                                        {
                                            if (pFace.Origin.Z < CurrentPlanarFace.Origin.Z & pFace.Origin.Z < elevationAndHalfHeight)
                                            {
                                                CurrentPlanarFace = pFace;

                                                if (floorInRoom == null || floor.Id != floorInRoom.Id)
                                                {
                                                    floorInRoom = floor;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return floorInRoom;
        }

        public PlanarFace SearchRoomBottomFace(GeometryElement geometryElement)
        {
            if (geometryElement == default) return default;

            PlanarFace pf = default;
            foreach (GeometryObject geomItem in geometryElement)
            {
                if (geomItem is Solid solid)
                {
                    foreach (object edgeItem in solid.Faces)
                    {
                        if (edgeItem is PlanarFace face)
                        {
                            if (Math.Round(face.FaceNormal.Z, 3) == 1)
                            {
                                if (pf == default)
                                {
                                    pf = face;
                                }
                                else
                                {
                                    if (face.Origin.Z < pf.Origin.Z)
                                    {
                                        pf = face;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return pf;
        }
    }
}
