using MachineClassLibrary.Classes;
using MachineClassLibrary.Laser.Entities;
//using netDxf.Entities;
using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;

namespace MachineClassLibrary.Laser
{
    public class EntityFileHandler : IDisposable
    {
        private readonly IDxfReader _dxfReader;
        private readonly string _folderPath;
        private string _path;
        const string CURVE_PREFIX = "Curve_";
        const string CIRCLE_PREFIX = "Circle_";
        const string RING_PREFIX = "Ring_";
        const string CONTRING_PREFIX = "ContourRing_";
        const string CLUSTER_PREFIX = "Cluster_";

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
                Ring ring => WriteTransformRing(ring),
                ContourRing contourRing => WriteTransformContourRing(contourRing),
                _ => throw new ArgumentException($"I can not save this type '{shape.GetType().Name}' of entity yet.")
            };
            return this;
        }

        public EntityFileHandler SaveEntitiesToFile(IShape[] shapes)
        {
            var filename = $"{CLUSTER_PREFIX}{Guid.NewGuid()}.dxf";
            var filePath = Path.Combine(_folderPath, filename);
            _dxfReader.WriteShapesToFile(filePath, shapes);
            _path = filePath;
            return this;
        }

        private string WriteTransformContourRing(ContourRing contourRing)
        {
            var filename = $"{CONTRING_PREFIX}{Guid.NewGuid()}.dxf";
            var filePath = Path.Combine(_folderPath, filename);

            _dxfReader.WriteShapesToFile(filePath, contourRing.Curves.ToArray());
            return filePath;
        }

        private string WriteTransformRing(Ring ring)
        {
            var filename = $"{RING_PREFIX}{Guid.NewGuid()}.dxf";
            var filePath = Path.Combine(_folderPath, filename);

            var circle1 = new Circle { Radius = ring.Radius1 };
            var circle2 = new Circle { Radius = ring.Radius2 };

            _dxfReader.WriteShapesToFile(filePath, circle1, circle2);
            return filePath;
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
