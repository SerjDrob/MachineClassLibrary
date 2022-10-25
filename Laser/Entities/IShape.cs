using System.Windows;

namespace MachineClassLibrary.Laser.Entities
{
    public interface IShape
    {
        Rect Bounds { get; }
        //void Scale(double scale);
        //void SetMirrorX(bool mirror);
        //void SetTurn90(bool turn);
    }
}
