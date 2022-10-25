using System.Windows;

namespace MachineClassLibrary.Laser.Entities
{
    public class Point:IShape
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Rect Bounds { get; init; }

        //public void Scale(double scale)
        //{
        //   // throw new System.NotImplementedException();
        //}

        //public void SetMirrorX(bool mirror)
        //{
        //   // throw new System.NotImplementedException();
        //}

        //public void SetTurn90(bool turn)
        //{
        //   // throw new System.NotImplementedException();
        //}
    }
}
