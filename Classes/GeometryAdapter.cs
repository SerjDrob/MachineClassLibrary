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
        public IEnumerable<Geometry> GetGeometies() => GetLineGeometry()
            .Concat(GetEllipseGeometry())
            .Concat(GetPointGeometry())
            .Select(item => item.geometry);
        public IEnumerable<AdaptedGeometry> GetGeometries() => 
            GetLineGeometry().
            Concat(GetEllipseGeometry()).
            Concat(GetPointGeometry());
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
        private IEnumerable<AdaptedGeometry> GetPointGeometry()
        {
            Func<double,double,StreamGeometry> func = (x,y) =>
            {
                var scale = 100;//TODO the scale should be passed here from outside
                var streamGeometry = new StreamGeometry();
                using var strContext = streamGeometry.Open();
                strContext.BeginFigure(new Point(x - 5 * scale, y), true, true);
                strContext.LineTo(new Point(x, 5 * scale + y), true, false);
                strContext.LineTo(new Point(5 * scale + x, y), true, false);
                strContext.LineTo(new Point(x, y - 5 * scale), true, false);
                streamGeometry.Freeze();
                return streamGeometry;
            };
            return _dxfReader.GetPoints()
                .Select(
                    point=>new AdaptedGeometry(
                        func.Invoke(point.X,point.Y),
                        point.LayerName,
                        GetColorFromArgb(_dxfReader.GetLayers()[point.LayerName]),
                        GetColorFromArgb(point.ARGBColor)
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
