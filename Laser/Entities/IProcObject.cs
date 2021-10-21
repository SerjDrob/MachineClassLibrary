
namespace MachineClassLibrary.Laser.Entities
{
    public interface IProcObject<TObject>
    {
        public string LayerName { get; set; }
        public double X { get; init; }
        public double Y { get; init; }
        public double Angle { get; init; }
        public TObject PObject { get; init; }
        public (double x, double y) GetSize();
        //object
    }
}
