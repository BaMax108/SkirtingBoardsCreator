using SkirtingBoardsCreator.Creation.Domain.Enums;
using System.Collections.Generic;

namespace SkirtingBoardsCreator.Creation.Domain.Entities.SkirtingBoards
{
    internal class SkBCeramic
    {
        public string FamilyName { get; } = "CGN_Плинтус_Площадка";
        public bool HasFurniture { get; } = false;

        public SkirtingMaterial Material { get; } = SkirtingMaterial.Ceramic;

        public string GetSymbolName(SkirtingType st) => Symbols.ContainsKey(st) ? Symbols[st] : default;

        private Dictionary<SkirtingType, string> Symbols { get; } = new Dictionary<SkirtingType, string>()
        {
            { SkirtingType.PL2, "ПЛ-2" },
            { SkirtingType.PL3, "ПЛ-3" },
            { SkirtingType.PL4, "ПЛ-4" },
            { SkirtingType.PL5, "ПЛ-5" },
            { SkirtingType.PL6, "ПЛ-6" }
        };
    }
}
