using System;
using System.Collections.Generic;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using SkirtingBoardsCreator.Creation.Controllers;
using SkirtingBoardsCreator.Creation.Domain.Interfaces;

namespace SkirtingBoardsCreator
{
    /// <summary></summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SkBoardsCreatorCommand : IExternalCommand
    {
        private UIApplication UIApp { get; set; }
        private UIDocument UIDoc => UIApp.ActiveUIDocument;
        private Document Doc => UIDoc.Document;

        /// <summary>
        /// Overload this method to implement and external command within Revit.
        /// </summary>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApp = new UIApplication(commandData.Application.Application);

            Reference r;
            RoomsSelectionFilter selectionFilter = new RoomsSelectionFilter(Doc);
            try
            {
                r = UIDoc.Selection.PickObject(ObjectType.PointOnElement,
                    selectionFilter, 
                    "Выберите помещение в связанном файле.");
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception)
            {
                return Result.Failed;
            }

            SkBoardsController controller = new SkBoardsController(selectionFilter.PickedRoom, Doc.GetElement(r), Doc);
            List<ILineParameters> list = controller.GetHostLinesList();
            if (list != null)
            { 
                foreach (ILineParameters item in list)
                {
                    controller.CreateSkirtingBoard(item);
                }
            }
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return Result.Succeeded;
        }

        /// <summary></summary>
        public class RoomsSelectionFilter : ISelectionFilter
        {
            readonly private Document doc;

            /// <summary></summary>
            /// <param name="doc"></param>
            public RoomsSelectionFilter(Document doc) => this.doc = doc;
            
            /// <summary>
            /// Выбранное помещение.
            /// </summary>
            public Room PickedRoom { get; private set; }

            /// <summary></summary>
            public bool AllowElement(Element e) => e is RevitLinkInstance;

            /// <summary></summary>
            public bool AllowReference(Reference reference, XYZ point)
            {
                if ((doc.GetElement(reference) as RevitLinkInstance).GetLinkDocument()
                    .GetElement(reference.LinkedElementId) is Room room)
                {
                    PickedRoom = room;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}