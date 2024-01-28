using Autodesk.Revit.DB;

namespace SkirtingBoardsCreator.Creation.Domain.Entities
{
    internal class LineForSetEndings
    {
        public XYZ StartPoint { get; }
        public XYZ EndPoint { get; }
        public bool IsEndPointInRoom { get; }

        public LineForSetEndings(XYZ point1, XYZ point2, bool isEndPointInRoom) 
        {
            StartPoint = point1;
            EndPoint = point2;
            IsEndPointInRoom = isEndPointInRoom;
        }
    }
}
