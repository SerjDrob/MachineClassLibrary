using IxMilia.Dxf.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Windows.Shapes;
using System.Windows.Media;
using IxMilia.Dxf;
using MachineClassLibrary.Laser.Entities;
using netDxf.Entities;
using static netDxf.Entities.HatchBoundaryPath;
using System.IO;
using System.Xml;

namespace MachineClassLibrary.Classes
{
    public static class PathMarkupConverter
    {
        public static Geometry ToGeometry(this DxfLwPolyline poly)
        {
            var svg = poly.GetSvgPath();
            var geometry = Geometry.Parse(svg.ToString());
            return geometry;
        }
        public static Geometry ToGeometry(this DxfEllipse ellipse)
        {
            var svg = ellipse.GetSvgPath();
            var geometry = Geometry.Parse(svg.ToString());
            return geometry;
        }
        public static Geometry ToGeometry(this DxfCircle circle)
        {
            var svg = circle.GetSvgPath();
            var geometry = Geometry.Parse(svg.ToString());
            return geometry;
        }
        public static Geometry ToGeometry(this PCurve pCurve)
        {
            var polylyne = new DxfLwPolyline(pCurve.PObject.Vertices.Select(v => new DxfLwPolylineVertex() 
            {
                X = v.X, 
                Y = v.Y,
                Bulge = v.Bulge,
            }))
            { IsClosed = pCurve.PObject.IsClosed };
            return polylyne.ToGeometry();
        }
        public static Geometry ToGeometry(this PCircle pCircle)
        {
            var circle = new DxfCircle(new DxfPoint(pCircle.X,pCircle.Y,0),pCircle.PObject.Radius);
            return circle.ToGeometry();
        }

        public static Geometry ToGeometry(this IProcObject procObject) => procObject switch
        {
            PCurve pCurve => pCurve.ToGeometry(),
            PCircle pCircle => pCircle.ToGeometry(),
            _ => null
        };
    }
    public class SvgPath
    {
        public List<SvgPathSegment> Segments { get; }

        public SvgPath(IEnumerable<SvgPathSegment> segments)
        {
            Segments = segments.ToList();
        }

        public override string ToString()
        {
            return string.Join(" ", Segments);
        }

        public static SvgPath FromEllipse(double centerX, double centerY, double majorAxisX, double majorAxisY, double minorAxisRatio, double startAngle, double endAngle)
        {
            // large arc and counterclockwise computations all rely on the end angle being greater than the start
            while (endAngle < startAngle)
            {
                endAngle += Math.PI * 2.0;
            }

            var axisAngle = Math.Atan2(majorAxisY, majorAxisY);
            var majorAxisLength = Math.Sqrt(majorAxisX * majorAxisX + majorAxisY * majorAxisY);
            var minorAxisLength = majorAxisLength * minorAxisRatio;

            var startSin = Math.Sin(startAngle);
            var startCos = Math.Cos(startAngle);
            var startX = centerX + startCos * majorAxisLength;
            var startY = centerY + startSin * minorAxisLength;

            var endSin = Math.Sin(endAngle);
            var endCos = Math.Cos(endAngle);
            var endX = centerX + endCos * majorAxisLength;
            var endY = centerY + endSin * minorAxisLength;

            var enclosedAngle = endAngle - startAngle;
            var isLargeArc = (endAngle - startAngle) > Math.PI;
            var isCounterClockwise = endAngle > startAngle;

            var segments = new List<SvgPathSegment>();
            segments.Add(new SvgMoveToPath(startX, startY));
            var oneDegreeInRadians = Math.PI / 180.0;
            if (Math.Abs(Math.PI - enclosedAngle) <= oneDegreeInRadians)
            {
                // really close to a semicircle; split into to half arcs to avoid rendering artifacts
                var midAngle = (startAngle + endAngle) / 2.0;
                var midSin = Math.Sin(midAngle);
                var midCos = Math.Cos(midAngle);
                var midX = centerX + midCos * majorAxisLength;
                var midY = centerY + midSin * minorAxisLength;
                segments.Add(new SvgArcToPath(majorAxisLength, minorAxisLength, axisAngle, false, isCounterClockwise, midX, midY));
                segments.Add(new SvgArcToPath(majorAxisLength, minorAxisLength, axisAngle, false, isCounterClockwise, endX, endY));
            }
            else
            {
                // can be contained by just one arc
                segments.Add(new SvgArcToPath(majorAxisLength, minorAxisLength, axisAngle, isLargeArc, isCounterClockwise, endX, endY));
            }

            return new SvgPath(segments);
        }
    }
    public class SvgLineToPath : SvgPathSegment
    {
        public double LocationX { get; }
        public double LocationY { get; }

