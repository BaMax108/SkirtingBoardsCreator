using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using SkirtingBoardsCreator.Creation.Domain.Interfaces;
using SkirtingBoardsCreator.Creation.Application.UseCases;

namespace SkirtingBoardsCreator.Creation.Controllers
{
    /// <summary></summary>
    public class SkBoardsController : ISkBoardsController
    {
        private ISkBoardsController Controller { get; }

        /// <summary>Текущая открытая модель.</summary>
        public Document Doc { get; }

        /// <summary>Связанный файл.</summary>
        public Document DocLink { get; }
        
        /// <summary>Выбранная комната.</summary>
        public Room SelectedRoom { get; }
        /// <summary></summary>
        public Element PickedElement { get; }
        /// <summary></summary>
        public PlanarFace CurrentPlanarFace { get; set; }

        private readonly Transactions Transactions;
        private readonly SkirtingTypeSelector SkirtingSelector;
        private readonly Floor FloorInRoom;
        private readonly Level CurrentLevel;
        private FamilySymbol CurrentSymbol = default;

        /// <summary>Конструктор класса SkBoardsController.</summary>
        public SkBoardsController(Room room, Element pickedElement, Document doc)
        {
            Controller = this;
            SelectedRoom = room;
            PickedElement = pickedElement;
            SkirtingSelector = new SkirtingTypeSelector(Controller);
            if (pickedElement is RevitLinkInstance)
            {
                DocLink = (PickedElement as RevitLinkInstance).GetLinkDocument();
            }
            
            Doc = doc;
            Transactions = new Transactions(Controller);
            CurrentLevel = SetCurrentLevel();
            RoomFaceChecker rfc = new RoomFaceChecker(Doc, DocLink);
            FloorInRoom = rfc.SearchFloorInRoom(SelectedRoom, CurrentLevel);

            CurrentPlanarFace = rfc.CurrentPlanarFace;
        }

        /// <summary>
        /// Получение списка линий.
        /// </summary>
        public List<ILineParameters> GetHostLinesList()
        {
            CurrentSymbol = SkirtingSelector.SymbolSearching();
            return CurrentSymbol == default ? default : new HostLinesCreation(Controller).CreateHostLines();
        }

        /// <summary>
        /// Создание экземпляра плинтуса.
        /// </summary>
        public void CreateSkirtingBoard(ILineParameters lineType)
        {
            if (CurrentLevel == default) return;
            if (Math.Round(lineType.Line.Length, 3) == 0) return;
            if (CreateInstanceTransaction(out FamilyInstance instance, lineType.Line) != Result.Succeeded) return;
            if (FloorInRoom != null)
            { 
                ChangeAtributeTransaction(instance, 
                    BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM, 
                    FloorInRoom.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).AsValueString());
            }
            
            FacingFlipTransaction(instance, lineType);
            
            if (SkirtingSelector.IsSkirtingTypePvc())
            {
                ChangeParametersTransaction(instance, lineType.CreateParametersList());
            }

            if((instance.Location as LocationCurve).Curve.Length < 0.164042d)
            {
                ChangeAtributeTransaction(instance, BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS, "Длина меньше 5 см");
            }    
            
        }

        #region Транзакции

        private Result CreateInstanceTransaction(out FamilyInstance instance, Line line) => 
            Transactions.CreateInstance(out instance, line, CurrentSymbol, CurrentLevel);

        private void FacingFlipTransaction(FamilyInstance instance, ILineParameters lineType)
        {
            XYZ centroid = GetCentroid(instance);
            if (centroid == default) return;
            XYZ centroidProject = ((instance.Location as LocationCurve).Curve as Line).Project(centroid).XYZPoint;
            Line tempLine = Line.CreateBound(new XYZ(centroid.X, centroid.Y, centroidProject.Z), centroidProject);

            Line newLine = tempLine.CreateTransformed(new Transform((Controller.PickedElement as RevitLinkInstance)
                .GetTotalTransform().Inverse)) as Line;

            XYZ pElev = new XYZ(0,0,1d + (PickedElement as RevitLinkInstance)
                .GetTotalTransform().Origin.Z);

            if (SelectedRoom.IsPointInRoom(newLine.GetEndPoint(0) + pElev) != true)
            { 
                Transactions.FacingFlip(instance);
                lineType.FlipDoorEndings();
            }
        }

        private void ChangeAtributeTransaction(FamilyInstance instance, BuiltInParameter bip, string value) =>
            Transactions.ChangingParam(instance, bip, value);

        private void ChangeParametersTransaction(FamilyInstance instance, IParameterForChanging[] parameters) =>
            Transactions.ChangeParameters(instance, parameters, SkirtingSelector.GetSkirtingTypeData().HasFurniture);

        #endregion

        private XYZ GetCentroid(FamilyInstance instance)
        {
            XYZ centroid = default;
            Stack<XYZ> stackXyz = new Stack<XYZ>();
            foreach (GeometryObject gObj in instance.get_Geometry(new Options()))
            {
                if (gObj is GeometryInstance gInstance)
                {
                    foreach (GeometryObject geom in gInstance.GetInstanceGeometry())
                    {
                        if (geom is Solid solid)
                        {
                            if (solid.SurfaceArea == 0 || solid.Faces.Size < 3) continue;
                            centroid = solid.ComputeCentroid();
                            if (stackXyz.Count == 0)
                            {
                                stackXyz.Push(centroid);
                            }
                            else if (stackXyz.Peek().Z > centroid.Z) continue;
                            else
                            {
                                stackXyz.Clear();
                                stackXyz.Push(centroid);
                            }
                        }
                    }
                }
            }
            return centroid;
        }

        private Level SetCurrentLevel() =>
             new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_Levels)
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<Level>()
                .ToArray()
                .FirstOrDefault(x => Math.Round(x.Elevation, 3) == Math.Round(SelectedRoom.Level.Elevation, 3));
    }
}
