﻿using MachineClassLibrary.Classes;
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

            //var obj = procObject switch
            //{
            //    PCurve curve => (IShape)RotatePCurve(curve),
            //    PCircle circle => (IShape)circle.PObject
            //};


            Func<IProcObject, IShape> foo = procObject =>
            {

                if (procObject is PCircle circle)
                {
                    if (circle.PObject.Radius + _contourOffset <= 0 | (_contourOffset == 0 & _contourWidth == 0)) return circle.PObject;

                    var r1 = circle.PObject.Radius + _contourOffset;
                    var r2 = r1 + _contourWidth;
                    return new Ring { Radius1 = r1, Radius2 = r2 };
                }

                if (procObject is PCurve curve)
                {
                    var initialCurve =  RotatePCurve(curve);
                    if (_contourOffset != 0 || _contourWidth > 0)
                    {
                        var outerCurves = initialCurve.InflateCurve(_contourOffset);
                        var innerCurves = outerCurves.SelectMany(curve=>curve.InflateCurve(-_contourWidth)); 
                        var resultCurves = outerCurves.Concat(innerCurves).ToList();
                        return new ContourRing { Curves = resultCurves };
                    }
                    else
                    {
                        return initialCurve;
                    }
                }
                throw new ArgumentException($"The preparator can not to process {procObject.GetType().Name} entity"); 
            };

            //return new EntityFileHandler(_dxfReader, _folderPath).SaveEntityToFile(obj/*.Invoke(procObject)*/);


            return new EntityFileHandler(_dxfReader,_folderPath).SaveEntityToFile(foo.Invoke(procObject));

            Curve RotatePCurve(PCurve pCurve)
            {
                var vertices = new List<(double x,double y,double bulge)>(pCurve.PObject.Vertices);
                var rotation = Matrix3x2.CreateRotation((float)_angle);
                var matrix = new Matrix(rotation);
                var points = vertices.Select(vertex => new PointF((float)vertex.x, (float)vertex.y)).ToArray();
                matrix.TransformPoints(points);
                var curve = new Curve(
                        points.Zip(vertices, (p, v) => ((double)p.X, (double)p.Y, v.bulge)),
                        pCurve.PObject.IsClosed);
                return curve;
            }

        }

    }
}
