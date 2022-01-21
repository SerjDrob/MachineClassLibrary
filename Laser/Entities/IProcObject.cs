
using System;

namespace MachineClassLibrary.Laser.Entities
{
    public interface IProcObject<TObject>
    {
        public int ARGBColor { get; set; }
        public string LayerName { get; set; }
        public double X { get; init; }
        public double Y { get; init; }
        public double Angle { get; init; }
        public TObject PObject { get; init; }
        public (double x, double y) GetSize();
        public IProcObject<TObject> CloneWithPosition(double x, double y);
    }
}
