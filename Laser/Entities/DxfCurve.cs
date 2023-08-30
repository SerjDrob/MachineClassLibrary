using System.Windows;

namespace MachineClassLibrary.Laser.Entities
{
    public class DxfCurve : IShape
    {
        private readonly string _fullPath;
        public string FilePath
        {
            get => _fullPath;
        }

        public Rect Bounds
        {
            get; set;
        }

        public DxfCurve(string fullPath)
        {
            _fullPath = fullPath;
        }
    }
}
