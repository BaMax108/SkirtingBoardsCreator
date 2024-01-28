using Autodesk.Revit.DB;
using Line = Autodesk.Revit.DB.Line;

namespace SkirtingBoardsCreator.Creation.Domain.Entities
{
    internal class DependencyWindow : DependencyElement
    {
        public string Name { get; private set; }
        public bool IsWindow { get; private set; }
        public bool IsSymetrical { get; private set; }
        public XYZ Point0 { get; private set; }
        public XYZ Point1 { get; private set; }
        public XYZ PointMid { get; private set; }

        private double WindowWidth, LeftOffset, RightOffset, FrameProfileWidth, DoorWidth;

        public DependencyWindow(ElementId elementId, XYZ location, double width, Element host) :
            base(elementId, location, width, host) { }

        public void SetSymetricType(bool value) => IsSymetrical = value;
        public void SetPointMid(XYZ point) => this.PointMid = point;

        public void SetFields(FamilyInstance instance)
        {
            Name = instance.Symbol.FamilyName;

            switch (instance.Category.Id.IntegerValue)
            {
                case -2000023: // Doors
                    IsWindow = false;
                    WindowWidth = GetParamValue(instance, "Ширина окна");
                    break;
                case -2000014: // Windows
                    IsWindow = true;
                    LeftOffset = GetParamValue(instance, "Монтажный зазор Слева_Расчет");
                    RightOffset = GetParamValue(instance, "Монтажный зазор Справа_Расчет");
                    FrameProfileWidth = GetParamValue(instance,"Рама профиль Ширина");
                    DoorWidth = GetParamValue(instance, "Створка_Дверь_Ширина_Расчет");
                    break;
                default: break;
            }
        }

        private double GetParamValue(FamilyInstance instance, string value)
        {
            double result = GetParam(instance.Symbol.LookupParameter(value));
            if (result == 0)
                result = GetParam(instance.LookupParameter(value));
            return result;
        }
        
        private double GetParam(Parameter p) => p == default ? default : p.AsDouble();

        public void SetPoints(Line line)
        {
            if (IsSymetrical) PointMid = this.Location;

            SetPoint0(line);
            SetPoint1(line);
        }
        
        private void SetPoint0(Line line) =>
            Point0 = IsWindow ?
                EvaluateCustom(line, this.DoorWidth / 2 + FrameProfileWidth + RightOffset) :
                EvaluateCustom(line, + (this.Width - WindowWidth) / 2);

        private void SetPoint1(Line line) =>
            Point1 = IsWindow ?
                EvaluateCustom(line, - this.DoorWidth / 2 - FrameProfileWidth - LeftOffset) :
                EvaluateCustom(line, - (this.Width - WindowWidth) / 2);

        private XYZ EvaluateCustom(Line line, double value) =>
            line.Evaluate(
                line.Origin.DistanceTo(
                    line.Project(this.PointMid).XYZPoint) + value, false);
    }
}
