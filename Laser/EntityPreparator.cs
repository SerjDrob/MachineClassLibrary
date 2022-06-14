using MachineClassLibrary.Classes;
using MachineClassLibrary.Laser.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineClassLibrary.Laser
{
    public class EntityPreparator
    {
        private readonly IDxfReader _dxfReader;
        private readonly string _folderPath;
        /// <summary>
        /// in radians
        /// </summary>
        private readonly double _entityAngle;

        public EntityPreparator(IDxfReader dxfReader, string folderPath, double entityAngle)
        {
            _dxfReader = dxfReader;
            _folderPath = folderPath;
            _entityAngle = entityAngle;
        }

        public EntityFileHandler GetPreparedEntityDxfHandler(IProcObject procObject, double contourOffset, double contourWidth)
        {
            //TODO apply contourWidth, contourOffset and _entityAngle arithmetic before saving
            return new EntityFileHandler(_dxfReader,_folderPath).SaveEntityToFile(procObject);
        }
               
    }

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
        public EntityFileHandler SaveEntityToFile(IProcObject procObject)
        {
            _path = procObject switch
            {
                PCurve curve => WriteTransformCurve(curve.PObject),
                PCircle circle => WriteTransformCircle(circle.PObject),
                _ => throw new ArgumentException($"I can not save this type '{procObject.GetType().Name}' of entity yet.")
            };
            return this;
        }

        private string WriteTransformCurve(Curve curve)
        {
            var filename = $"{CURVE_PREFIX}{Guid.NewGuid()}.dxf";
            _dxfReader.WriteCurveToFile(Path.Combine(_folderPath, filename), curve, true);
            return filename;
        }

        private string WriteTransformCircle(Circle circle)
        {
            var filename = $"{CIRCLE_PREFIX}{Guid.NewGuid()}.dxf";
            _dxfReader.WriteCircleToFile(Path.Combine(_folderPath, filename), circle);
            return filename;
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
