using MachineClassLibrary.Laser.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MachineClassLibrary.Classes
{
    public class DxfEditor : IDxfReader
    {
        private readonly IDxfReader _dxfReader;

        public DxfEditor(IDxfReader dxfReader)
        {
            _dxfReader = dxfReader;
        }

        public IEnumerable<PCurve> GetAllCurves(string fromLayer = null)
        {
            return _dxfReader.GetAllCurves(fromLayer)
                .Where(Satisfy);
        }

        public IEnumerable<PLine> GetAllSegments()
        {
            return _dxfReader.GetAllSegments()
                .Where(Satisfy);
        }

        public IEnumerable<PCircle> GetCircles(string fromLayer = null)
        {
            return _dxfReader.GetCircles(fromLayer)
                .Where(Satisfy);
        }

        public IDictionary<string, int> GetLayers()
        {
            return _dxfReader.GetLayers();
        }

        public IDictionary<string, IEnumerable<(string objType, int count)>> GetLayersStructure()
        {
            return _dxfReader.GetLayersStructure();
        }

        public IEnumerable<PLine> GetLines()
        {
            return _dxfReader.GetLines()
                .Where(Satisfy);
        }

        public IEnumerable<IProcObject> GetObjectsFromLayer<TObject>(string layerName) where TObject : IProcObject
        {
            return _dxfReader.GetObjectsFromLayer<TObject>(layerName).Where(Satisfy);
        }

        public IEnumerable<PPoint> GetPoints()
        {
            return _dxfReader.GetPoints().Where(Satisfy);
        }

        public (double width, double height) GetSize()
        {
            return _dxfReader.GetSize();
        }

        public void WriteCircleToFile(string filePath, Circle circle)
        {
            _dxfReader.WriteCircleToFile(filePath, circle);
        }

        public void WriteCurveToFile(string filePath, Curve curve, bool isClosed)
        {
            _dxfReader.WriteCurveToFile(filePath, curve, isClosed);
        }


        //-------------------------------

        private bool Satisfy(IProcObject procObject)
        {
            return !_erasedObjects?.Where(e => e.selection.Contains(procObject.GetBoundingBox()))
                .Where(e => e.layers.Any(l => l == procObject.LayerName))
                .Any() ?? true;
        }

        private Stack<(string[] layers, Rect selection)> _erasedObjects;
        public void RemoveBySelection(string layerName, Rect selection)
        {
            _erasedObjects.Push((new[] { layerName }, selection));
        }
        public void RemoveBySelection(string[] layers, Rect selection)
        {
            _erasedObjects ??= new();
            _erasedObjects.Push((layers, selection));
        }

        public void Undo()
        {
            _erasedObjects.TryPop(out var values);
        }
        public void Reset()
        {
            _erasedObjects?.Clear();
        }
    }

}
