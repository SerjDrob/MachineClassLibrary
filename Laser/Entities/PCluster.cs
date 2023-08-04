using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using MachineClassLibrary.Classes;

namespace MachineClassLibrary.Laser.Entities
{
    internal class PCluster : IProcObject, IShape
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
        private readonly Curve _curve;
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

        public void Scale(double scale)
        {
            Scaling = scale;
        }
        public void SetMirrorX(bool mirror)
        {
            MirrorX = mirror;
        }
        public void SetTurn90(bool turn)
        {
            Turn90 = turn;
        }
        public override string ToString() => $"{GetType().Name} X:{X}, Y:{Y} Id = {Id}";
        public IProcObject CloneWithPosition(double x, double y) => new PCluster(x, y, Angle, _procObjects, LayerName, ARGBColor, Id);
        public (double x, double y) GetSize()
        {
            return (Bounds.Width, Bounds.Height);
        }
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
            return boundaries.Select(b => {
                var centerX = b.X + b.Width / 2;
                var centerY = b.Y + b.Height / 2;
                return new PCluster(centerX, centerY, objects.ExtractMorph(o => b.Contains(o.X, o.Y), ex => ex.CloneWithPosition(ex.X - centerX, ex.Y - centerY)));
            }).Where(b=>b.ProcObjects.Any());
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