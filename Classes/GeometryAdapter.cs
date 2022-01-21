using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MachineClassLibrary.Classes
{
    public class GeometryAdapter
    {
        private readonly IDxfReader _dxfReader;
        public GeometryCollection Geometries { get => new GeometryCollection(GetGeometies()); }
        public GeometryAdapter(IDxfReader dxfReader)
        {
            _dxfReader = dxfReader;
        }
        public IEnumerable<Geometry> GetGeometies()
        {
            foreach (var item in GetLineGeometry())
            {
                yield return item.geometry;
            }
            foreach (var item in GetEllipseGeometry())
            {
                yield return item.geometry;
            }
        }
        public IEnumerable<AdaptedGeometry> GetGeometries()
        {
            foreach (var item in GetLineGeometry())
            {
                yield return item;
            }
            foreach (var item in GetEllipseGeometry())
            {
                yield return item;
            }
        }
        private IEnumerable<AdaptedGeometry> GetLineGeometry()
        {
            foreach (var line in _dxfReader.GetAllSegments())
            {
                var entColor = GetColorFromArgb(line.ARGBColor);
                var layerColor = GetColorFromArgb(_dxfReader.GetLayers()[line.LayerName]);

                yield return new AdaptedGeometry(new LineGeometry(new Point(line.PObject.X1, line.PObject.Y1), new Point(line.PObject.X2, line.PObject.Y2)),
                    line.LayerName, layerColor, entColor);
            }
        }
        private IEnumerable<AdaptedGeometry> GetEllipseGeometry()
        {
            foreach (var circle in _dxfReader.GetCircles())
            {
                var entColor = GetColorFromArgb(circle.ARGBColor);
                var layerColor = GetColorFromArgb(_dxfReader.GetLayers()[circle.LayerName]);

                yield return new AdaptedGeometry(new EllipseGeometry(new Point(circle.X, circle.Y), circle.PObject.Radius, circle.PObject.Radius),
                    circle.LayerName, layerColor, entColor);
            }
        }
        public static SolidColorBrush GetColorFromArgb(int argb)
        {
            var rgbValues = BitConverter.GetBytes(argb);
            if (!BitConverter.IsLittleEndian) Array.Reverse(rgbValues);
            var r = rgbValues[2];
            var g = rgbValues[1];
            var b = rgbValues[0];
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }
    }
}