        public SvgLineToPath(double locationX, double locationY)
        {
            LocationX = locationX;
            LocationY = locationY;
        }

        public override string ToString()
        {
            return string.Join(" ", new[]
            {
                "L", // line absolute
                LocationX.ToDisplayString(),
                LocationY.ToDisplayString()
            });
        }
    }



    public class SvgArcToPath : SvgPathSegment
    {
        public double RadiusX { get; }
        public double RadiusY { get; }
        public double XAxisRotation { get; }
        public bool IsLargeArc { get; }
        public bool IsCounterClockwiseSweep { get; }
        public double EndPointX { get; }
        public double EndPointY { get; }

        public SvgArcToPath(double radiusX, double radiusY, double xAxisRotation, bool isLargeArc, bool isCounterClockwiseSweep, double endPointX, double endPointY)
        {
            RadiusX = radiusX;
            RadiusY = radiusY;
            XAxisRotation = xAxisRotation;
            IsLargeArc = isLargeArc;
            IsCounterClockwiseSweep = isCounterClockwiseSweep;
            EndPointX = endPointX;
            EndPointY = endPointY;
        }

        public override string ToString()
        {
            return string.Join(" ", new object[]
            {
                "A", // arc absolute
                RadiusX.ToDisplayString(),
                RadiusY.ToDisplayString(),
                XAxisRotation.ToDisplayString(),
                IsLargeArc ? 1 : 0,
                IsCounterClockwiseSweep ? 1 : 0,
                EndPointX.ToDisplayString(),
                EndPointY.ToDisplayString()
            });
        }
    }
    public class SvgMoveToPath : SvgPathSegment
    {
        public double LocationX { get; }
        public double LocationY { get; }

        public SvgMoveToPath(double locationX, double locationY)
        {
            LocationX = locationX;
            LocationY = locationY;
        }

        public override string ToString()
        {
            return string.Join(" ", new[]
            {
                "M", // move absolute
                LocationX.ToDisplayString(),
                LocationY.ToDisplayString()
            });
        }
    }
    public abstract class SvgPathSegment
    {
    }
    public static class SvgExtensions
    {
        public static void SaveTo(this XElement document, Stream output)
        {
            var settings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "  "
            };
            using (var writer = XmlWriter.Create(output, settings))
            {
                document.WriteTo(writer);
            }
        }

