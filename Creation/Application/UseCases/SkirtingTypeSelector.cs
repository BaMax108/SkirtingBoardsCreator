using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using SkirtingBoardsCreator.Creation.Application.Repositories;
using SkirtingBoardsCreator.Creation.Controllers;
using SkirtingBoardsCreator.Creation.Domain.Enums;
using SkirtingBoardsCreator.Creation.Domain.Entities;

namespace SkirtingBoardsCreator.Creation.Application.UseCases
{
    internal class SkirtingTypeSelector
    {
        private ISkBoardsController Controller { get; }
        private SkirtingType CurrentSkirtingBoard;
        private SkirtingData CurrentSkirtingData;
        /// <summary></summary>
        public SkirtingTypeSelector(ISkBoardsController controller) => Controller = controller;
        
        /// <summary></summary>
        public SkirtingData GetSkirtingTypeData() => CurrentSkirtingData;

        /// <summary>
        /// Поиск типа семейства, на основе названия и типа помещения.
        /// </summary>
        public FamilySymbol SymbolSearching()
        {
            FamilySymbol symbol = default;
            string roomName = GetRoomName();

            if (roomName == string.Empty) return symbol;
            
            RoomParameters isNameContains = new RoomsData().Get(roomName);
            CurrentSkirtingBoard = isNameContains == default ? SkirtingType.Unknown : isNameContains.SkirtingType;

            IEnumerable<FamilySymbol> filter = new FilteredElementCollector(Controller.Doc)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .WhereElementIsElementType()
                .ToElements()
                .Where(x => x is FamilySymbol)
                .Cast<FamilySymbol>();

            CurrentSkirtingData = new SkirtingBoardsData().SkirtingBoardsDictionary[CurrentSkirtingBoard];

            if (filter.Count() != 0 & CurrentSkirtingData != default)
            {
                symbol = CurrentSkirtingBoard == SkirtingType.Unknown ? default : filter
                    .FirstOrDefault(y => y.FamilyName == CurrentSkirtingData.FamilyName & y.Name == CurrentSkirtingData.SymbolName);
            }

            return symbol;
        }

        public bool IsSkirtingTypePvc() => CurrentSkirtingData.Material == SkirtingMaterial.Pvc;

        private string GetRoomName() => GetParameterValue(Controller.SelectedRoom.get_Parameter(BuiltInParameter.ROOM_NAME));

        private string GetParameterValue(Parameter parameter) => parameter == null ? string.Empty : parameter.AsString();
    }
}
