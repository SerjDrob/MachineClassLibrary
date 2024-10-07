using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Windows;
using MachineClassLibrary.Classes;

namespace MachineClassLibrary.Laser.Entities
{
    internal class PCluster : IProcObject<PCluster>, IShape
    {
        private readonly IProcObject[] _procObjects;
        public IEnumerable<IProcObject> ProcObjects => _procObjects;
        public PCluster(double x, double y, IEnumerable<IProcObject> procObjects)
        {
            Id = Guid.NewGuid();
            _procObjects = procObjects.ToArray();
            X = x;
            Y = y;
        }
        public PCluster(double x, double y, double angle, IEnumerable<IProcObject> procObjects, string layerName, int argBColor)
        {
            Id = Guid.NewGuid();
            X = x;
            Y = y;
            Angle = angle;
            LayerName = layerName;
            ARGBColor = argBColor;
            _procObjects = procObjects.ToArray();
        }
        private PCluster(double x, double y, double angle, IEnumerable<IProcObject> procObjects, string layerName, int argBColor, Guid id)
        {
            Id = id;
            X = x;
            Y = y;
            Angle = angle;
            LayerName = layerName;
            ARGBColor = argBColor;
            _procObjects = procObjects.ToArray();
        }
        public Guid Id
        {
            get; init;
        }
        public double X
        {
            get; init;
        }
        public double Y
        {
            get; init;
        }
        public double Angle
        {
            get; init;
        }
        public string LayerName
        {
            get; set;
        }
        public int ARGBColor
        {
            get; set;
        }
        //private readonly Curve _curve;
        public double Scaling { get; private set; } = 1;
        public bool MirrorX { get; private set; } = false;
        public bool Turn90 { get; private set; } = false;
        public bool IsBeingProcessed { get; set; } = false;
        public bool IsProcessed { get; set; } = false;
        public bool ToProcess { get; set; } = true;

        public Rect Bounds
        {
            get; private set;
        }
        private PCluster _pCluster;
        public PCluster PObject { get => GetTransformedCluster(); init => _pCluster = value; }

        private PCluster GetTransformedCluster()
        {
            var transformation = Matrix3x2.Identity;
            var mirror = Matrix3x2.CreateScale(MirrorX ? -1 : 1, 1);
            var scaling = Matrix3x2.CreateScale((float)Scaling);
            var rotation = Matrix3x2.CreateRotation(Turn90 ? MathF.PI * 90 / 180 : 0);
            transformation *= scaling * mirror * rotation;
            var matrix = new Matrix(transformation);

            var transObjects = _procObjects.Select(p =>
            {
                var points = new PointF[] { new PointF((float)p.X, (float)p.Y) };
                matrix.TransformPoints(points);
                var pobj = p.CloneWithPosition(points[0].X, points[0].Y);
                return pobj;
            }).ToArray(); 

            return new(this.X,this.Y,transObjects);
        }

        public void Scale(double scale)
        {
            Scaling = scale;
            foreach (var @object in _procObjects)
            {
                @object.Scale(scale);
            }
        }
        public void SetMirrorX(bool mirror)
        {
            MirrorX = mirror;
            foreach (var @object in _procObjects)
            {
                @object.SetMirrorX(mirror);
            }
        }
        public void SetTurn90(bool turn)
        {
            Turn90 = turn;
            foreach (var @object in _procObjects)
            {
                @object.SetTurn90(turn);
            }
        }
        public override string ToString() => $"{GetType().Name} X:{X}, Y:{Y} Id = {Id}";
        
        public (double x, double y) GetSize()
        {
            return (Bounds.Width, Bounds.Height);
        }

        public void Deconstruct(out IShape[] primaryShape, out int num)
        {
            throw new NotImplementedException();
        }

        public IProcObject<PCluster> CloneWithPosition(double x, double y) => new PCluster(x, y, Angle, _procObjects, LayerName, ARGBColor, Id);
        IProcObject IProcObject.CloneWithPosition(double x, double y) => CloneWithPosition(x, y);
    }
    public static class ProcObjectsExtensions
    {
        public static IEnumerable<IProcObject> SplitOnClusters(this IEnumerable<IProcObject> procObjects, int xParts, int yParts)
        {
            var boundary = procObjects.Aggregate(new Rect(), (acc, pobj) =>
            {
                acc.Union(pobj.GetBoundingBox());
                return acc;
            });

            return procObjects.SplitOnClusters(boundary, xParts, yParts);
        }

        public static IEnumerable<IProcObject> SplitOnClusters(this IEnumerable<IProcObject> procObjects, Rect boundary, int xParts, int yParts)
        {
            var width = boundary.Width / xParts;
            var height = boundary.Height / yParts;
            var boundaries = Enumerable.Range(0, xParts)
                .SelectMany(x => Enumerable.Range(0, yParts).Select(y => new Rect(x * width + boundary.X, y * height + boundary.Y, width, height)));

            var objects = procObjects.ToList();
            return boundaries.Select(b =>
            {
                var centerX = b.X + b.Width / 2;
                var centerY = b.Y + b.Height / 2;
                return new PCluster(centerX, centerY, objects.ExtractMorph(o => b.Contains(o.X, o.Y), ex =>
                {
                    var obj = ex.CloneWithPosition(ex.X - centerX, ex.Y - centerY);
                    return obj;
                }));
            }).Where(b => b.ProcObjects.Any());
        }

        public static IEnumerable<T> Extract<T>(this List<T> values, Predicate<T> predicate)
        {
            var extracted = new List<T>(values.Where(predicate.Invoke));
            values.RemoveAll(predicate);
            return extracted;
        }
        public static IEnumerable<T> ExtractMorph<T>(this List<T> values, Predicate<T> predicate, Func<T, T> morph) => values.Extract(predicate).Select(e => morph(e));
    }
}
