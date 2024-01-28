using SkirtingBoardsCreator.Creation.Domain.Enums;

namespace SkirtingBoardsCreator.Creation.Domain.Entities
{
    /// <summary></summary>
    public class RoomParameters
    {
        /// <summary></summary>
        public SkirtingType SkirtingType { get; }
        
        /// <summary></summary>
        public string RoomName { get; }
        
        /// <summary></summary>
        public RoomParameters(SkirtingType skirtingType, string roomName)
        {
            SkirtingType = skirtingType;
            RoomName = roomName;
        }
    }
}
