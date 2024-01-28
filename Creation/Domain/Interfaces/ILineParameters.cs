using System.Collections.Generic;
using Autodesk.Revit.DB;
using SkirtingBoardsCreator.Creation.Domain.Enums;
using SkirtingBoardsCreator.Creation.Domain.Entities;

namespace SkirtingBoardsCreator.Creation.Domain.Interfaces
{
    /// <summary></summary>
    public interface ILineParameters
    {
        #region Свойства класса

        /// <summary>Идентификатор объекта.</summary>
        int Id { get; }

        /// <summary>Фактическая длина линии.</summary>
        double Length { get; }

        /// <summary>Временная длина линии.</summary>
        double VirtualLength { get; }

        /// <summary>Объект Line.</summary>
        Line Line { get; }

        /// <summary>Начальная точка линии.</summary>
        XYZ Origin { get; }

        /// <summary>Конечная точка линии.</summary>
        XYZ EndPoint { get; }

        /// <summary>Середина линии.</summary>
        XYZ MidPoint { get; }

        /// <summary>Направление линии.</summary>
        XYZ Direction { get; }

        /// <summary>Массив элементов фурнитуры.</summary>
        EndingTypes[] Endings { get; }

        /// <summary>Коллекция временных точек на линии.</summary>
        List<PointOnLine> PointsOnLine { get; }

        #endregion

        /// <summary>
        /// Изменение линии, хранящейся в объекте.
        /// </summary>
        void SetLineParams(Line line);

        /// <summary>
        /// Прибавить значение value полю VirtualLength.
        /// </summary>
        void AddToVirtualLength(double value);

        /// <summary>
        /// Изменить конечную точку линии.
        /// </summary>
        void SetEndPoint();

        /// <summary>
        /// Изменить среднюю точку линии.
        /// </summary>
        void SetMidPoint(XYZ point);

        /// <summary>
        /// Получить элемент фурнитуры, который должен быть размещен в координатах начальной точки линии.
        /// </summary>
        EndingTypes GetEndingStart();

        /// <summary>
        /// Получить элемент фурнитуры, который должен быть размещен в координатах конечной точки линии.
        /// </summary>
        EndingTypes GetEndingEnd();

        /// <summary>
        /// Изменить элемент фурнитуры, который должен быть размещен в координатах начальной точки линии.
        /// </summary>
        EndingTypes SetEndingStart(EndingTypes ending);
        /// <summary>
        /// Изменить элемент фурнитуры, который должен быть размещен в координатах конечной точки линии.
        /// </summary>
        EndingTypes SetEndingEnd(EndingTypes ending);

        /// <summary>
        /// Создать список точек.
        /// </summary>
        void SetPointsOnLine(List<PointOnLine> list);

        /// <summary>
        /// Добавить точку во внутреннюю коллекцию.
        /// </summary>
        void AddPointsOnLine(PointOnLine point);

        /// <summary>
        /// Отзеркалить фурнитуру дверных проемов.
        /// </summary>
        void FlipDoorEndings();
        
        /// <summary></summary>
        IParameterForChanging[] CreateParametersList();
    }
}
