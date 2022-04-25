namespace MachineClassLibrary.Laser.Entities
{
    public class Point:IShape
    {
        public double X { get; set; }
        public double Y { get; set; }

        public void Scale(double scale)
        {
            throw new System.NotImplementedException();
        }
    }
}
