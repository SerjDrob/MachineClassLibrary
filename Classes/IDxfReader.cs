﻿using MachineClassLibrary.Laser.Entities;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;

namespace MachineClassLibrary.Classes
{
    public interface IDxfReader
    {
        (double width, double height) GetSize();
        (PointF minPoint, PointF maxPoint) GetSize2();
        IEnumerable<PCircle> GetCircles(string fromLayer = null);
        IEnumerable<PLine> GetLines();
        IEnumerable<PPoint> GetPoints();
        /// <summary>
        /// Gets a list of dxf layers
        /// </summary>
        /// <returns>Dictionary where Key is the name of the layer and Value is its color </returns>
        IDictionary<string, int> GetLayers();
        IEnumerable<PLine> GetAllSegments();
        /// <summary>
        /// Gets structure of the file's layers presented by dictionary. 
        /// </summary>
        /// <returns>Dictionary  where a layer's name is a Key and Value is a list of the layer's object names with their count on the layer.</returns>
        IDictionary<string, IEnumerable<(string objType, int count)>> GetLayersStructure();
        IEnumerable<PCurve> GetAllCurves(string fromLayer = null);
        void WriteCurveToFile(string filePath, Curve curve, bool isClosed);
        void WriteCircleToFile(string filePath, Circle circle);
        void WriteShapesToFile(string filePath, params IShape[] shapes);
        IEnumerable<IProcObject> GetObjectsFromLayer<TObject>(string layerName) where TObject : IProcObject;
    }
}
