using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using SkirtingBoardsCreator.Creation.Domain.Interfaces;
using System.Collections.Generic;

namespace SkirtingBoardsCreator.Creation.Controllers
{
    internal interface ISkBoardsController
    {
        /// <summary>Текущая открытая модель.</summary>
        Document Doc { get; }

        /// <summary>Связанный файл.</summary>
        Document DocLink { get; }

        Element PickedElement { get; }
        /// <summary>Выбранная комната.</summary>
        Room SelectedRoom { get; }

        /// <summary>Получение списка линий.</summary>
        List<ILineParameters> GetHostLinesList();

        /// <summary>Создание экземпляра плинтуса.</summary>
        void CreateSkirtingBoard(ILineParameters lineType);
    }
}