        public static void SaveTo(this XElement document, string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                document.SaveTo(fileStream);
            }
        }

        public static string ToRGBString(this DxfColor color)
        {
            var intValue = color.IsIndex
                ? color.ToRGB()
                : 0; // fall back to black
            var r = (intValue >> 16) & 0xFF;
            var g = (intValue >> 8) & 0xFF;
            var b = intValue & 0xFF;
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        internal static string ToDisplayString(this double value)
        {
            return value.ToString("0.0##############", CultureInfo.InvariantCulture);
        }

        //public static async Task<XElement> ToXElement(this DxfEntity entity, DxfToSvgConverterOptions options)
        //{
        //    // elements are simply flattened in the z plane; the world transform in the main function handles the rest
        //    switch (entity)
        //    {
        //        case DxfArc arc:
        //            return arc.ToXElement();
        //        case DxfCircle circle:
        //            return circle.ToXElement();
        //        case DxfEllipse ellipse:
        //            return ellipse.ToXElement();
        //        case DxfImage image:
        //            return await image.ToXElement(options);
        //        case DxfLine line:
        //            return line.ToXElement();
        //        case DxfLwPolyline lwPolyline:
        //            return lwPolyline.ToXElement();
        //        case DxfPolyline polyline:
        //            return polyline.ToXElement();
        //        case DxfInsert insert:
        //            return await insert.ToXElement(options);
        //        case DxfSpline spline:
        //            return spline.ToXElement();
        //        default:
        //            return null;
        //    }
        //}

        //public static XElement ToXElement(this DxfArc arc)
        //{
        //    var path = arc.GetSvgPath();
        //    return new XElement(DxfToSvgConverter.Xmlns + "path",
        //        new XAttribute("d", path.ToString()),
        //        new XAttribute("fill-opacity", 0))
        //        .AddStroke(arc.Color)
        //        .AddStrokeWidth(arc.Thickness)
        //        .AddVectorEffect();
        //}

        //public static XElement ToXElement(this DxfCircle circle)
        //{
        //    return new XElement(DxfToSvgConverter.Xmlns + "ellipse",
        //        new XAttribute("cx", circle.Center.X.ToDisplayString()),
        //        new XAttribute("cy", circle.Center.Y.ToDisplayString()),
        //        new XAttribute("rx", circle.Radius.ToDisplayString()),
        //        new XAttribute("ry", circle.Radius.ToDisplayString()),
        //        new XAttribute("fill-opacity", 0))
        //        .AddStroke(circle.Color)
        //        .AddStrokeWidth(circle.Thickness)
        //        .AddVectorEffect();
        //}

        //public static XElement ToXElement(this DxfEllipse ellipse)
        //{
        //    XElement baseShape;
        //    if (ellipse.StartParameter.IsCloseTo(0.0) && ellipse.EndParameter.IsCloseTo(Math.PI * 2.0))
        //    {
        //        baseShape = new XElement(DxfToSvgConverter.Xmlns + "ellipse",
        //            new XAttribute("cx", ellipse.Center.X.ToDisplayString()),
        //            new XAttribute("cy", ellipse.Center.Y.ToDisplayString()),
        //            new XAttribute("rx", ellipse.MajorAxis.Length.ToDisplayString()),
        //            new XAttribute("ry", ellipse.MinorAxis().Length.ToDisplayString()));
        //    }
        //    else
        //    {
        //        var path = ellipse.GetSvgPath();
        //        baseShape = new XElement(DxfToSvgConverter.Xmlns + "path",
        //            new XAttribute("d", path.ToString()));
        //    }

        //    baseShape.Add(new XAttribute("fill-opacity", 0));
        //    return baseShape
        //        .AddStroke(ellipse.Color)
        //        .AddStrokeWidth(1.0)
        //        .AddVectorEffect();
        //}

        //public static async Task<XElement> ToXElement(this DxfImage image, DxfToSvgConverterOptions options)
        //{
        //    var imageHref = await options.ResolveImageHrefAsync(image.ImageDefinition.FilePath);
        //    var imageWidth = image.UVector.Length * image.ImageSize.X;
        //    var imageHeight = image.VVector.Length * image.ImageSize.Y;
        //    var radians = Math.Atan2(image.UVector.Y, image.UVector.X);
        //    var upVector = new DxfVector(-Math.Sin(radians), Math.Cos(radians), 0.0) * imageHeight;
        //    var displayRotationDegrees = -radians * 180.0 / Math.PI;
        //    var topLeftDxf = image.Location + upVector;
        //    var insertLocation = topLeftDxf;
        //    return new XElement(DxfToSvgConverter.Xmlns + "image",
        //        new XAttribute("href", imageHref),
        //        new XAttribute("width", imageWidth.ToDisplayString()),
        //        new XAttribute("height", imageHeight.ToDisplayString()),
        //        new XAttribute("transform", $"translate({insertLocation.X.ToDisplayString()} {insertLocation.Y.ToDisplayString()}) scale(1 -1) rotate({displayRotationDegrees.ToDisplayString()})"))
        //        .AddStroke(image.Color)
        //        .AddVectorEffect();
        //}

        //public static XElement ToXElement(this DxfLine line)
        //{
        //    return new XElement(DxfToSvgConverter.Xmlns + "line",
        //        new XAttribute("x1", line.P1.X.ToDisplayString()),
        //        new XAttribute("y1", line.P1.Y.ToDisplayString()),
        //        new XAttribute("x2", line.P2.X.ToDisplayString()),
        //        new XAttribute("y2", line.P2.Y.ToDisplayString()))
        //        .AddStroke(line.Color)
        //        .AddStrokeWidth(line.Thickness)
        //        .AddVectorEffect();
        //}

        //public static XElement ToXElement(this DxfLwPolyline poly)
        //{
        //    var path = poly.GetSvgPath();
        //    return new XElement(DxfToSvgConverter.Xmlns + "path",
        //        new XAttribute("d", path.ToString()),
        //        new XAttribute("fill-opacity", 0))
        //        .AddStroke(poly.Color)
        //        .AddStrokeWidth(1.0)
        //        .AddVectorEffect();
        //}

        //public static XElement ToXElement(this DxfPolyline poly)
        //{
        //    var path = poly.GetSvgPath();
        //    return new XElement(DxfToSvgConverter.Xmlns + "path",
        //        new XAttribute("d", path.ToString()),
        //        new XAttribute("fill-opacity", 0))
        //        .AddStroke(poly.Color)
        //        .AddStrokeWidth(1.0)
        //        .AddVectorEffect();
        //}

        //public static async Task<XElement> ToXElement(this DxfInsert insert, DxfToSvgConverterOptions options)
        //{
        //    var g = new XElement(DxfToSvgConverter.Xmlns + "g",
        //        new XAttribute("class", $"dxf-insert {insert.Name}"),
        //        new XAttribute("transform", $"translate({insert.Location.X.ToDisplayString()} {insert.Location.Y.ToDisplayString()}) scale({insert.XScaleFactor.ToDisplayString()} {insert.YScaleFactor.ToDisplayString()})"));
        //    foreach (var blockEntity in insert.Entities)
        //    {
        //        g.Add(await blockEntity.ToXElement(options));
        //    }

        //    return g;
        //}

        //public static XElement ToXElement(this DxfSpline spline)
        //{
        //    var spline2 = new Spline2(
        //        spline.DegreeOfCurve,
        //        spline.ControlPoints.Select(p => new SplinePoint2(p.Point.X, p.Point.Y)),
        //        spline.KnotValues);
        //    var beziers = spline2.ToBeziers();
        //    var path = beziers.GetSvgPath();
        //    return new XElement(DxfToSvgConverter.Xmlns + "path",
        //        new XAttribute("d", path.ToString()),
        //        new XAttribute("fill-opacity", 0))
        //        .AddStroke(spline.Color)
        //        .AddStrokeWidth(1.0)
        //        .AddVectorEffect();
        //}

        private static SvgPathSegment FromPolylineVertices(DxfLwPolylineVertex last, DxfLwPolylineVertex next)
        {
            return FromPolylineVertices(last.X, last.Y, last.Bulge, next.X, next.Y);
        }

        private static SvgPathSegment FromPolylineVertices(DxfVertex last, DxfVertex next)
        {
            return FromPolylineVertices(last.Location.X, last.Location.Y, last.Bulge, next.Location.X, next.Location.Y);
        }

        private static SvgPathSegment FromPolylineVertices(double lastX, double lastY, double lastBulge, double nextX, double nextY)
        {
            var dx = nextX - lastX;
            var dy = nextY - lastY;
            var dist = Math.Sqrt(dx * dx + dy * dy);
            if (lastBulge.IsCloseTo(0.0) || dist.IsCloseTo(1.0e-10))
            {
                // line or a really short arc
                return new SvgLineToPath(nextX, nextY);
            }

            // given the following diagram:
            //
            //                p1
            //               -)
            //            -  |  )
            //        -      |    )
            //    -          |     )
            // O ------------|C----T
            //    -          |     )
            //        -      |    )
            //            -  |  )
            //               -)
            //               p2
            //
            // where O is the center of the circle, C is the midpoint between p1 and p2, calculate
            // the hypotenuse of the triangle Op1C to get the radius

            var includedAngle = Math.Atan(Math.Abs(lastBulge)) * 4.0;
            var isLargeArc = includedAngle > Math.PI;
            var isCounterClockwise = lastBulge > 0.0;

            // find radius
            var oppositeLength = dist / 2.0;
            var radius = oppositeLength / Math.Sin(includedAngle / 2.0);

            return new SvgArcToPath(radius, radius, 0.0, isLargeArc, isCounterClockwise, nextX, nextY);
        }

        internal static SvgPath GetSvgPath(this DxfArc arc)
        {
            var startAngle = arc.StartAngle * Math.PI / 180.0;
            var endAngle = arc.EndAngle * Math.PI / 180.0;
            return SvgPath.FromEllipse(arc.Center.X, arc.Center.Y, arc.Radius, 0.0, 1.0, startAngle, endAngle);
        }


        internal static SvgPath GetSvgPath(this DxfCircle circle)
        {
            var arc1 = new DxfArc(circle.Center, circle.Radius, 0, 180);
            var arc2 = new DxfArc(circle.Center, circle.Radius, 180, 0);
            var svg1 = arc1.GetSvgPath();
            var svg2 = arc2.GetSvgPath();
            var segments = svg1.Segments.Concat(svg2.Segments);
            return new SvgPath(segments);
        }

        internal static SvgPath GetSvgPath(this DxfEllipse ellipse)
        {
            return SvgPath.FromEllipse(ellipse.Center.X, ellipse.Center.Y, ellipse.MajorAxis.X, ellipse.MajorAxis.Y, ellipse.MinorAxisRatio, ellipse.StartParameter, ellipse.EndParameter);
        }

        internal static SvgPath GetSvgPath(this DxfLwPolyline poly)
        {
            var first = poly.Vertices.First();
            var segments = new List<SvgPathSegment>();
            segments.Add(new SvgMoveToPath(first.X, first.Y));
            var last = first;
            foreach (var next in poly.Vertices.Skip(1))
            {
                segments.Add(FromPolylineVertices(last, next));
                last = next;
            }

            if (poly.IsClosed)
            {
                segments.Add(FromPolylineVertices(last, first));
            }

            return new SvgPath(segments);
        }

        //internal static SvgPath GetSvgPath(this DxfPolyline poly)
        //{
        //    var first = poly.Vertices.First();
        //    var segments = new List<SvgPathSegment>();
        //    segments.Add(new SvgMoveToPath(first.Location.X, first.Location.Y));
        //    var last = first;
        //    foreach (var next in poly.Vertices.Skip(1))
        //    {
        //        segments.Add(FromPolylineVertices(last, next));
        //        last = next;
        //    }

        //    if (poly.IsClosed)
        //    {
        //        segments.Add(FromPolylineVertices(last, first));
        //    }

        //    return new SvgPath(segments);
        //}

        //internal static SvgPath GetSvgPath(this IList<Bezier2> beziers)
        //{
        //    var first = beziers[0];
        //    var segments = new List<SvgPathSegment>();
        //    segments.Add(new SvgMoveToPath(first.Start.X, first.Start.Y));
        //    var last = first.Start;
        //    foreach (var next in beziers)
        //    {
        //        if (next.Start != last)
        //        {
        //            segments.Add(new SvgMoveToPath(next.Start.X, next.Start.Y));
        //        }

        //        segments.Add(new SvgCubicBezierToPath(next.Control1.X, next.Control1.Y, next.Control2.X, next.Control2.Y, next.End.X, next.End.Y));
        //        last = next.End;
        //    }

        //    return new SvgPath(segments);
        //}

        private static XElement AddStroke(this XElement element, DxfColor color)
        {
            if (color.IsIndex)
            {
                var colorString = color.ToRGBString();
                element.SetAttributeValue("stroke", colorString);
            }

            return element;
        }

        private static XElement AddStrokeWidth(this XElement element, double strokeWidth)
        {
            element.Add(new XAttribute("stroke-width", $"{Math.Max(strokeWidth, 1.0).ToDisplayString()}px"));
            return element;
        }

        private static XElement AddVectorEffect(this XElement element)
        {
            element.Add(new XAttribute("vector-effect", "non-scaling-stroke"));
            return element;
        }
    }
    public static class DoubleExtensions
    {
        public static bool IsCloseTo(this double a, double b)
        {
            return Math.Abs(a - b) < 1.0e-10;
        }
    }
    public class TestDxfReader
    {
        private readonly string _fileName;
        private readonly DxfFile _document;

        public TestDxfReader(string fileName)
        {
            _fileName = fileName;
            _document = DxfFile.Load(_fileName);
        }

        public Geometry GetTestGeometry()
        {
            var polyline = _document.Entities
                .OfType<DxfLwPolyline>()
                .Where(p=>p.Layer == "PAZ")
                .ElementAt(0);
            var g = new GeometryCollection();
            return polyline.ToGeometry();
        }

        public GeometryCollection GetTestGeomCollection()
        {
            var curves = _document.Entities
                .OfType<DxfLwPolyline>()
                .Select(p => p.ToGeometry());
            return new GeometryCollection(curves);
        }
    }
}
