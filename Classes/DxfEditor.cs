using MachineClassLibrary.Laser.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
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
                .Where(DissatisfySelection);
        }

        public IEnumerable<PLine> GetAllSegments()
        {
            return _dxfReader.GetAllSegments()
                .Where(DissatisfySelection);
        }

        public IEnumerable<PCircle> GetCircles(string fromLayer = null)
        {
            return _dxfReader.GetCircles(fromLayer)
                .Where(DissatisfySelection);
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
                .Where(DissatisfySelection);
        }

        public IEnumerable<IProcObject> GetObjectsFromLayer<TObject>(string layerName) where TObject : IProcObject
        {
            return _dxfReader.GetObjectsFromLayer<TObject>(layerName).Where(DissatisfySelection);
        }

        public IEnumerable<PPoint> GetPoints()
        {
            return _dxfReader.GetPoints().Where(DissatisfySelection);
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
        public event EventHandler<bool> CanUndoChanged;
        private bool DissatisfySelection(IProcObject procObject)
        {
            var res =  !_erasedObjects?.Where(e => e.selection.Contains(procObject.GetBoundingBox()))
                .Where(e => e.layers.Any(l => l == procObject.LayerName))
                .Any() ?? true;
            return res;
        }

        private Stack<(string[] layers, Rect selection)> _erasedObjects;

        public void RemoveBySelection(string layerName, Rect selection)
        {
            _erasedObjects.Push((new[] { layerName }, selection));
            CanUndoChanged?.Invoke(this, true);
        }
        public void RemoveBySelection(string[] layers, Rect selection)
        {
            _erasedObjects ??= new();
            _erasedObjects.Push((layers, selection));
            CanUndoChanged?.Invoke(this, true);
        }
        public void Undo()
        {
            _erasedObjects?.TryPop(out _);
            var canUndo = _erasedObjects?.Any() ?? false;
            CanUndoChanged?.Invoke(this, canUndo);
        }
        public void Reset()
        {
            _erasedObjects?.Clear();
        }

        public void WriteShapesToFile(string filePath, params IShape[] shapes)
        {
            _dxfReader?.WriteShapesToFile(filePath, shapes);
        }

        public (PointF minPoint, PointF maxPoint) GetSize2()
        {
            return _dxfReader.GetSize2();
        }
    }

}
