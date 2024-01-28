using System.Collections.Generic;
using Autodesk.Revit.DB;
using SkirtingBoardsCreator.Creation.Domain.Interfaces;
using SkirtingBoardsCreator.Creation.Domain.Enums;
using LineRvt = Autodesk.Revit.DB.Line;
using System;

namespace SkirtingBoardsCreator.Creation.Domain.Entities
{
    /// <summary></summary>
    public class LineParameters : ILineParameters
    {
        #region Свойства класса

        /// <summary>Идентификатор объекта.</summary>
        public int Id { get; }

        /// <summary>Фактическая длина линии.</summary>
        public double Length { get; private set; }

        /// <summary>Временная длина линии.</summary>
        public double VirtualLength { get; private set; }

        /// <summary>Объект Line.</summary>
        public LineRvt Line { get; private set; }

        /// <summary>Начальная точка линии.</summary>
        public XYZ Origin { get; private set; }

        /// <summary>Конечная точка линии.</summary>
        public XYZ EndPoint { get; private set; }

        /// <summary>Середина линии.</summary>
        public XYZ MidPoint { get; private set; }

        /// <summary>Направление линии.</summary>
        public XYZ Direction { get; private set; }

        /// <summary>Коллекция временных точек на линии.</summary>
        public List<PointOnLine> PointsOnLine { get; private set; }
        
        /// <summary>Массив элементов фурнитуры.</summary>
        public EndingTypes[] Endings { get; private set; }

        #endregion

        /// <summary>Конструктор класса LineParameters.</summary>
        public LineParameters(int id, LineRvt line)
        {
            Id = id;
            SetLineParams(line);
        }

        /// <summary>Конструктор класса LineParameters.</summary>
        public LineParameters(int id, LineRvt line, EndingTypes[] endings)
        {
            Id = id;
            SetLineParams(line);

            Endings = new EndingTypes[2];
            
            SetEndingStart(endings[0] == EndingTypes.DoorEnding || 
                endings[0] == EndingTypes.DoorEndingMirrored ?
                endings[0] : EndingTypes.EMpty);
            
            SetEndingEnd(endings[1] == EndingTypes.DoorEnding || 
                endings[1] == EndingTypes.DoorEndingMirrored ?
                endings[1] : EndingTypes.EMpty);
        }

        /// <summary>
        /// Изменение линии, хранящейся в объекте.
        /// </summary>
        public void SetLineParams(LineRvt line)
        {
            Direction = line.Direction;
            Origin = line.Origin;
            Length = line.Length;
            var endPoint1 = line.GetEndPoint(1);
            EndPoint = IsEqualPoints(endPoint1, Origin) ? line.GetEndPoint(0) : endPoint1;
            Line = LineRvt.CreateBound(Origin, EndPoint);
        }

        private bool IsEqualPoints(XYZ vector1, XYZ vector2) =>
            Math.Round(vector1.X,3) == Math.Round(vector2.X,3) &
            Math.Round(vector1.Y,3) == Math.Round(vector2.Y,3) &
            Math.Round(vector1.Z,3) == Math.Round(vector2.Z,3);

        /// <summary>
        /// Прибавить значение value полю VirtualLength.
        /// </summary>
        public void AddToVirtualLength(double value) => VirtualLength += value;

        /// <summary>
        /// Изменить конечную точку линии.
        /// </summary>
        public void SetEndPoint() => Line.GetEndPoint(1);

        /// <summary>
        /// Изменить среднюю точку линии.
        /// </summary>
        public void SetMidPoint(XYZ point) => MidPoint = Line.Project(point).XYZPoint;

        /// <summary>
        /// Получить элемент фурнитуры, который должен быть размещен в координатах начальной точки линии.
        /// </summary>
        public EndingTypes GetEndingStart() => Endings[0];

        /// <summary>
        /// Получить элемент фурнитуры, который должен быть размещен в координатах конечной точки линии.
        /// </summary>
        public EndingTypes GetEndingEnd() => Endings[1];

        /// <summary>
        /// Изменить элемент фурнитуры, который должен быть размещен в координатах начальной точки линии.
        /// </summary>
        public EndingTypes SetEndingStart(EndingTypes ending) => Endings[0] = ending;
        /// <summary>
        /// Изменить элемент фурнитуры, который должен быть размещен в координатах конечной точки линии.
        /// </summary>
        public EndingTypes SetEndingEnd(EndingTypes ending) => Endings[1] = ending;

        /// <summary>
        /// Создать список точек.
        /// </summary>
        public void SetPointsOnLine(List<PointOnLine> list) => PointsOnLine = list;

        /// <summary>
        /// Добавить точку во внутреннюю коллекцию.
        /// </summary>
        public void AddPointsOnLine(PointOnLine point) => PointsOnLine.Add(point);

        /// <summary>
        /// Отзеркалить фурнитуру дверных проемов.
        /// </summary>
        public void FlipDoorEndings()
        {
            if (Endings[0] == EndingTypes.DoorEnding)
            {
                SetEndingStart(EndingTypes.DoorEndingMirrored);
            }
            if (Endings[1] == EndingTypes.DoorEnding)
            {
                SetEndingEnd(EndingTypes.DoorEndingMirrored);
            }
        }

        /// <summary>
        /// Создать список, содержащий данные для изменения параметров семейства.
        /// </summary>
        public IParameterForChanging[] CreateParametersList() =>
            new IParameterForChanging[]
            {
                    new ParameterForChanging("Начало", GetEndingStart()),
                    new ParameterForChanging("Окончание", GetEndingEnd())
            };
    }
}
