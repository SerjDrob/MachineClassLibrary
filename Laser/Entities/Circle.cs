namespace MachineClassLibrary.Laser.Entities
{
    public class Circle:IShape
    {
        public double Radius { get; set; }

        public void Scale(double scale)
        {
            Radius *= scale;
        }
    }
}
