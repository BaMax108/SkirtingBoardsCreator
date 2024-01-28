using SkirtingBoardsCreator.Creation.Domain.Entities;
using SkirtingBoardsCreator.Creation.Domain.Entities.SkirtingBoards;
using System.Collections.Generic;
using SkBCeramic = SkirtingBoardsCreator.Creation.Domain.Entities.SkirtingBoards.SkBCeramic;
using SkType = SkirtingBoardsCreator.Creation.Domain.Enums.SkirtingType;

namespace SkirtingBoardsCreator.Creation.Application.Repositories
{
    internal class SkirtingBoardsData
    {
        public SkirtingBoardsData() => SkirtingBoardsDictionary = CreateDictionary();

        private Dictionary<SkType, SkirtingData> CreateDictionary()
        {
            SkBCeramic ceramic = new SkBCeramic();
            SkBPvc pvc = new SkBPvc();

            return new Dictionary<SkType, SkirtingData>()
            {
                { SkType.PL1, new SkirtingData(pvc.FamilyName,pvc.GetSymbolName(SkType.PL1), pvc.HasFurniture, pvc.Material) },
                { SkType.PL2, new SkirtingData(ceramic.FamilyName, ceramic.GetSymbolName(SkType.PL2), ceramic.HasFurniture, ceramic.Material) },
                { SkType.PL3, new SkirtingData(ceramic.FamilyName, ceramic.GetSymbolName(SkType.PL3), ceramic.HasFurniture, ceramic.Material) },
                { SkType.PL4, new SkirtingData(ceramic.FamilyName, ceramic.GetSymbolName(SkType.PL4), ceramic.HasFurniture, ceramic.Material) },
                { SkType.PL5, new SkirtingData(ceramic.FamilyName, ceramic.GetSymbolName(SkType.PL5), ceramic.HasFurniture, ceramic.Material) },
                { SkType.PL6, new SkirtingData(ceramic.FamilyName, ceramic.GetSymbolName(SkType.PL6), ceramic.HasFurniture, ceramic.Material) },
                { SkType.Unknown, default },
            };
        }

        public Dictionary<SkType, SkirtingData> SkirtingBoardsDictionary { get; }
    }
}
