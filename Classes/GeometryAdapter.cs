using System;
using System.Collections.Generic;
using System.Linq;
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
        public IEnumerable<Geometry> GetGeometies() => GetLineGeometry().Concat(GetEllipseGeometry()).Select(item => item.geometry);
        public IEnumerable<AdaptedGeometry> GetGeometries() => GetLineGeometry().Concat(GetEllipseGeometry());
        private IEnumerable<AdaptedGeometry> GetLineGeometry()
        {
            return _dxfReader.GetAllSegments()
                .Select(
                        line => new AdaptedGeometry(
                            new LineGeometry(new Point(line.PObject.X1, line.PObject.Y1), new Point(line.PObject.X2, line.PObject.Y2)),
                            line.LayerName,
                            GetColorFromArgb(_dxfReader.GetLayers()[line.LayerName]),
                            GetColorFromArgb(line.ARGBColor)
                    )
                );
        }
        private IEnumerable<AdaptedGeometry> GetEllipseGeometry()
        {
           return  _dxfReader.GetCircles()
                .Select(
                    circle => new AdaptedGeometry(
                        new EllipseGeometry(new Point(circle.X, circle.Y), circle.PObject.Radius, circle.PObject.Radius),
                        circle.LayerName,
                        GetColorFromArgb(_dxfReader.GetLayers()[circle.LayerName]),
                        GetColorFromArgb(circle.ARGBColor)
                    )
                );
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
