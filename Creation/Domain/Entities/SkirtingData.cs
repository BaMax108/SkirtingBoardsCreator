using SkirtingBoardsCreator.Creation.Domain.Enums;

namespace SkirtingBoardsCreator.Creation.Domain.Entities
{
    internal class SkirtingData
    {
        public string FamilyName { get; }
        public string SymbolName { get; }
        public bool HasFurniture { get; }
        public SkirtingMaterial Material { get; }

        public SkirtingData(string name, string symbol, bool hasFurniture, SkirtingMaterial material)
        { 
            FamilyName = name;
            SymbolName = symbol;
            HasFurniture = hasFurniture;
            Material = material;
        }
    }
}
