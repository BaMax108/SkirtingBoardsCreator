using Autodesk.Revit.DB;
using SkirtingBoardsCreator.Creation.Domain.Enums;

namespace SkirtingBoardsCreator.Creation.Domain.Entities
{
    /// <summary></summary>
    public class PointOnLine
    {
        /// <summary>Координаты точки.</summary>
        public XYZ Point { get; set; }

        /// <summary>Тип фурнитуры.</summary>
        public EndingTypes EndingType { get; set; }

        /// <summary>Конструктор класса PointOnLine.</summary>
        public PointOnLine(XYZ point, EndingTypes endingType)
        { 
            Point = point;
            EndingType = endingType;
        }
    }
}
