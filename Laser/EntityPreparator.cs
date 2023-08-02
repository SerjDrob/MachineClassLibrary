using MachineClassLibrary.Classes;
using MachineClassLibrary.Laser.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

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
            if(procObject is not PCluster) return new EntityFileHandler(_dxfReader, _folderPath).SaveEntityToFile(PrepareShape(procObject));
            else if (procObject is PCluster cluster)
            {
                var shapes = cluster.ProcObjects.Select(p => PrepareShape(p, false)).ToArray();
                return new EntityFileHandler(_dxfReader, _folderPath).SaveEntitiesToFile(shapes);
            }
            throw new ArgumentException($"The preparator can not to process {procObject.GetType().Name} entity");
        }


        private IShape PrepareShape(IProcObject procObject, bool onPosition = true)
        {

            if (procObject is PCircle circle)
            {
                var rotatedCircle = RotatePCircle(circle, onPosition);
                if (rotatedCircle.Radius + _contourOffset <= 0 | (_contourOffset == 0 & _contourWidth == 0)) return rotatedCircle;

                var r1 = rotatedCircle.Radius + _contourOffset;
                var r2 = r1 - _contourWidth;
                if (r2 < 0.02) return new Circle 
                {
                    Radius = r1, 
                    CenterX = rotatedCircle.CenterX,
                    CenterY = rotatedCircle.CenterY
                };
                return new Ring 
                {
                    Radius1 = r1, 
                    Radius2 = r2,
                    CenterX = rotatedCircle.CenterX,
                    CenterY = rotatedCircle.CenterY
                };
            }

            if (procObject is PCurve curve)
            {
                var initialCurve = RotatePCurve(curve, onPosition);
                if (_contourOffset != 0 || _contourWidth > 0)
                {
                    var outerCurves = _contourOffset != 0 ? initialCurve.InflateCurve(_contourOffset) : Enumerable.Repeat(initialCurve, 1);
                    var innerCurves = outerCurves.SelectMany(curve => curve.InflateCurve(-_contourWidth));
                    var resultCurves = outerCurves.Concat(innerCurves).ToList();

                    return new ContourRing { Curves = resultCurves };
                }
                else
                {
                    return initialCurve;
                }
            }

            throw new ArgumentException($"The preparator can not to process {procObject.GetType().Name} entity");
        }

        private Curve RotatePCurve(PCurve pCurve, bool onPosition = true)
        {
            var deltaX = onPosition ? 0 : pCurve.X;
            var deltaY = onPosition ? 0 : pCurve.Y;
            var vertices = new List<(double x, double y, double bulge)>(pCurve.PObject.Vertices);
            var rotation = Matrix3x2.CreateRotation((float)_angle);
            var matrix = new Matrix(rotation);
            var points = vertices.Select(vertex => new PointF((float)(vertex.x + deltaX), (float)(vertex.y + deltaY))).ToArray();
            matrix.TransformPoints(points);
            var curve = new Curve(
                    points.Zip(vertices, (p, v) => ((double)p.X, (double)p.Y, v.bulge)),
                    pCurve.PObject.IsClosed);
            return curve;//TODO now bounds are incorrect
        }

        private Circle RotatePCircle(PCircle pCircle, bool onPosition = true)
        {
            if (onPosition) return pCircle.PObject;
            var rotation = Matrix3x2.CreateRotation((float)_angle);
            var matrix = new Matrix(rotation);
            var points = new PointF[] { new((float)pCircle.X, (float)pCircle.Y) };
            matrix.TransformPoints(points);
            return new Circle()
            {
                CenterX = points[0].X,
                CenterY = points[0].Y,  
                Radius = pCircle.PObject.Radius
            };//TODO now bounds are incorrect
        }


    }
}
