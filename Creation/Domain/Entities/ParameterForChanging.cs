using SkirtingBoardsCreator.Creation.Domain.Enums;
using SkirtingBoardsCreator.Creation.Domain.Interfaces;

namespace SkirtingBoardsCreator.Creation.Domain.Entities
{
    internal class ParameterForChanging : IParameterForChanging
    {
        public string Name { get; }
        public EndingTypes Value { get; }

        public ParameterForChanging(string name, EndingTypes value)
        {
            Name = name;
            Value = value;
        }
    }
}
