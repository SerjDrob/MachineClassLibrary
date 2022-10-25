using System.Windows;

namespace MachineClassLibrary.Laser.Entities
{
    public class Circle:IShape
    {
        public double Radius { get; set; }

        public Rect Bounds { get; init; }

        //public void Scale(double scale)
        //{
        //    Radius *= scale;
        //}

        //public void SetMirrorX(bool mirror)
        //{
        //  //  throw new System.NotImplementedException();
        //}

        //public void SetTurn90(bool turn)
        //{
        // //   throw new System.NotImplementedException();
        //}
    }
}
