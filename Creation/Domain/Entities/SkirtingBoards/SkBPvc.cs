using SkirtingBoardsCreator.Creation.Domain.Enums;
using System.Collections.Generic;

namespace SkirtingBoardsCreator.Creation.Domain.Entities.SkirtingBoards
{
    internal class SkBPvc
    {
        public string FamilyName { get; } = "CGN_Плинтус_сборный_ПВХ";
        public bool HasFurniture { get; } = true;

        public SkirtingMaterial Material { get; } = SkirtingMaterial.Pvc;

        public string GetSymbolName(SkirtingType st) => Symbols.ContainsKey(st) ? Symbols[st] : default;

        private Dictionary<SkirtingType, string> Symbols { get; } = new Dictionary<SkirtingType, string>()
        {
            { SkirtingType.PL1,"ПЛ-1" }
        };
    }
}
