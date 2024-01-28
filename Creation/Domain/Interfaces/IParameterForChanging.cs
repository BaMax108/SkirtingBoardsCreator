using SkirtingBoardsCreator.Creation.Domain.Enums;

namespace SkirtingBoardsCreator.Creation.Domain.Interfaces
{
    /// <summary></summary>
    public interface IParameterForChanging
    {
        /// <summary></summary>
        string Name { get; }
        /// <summary></summary>
        EndingTypes Value { get; }
    }
}
