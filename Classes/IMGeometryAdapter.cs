using IxMilia.Dxf;
using IxMilia.Dxf.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace MachineClassLibrary.Classes
{
    internal static class IMDxfExtensions
    {
        public static DxfEllipse ToEllipse(this DxfCircle dxfCircle)
        {
            return new DxfEllipse(dxfCircle.Center, new DxfVector(dxfCircle.Radius, 0, 0), 1)
            {
                Color = dxfCircle.Color,
                Color24Bit = dxfCircle.Color24Bit,  
                ColorName = dxfCircle.ColorName,
                Layer = dxfCircle.Layer
            };
        }
    }
    public class IMGeometryAdapter : IGeometryAdapter
    {
        private readonly DxfFile _document;
        //private readonly IList<DxfEntity> _dxfEntities;


        public IMGeometryAdapter(string fileName)
        {
            _document = DxfFile.Load(fileName);
            //_dxfEntities = _document.Entities.ToList();
        }
        public IMGeometryAdapter(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            _document = DxfFile.Load(stream);
        }
       
        public IMGeometryAdapter(DxfFile document)
        {
            _document = document;
        }

        public GeometryCollection Geometries { get => new GeometryCollection(GetGeometies()); }

        public IEnumerable<Geometry> GetGeometies()
        {
            return _document.Entities
                .OfType<DxfLwPolyline>()
                .Select(p => p.ToGeometry())
                .Concat(
                    _document.Entities
                    .OfType<DxfCircle>()
                    .Select(c => c.ToGeometry())
                );
        }

        public IEnumerable<AdaptedGeometry> GetGeometries()
        {
            var layers = _document.Layers.ToDictionary(l => l.Name);
                       

            return _document.Entities.
                 OfType<DxfLwPolyline>()
                .Select(p => new AdaptedGeometry(p.ToGeometry(), p.Layer,
                GetColorFromArgb(layers[p.Layer].Color.ToRGB()), 
                GetColorFromArgb(p.Color.ToRGB())))
                .Concat(
                    _document.Entities
                    .OfType<DxfCircle>()
                      .Select(p => new AdaptedGeometry(p.ToGeometry(), p.Layer,
                      GetColorFromArgb(layers[p.Layer].Color.ToRGB()),
                      GetColorFromArgb(p.Color.ToRGB())))
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
