using MachineClassLibrary.Classes;
using MachineClassLibrary.Laser.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Numerics;

namespace MachineClassLibrary.Laser
{
    public class EntityPreparator
    {
        private readonly IDxfReader _dxfReader;
        private readonly string _folderPath;
        private double _angle;
        private double _contourWidth;
        private double _contourOffset;

        public EntityPreparator(IDxfReader dxfReader, string folderPath)
        {
            _dxfReader = dxfReader;
            _folderPath = folderPath;
        }

        public EntityPreparator SetEntityAngle(double angle)
        {
            _angle = angle;
            return this;
        }

        public EntityPreparator AddEntityAngle(double angle)
        {
            _angle += angle;
            return this;
        }

        public EntityPreparator SetEntityContourWidth(double width)
        {
            _contourWidth = width;
            return this;
        }

        public EntityPreparator SetEntityContourOffset(double offset)
        {
            _contourOffset = offset;
            return this;
        }

        public EntityFileHandler GetPreparedEntityDxfHandler(IProcObject procObject)
        {
            //TODO apply contourWidth, contourOffset and _entityAngle arithmetic before saving

            var obj = procObject switch
            {
                PCurve curve => (IShape)RotatePCurve(curve),
                PCircle circle => (IShape)circle.PObject
            };

            return new EntityFileHandler(_dxfReader, _folderPath).SaveEntityToFile(obj);

            Curve RotatePCurve(PCurve pCurve)
            {
                var vertices = new List<(double x,double y,double bulge)>(pCurve.PObject.Vertices);
                var rotation = Matrix3x2.CreateRotation((float)_angle);
                var matrix = new Matrix(rotation);
                var points = vertices.Select(vertex => new PointF((float)vertex.x, (float)vertex.y)).ToArray();
                matrix.TransformPoints(points);
                var curve = new Curve();
                curve.Vertices = points.Zip(vertices, (p, v) => ((double)p.X, (double)p.Y, v.bulge));
                return curve;
            }

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
