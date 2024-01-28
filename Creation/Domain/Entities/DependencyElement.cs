using Autodesk.Revit.DB;

namespace SkirtingBoardsCreator.Creation.Domain.Entities
{
    internal class DependencyElement
    {
        public ElementId ElementId { get; }
        public Element HostObject { get; }
        public XYZ Location { get; }
        public double Width { get; }

        public DependencyElement(ElementId elementId, XYZ location, double width, Element host) 
        { 
            ElementId = elementId;
            Location = location;
            Width = width;
            HostObject = host;
        }
    }
}
