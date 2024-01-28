using SkirtingBoardsCreator.Creation.Domain.Enums;
using SkirtingBoardsCreator.Creation.Domain.Entities;
using System.Linq;

namespace SkirtingBoardsCreator.Creation.Application.Repositories
{
    /// <summary></summary>
    public class RoomsData
    {
        private RoomParameters[] RoomsDictionary { get; } = new RoomParameters[]
        {
            new RoomParameters(SkirtingType.PL1,"Жилая комната"),
            new RoomParameters(SkirtingType.PL1, "Кухня"),
            new RoomParameters(SkirtingType.PL1, "Кухня-гостинная"),
            new RoomParameters(SkirtingType.PL1, "Кухня-ниша"),
            new RoomParameters(SkirtingType.Unknown, "С/У"),
            new RoomParameters(SkirtingType.Unknown, "Ванная комната"),
            new RoomParameters(SkirtingType.Unknown, "Туалет"),
            new RoomParameters(SkirtingType.PL1, "Холл"),
            new RoomParameters(SkirtingType.Unknown, "Кладовая"),
            new RoomParameters(SkirtingType.PL5, "Летняя лоджия"),
            new RoomParameters(SkirtingType.PL5, "Летний балкон"),
            new RoomParameters(SkirtingType.PL2, "МОП - Переходная лоджия"),
            new RoomParameters(SkirtingType.PL2, "МОП - Переходной балкон"),
            new RoomParameters(SkirtingType.Unknown, "БКТ (Ф4.3)"),
            new RoomParameters(SkirtingType.Unknown, "Венткамера вытяжная"),
            new RoomParameters(SkirtingType.Unknown, "Венткамера приточная"),
            new RoomParameters(SkirtingType.Unknown, "Вестибюль"),
            new RoomParameters(SkirtingType.Unknown, "Зона, свободная от пожарной нагрузки"),
            new RoomParameters(SkirtingType.Unknown, "ИТП"),
            new RoomParameters(SkirtingType.Unknown, "Колясочная"),
            new RoomParameters(SkirtingType.PL2, "Лестничная клетка"),
            new RoomParameters(SkirtingType.PL4, "Лестничная клетка технического этажа"),
            new RoomParameters(SkirtingType.PL2, "Лифтовой холл (зона безопасности для МГН)"),
            new RoomParameters(SkirtingType.Unknown, "Машинное отделение лифтов"),
            new RoomParameters(SkirtingType.PL2, "Межквартирный коридор"),
            new RoomParameters(SkirtingType.Unknown, "Мусоросборная камера"),
            new RoomParameters(SkirtingType.Unknown, "Насосная АУПТ"),
            new RoomParameters(SkirtingType.Unknown, "Насосная водоснабжения"),
            new RoomParameters(SkirtingType.Unknown, "ПУИ"),
            new RoomParameters(SkirtingType.Unknown, "ПУТ"),
            new RoomParameters(SkirtingType.PL4, "Помещение КП МПТЦ"),
            new RoomParameters(SkirtingType.PL4, "Помещение СС"),
            new RoomParameters(SkirtingType.Unknown, "Помещение автостоянки"),
            new RoomParameters(SkirtingType.Unknown, "Помещение для ствола мусоропровода"),
            new RoomParameters(SkirtingType.PL2, "Помещение консъержа"),
            new RoomParameters(SkirtingType.Unknown, "Пространство для ствола мусоропровода"),
            new RoomParameters(SkirtingType.PL6, "Рабочая зона на 2 раб. места"),
            new RoomParameters(SkirtingType.Unknown, "Рампа"),
            new RoomParameters(SkirtingType.Unknown, "С/У (МОП)"),
            new RoomParameters(SkirtingType.Unknown, "С/У для персонала"),
            new RoomParameters(SkirtingType.Unknown, "С/у доступный для МГН"),
            new RoomParameters(SkirtingType.PL6, "Служебное помещение (комната приема пищи персонала)"),
            new RoomParameters(SkirtingType.Unknown, "Тамбур"),
            new RoomParameters(SkirtingType.Unknown, "Тамбур БКТ"),
            new RoomParameters(SkirtingType.Unknown, "Тамбур-шлюз"),
            new RoomParameters(SkirtingType.PL2, "Тамбур-шлюз (зона безопасности для МГН)"),
            new RoomParameters(SkirtingType.Unknown, "Техническое помещение для прокладки инженерных коммуникаций"),
            new RoomParameters(SkirtingType.PL4, "Узел учета тепла"),
            new RoomParameters(SkirtingType.PL2, "Универсальный С/У"),
            new RoomParameters(SkirtingType.Unknown, "Форкамера"),
            new RoomParameters(SkirtingType.PL6, "ЦИН"),
            new RoomParameters(SkirtingType.PL4, "Электрощитовая автостоянки"),
            new RoomParameters(SkirtingType.PL4, "Электрощитовая жилой части"),
            new RoomParameters(SkirtingType.PL4, "Электрощитовая нежилой части"),
            new RoomParameters(SkirtingType.PL4, "Помещение охраны"),
            new RoomParameters(SkirtingType.PL2, "Лифтовой холл"),
            new RoomParameters(SkirtingType.PL4, "Помещение узла учёта тепла"),
            new RoomParameters(SkirtingType.PL5, "Лоджия"),
            new RoomParameters(SkirtingType.PL5, "Балкон"),
        };

        /// <summary></summary>
        public RoomParameters Get(string roomName)
        {
            if (roomName == null) return null;
            return RoomsDictionary.FirstOrDefault(x => x.RoomName.Contains(roomName));
        }
    }
}
