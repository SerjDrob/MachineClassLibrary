using MachineClassLibrary.Laser.Entities;
using System.Collections.Generic;
using System.Drawing;

namespace MachineClassLibrary.Classes
{
    public interface IDxfReader
    {
        (double width, double height) GetSize();
        IEnumerable<PCircle> GetCircles();
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
        IEnumerable<PCurve> GetAllCurves();
        IEnumerable<PDxfCurve> GetAllDxfCurves(string folder, string fromLayer);
        void WriteCurveToFile(string filePath, Curve curve, bool isClosed);
        IEnumerable<PDxfCurve2> GetAllDxfCurves2(string folder, string fromLayer);
    }
}