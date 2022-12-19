using MachineClassLibrary.Classes;
using MachineClassLibrary.Laser.Entities;
using System;
using System.IO;

namespace MachineClassLibrary.Laser
{
    public class EntityFileHandler : IDisposable
    {
        private readonly IDxfReader _dxfReader;
        private readonly string _folderPath;
        private string _path;
        const string CURVE_PREFIX = "Curve_";
        const string CIRCLE_PREFIX = "Circle_";
        public string FilePath { get => _path; }
        public EntityFileHandler(IDxfReader dxfReader, string folderPath)
        {
            _dxfReader = dxfReader;
            _folderPath = folderPath;
        }
        ~EntityFileHandler() => Dispose();
        public EntityFileHandler SaveEntityToFile(IShape shape)
        {
            _path = shape switch
            {
                Curve curve => WriteTransformCurve(curve),
                Circle circle => WriteTransformCircle(circle),
                _ => throw new ArgumentException($"I can not save this type '{shape.GetType().Name}' of entity yet.")
            };
            return this;
        }

        private string WriteTransformCurve(Curve curve)
        {
            var filename = $"{CURVE_PREFIX}{Guid.NewGuid()}.dxf";
            var filePath = Path.Combine(_folderPath, filename);
            _dxfReader.WriteCurveToFile(filePath, curve, true);
            return filePath;
        }

        private string WriteTransformCircle(Circle circle)
        {
            var filename = $"{CIRCLE_PREFIX}{Guid.NewGuid()}.dxf";
            var filePath = Path.Combine(_folderPath, filename);
            _dxfReader.WriteCircleToFile(filePath, circle);
            return filePath;
        }

        public void Dispose()
        {
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
        }
    }
}
