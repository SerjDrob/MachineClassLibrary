using MachineClassLibrary.Classes;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;

namespace MachineClassLibrary.Laser.Entities
{
    public class LaserWafer : IEnumerable<IProcObject>, 
        IDisposable, 
        ITransformable,
        ICollection<IProcObject>
    {
        private readonly (double x, double y) _size;
        private readonly List<IProcObject> _procObjects;
        private Matrix _matrix;
        private bool _turned90;
        private bool _mirroredX;
        private bool _mirroredY;
        private float _scale = 1;
        private float _offsetX = 0;
        private float _offsetY = 0;
        private bool _shuffleEnumeration;
        private RectangleF _restrictingArea;
        private bool _restricted = false;

        public int Count => _procObjects.Count();

        public bool IsReadOnly => throw new NotImplementedException();

        public LaserWafer(IEnumerable<IProcObject> procObjects, (double x, double y) size)//TODO add scale here
        {
            Guard.IsNotNull(procObjects, nameof(procObjects));
            Guard.IsGreaterThan(size.x, 0, nameof(size.x));
            Guard.IsGreaterThan(size.y, 0, nameof(size.y));

            _procObjects = procObjects.ToList();
            _size = size;
            _matrix = new Matrix();
            _turned90 = false;
            _mirroredX = false;
            _mirroredY = false;
        }

        /// <summary>
        /// Create empty wafer
        /// </summary>
        /// <param name="size">size of the wafer</param>
        public LaserWafer((double x, double y) size)
        {
            Guard.IsGreaterThan(size.x, 0, nameof(size.x));
            Guard.IsGreaterThan(size.y, 0, nameof(size.y));

            _procObjects = new();
            _size = size;
            _matrix = new Matrix();
            _turned90 = false;
            _mirroredX = false;
            _mirroredY = false;
        }

        /// <summary>
        /// ^
        /// |____.
        /// |____|___>
        /// 
        /// Transform to:
        /// 
        /// ^
        /// |__
        /// |  |
        /// |__|.___>
        /// 
        /// </summary>
        /// <returns>LaserWafer<TObject></returns>
        public void Turn90()
        {
            _turned90 ^= true;
        }
        public void OffsetX(float offset)
        {
            _offsetX = offset;
        }
        public void OffsetY(float offset)
        {
            _offsetY = offset;
        }
        public void MirrorX()
        {
            _mirroredX ^= true;
        }
        public void MirrorY()
        {
            _mirroredY ^= true;
        }
        public void Scale(float scale)
        {
            _scale *= scale;
        }
        public void SetRestrictingArea(double x, double y, double width, double height)
        {
            _restrictingArea = new RectangleF((float)x, (float)y, (float)width, (float)height);
            _restricted = true;
        }
        public void SetEnumerationStyle(bool shuffle) => _shuffleEnumeration = shuffle;
        public IEnumerator<IProcObject> GetEnumerator()
        {
            _matrix = GetCurrentTransformation();

            var objects = _shuffleEnumeration ? _procObjects.Shuffle() : _procObjects;

            foreach (var pobject in objects)
            {
                var points = new PointF[] { new((float)pobject.X, (float)pobject.Y) };
                _matrix.TransformPoints(points);
                if (!_restricted || _restrictingArea.Contains(points[0]))
                {
                    var newObj = pobject.CloneWithPosition(points[0].X, points[0].Y);
                    newObj.Scale(_scale);
                    newObj.SetMirrorX(_mirroredX);
                    newObj.SetTurn90(_turned90);
                    yield return newObj;
                }
            }
        }

        public IProcObject GetProcObjectToWafer(IProcObject procObject)
        {
            var matrix = GetCurrentTransformation();
            var points = new PointF[] { new((float)procObject.X, (float)procObject.Y) };
            matrix.TransformPoints(points);
            
            var newObj = procObject.CloneWithPosition(points[0].X, points[0].Y);
            newObj.Scale(_scale);
            newObj.SetMirrorX(_mirroredX);
            newObj.SetTurn90(_turned90);
            return newObj;
        }

        public PointF GetPointToWafer(PointF point)
        {
            var matrix = GetCurrentTransformation();
            var points = new PointF[] { point };
            matrix.TransformPoints(points);

            return points[0];
        }

        public PointF GetPointFromWafer(PointF point)
        {
            var matrix = GetCurrentTransformation();
            matrix.Invert();
            var points = new PointF[] { point };
            matrix.TransformPoints(points);

            return points[0];
        }


        private Matrix GetCurrentTransformation()
        {
            var transformation = Matrix3x2.Identity;

            if (_mirroredX)
            {
                var mirror = Matrix3x2.CreateScale(-1, 1);
                var translation = Matrix3x2.CreateTranslation((float)_size.x, 0);
                transformation *= mirror * translation;
            }
            if (_mirroredY)
            {
                var mirror = Matrix3x2.CreateScale(1, -1);
                var translation = Matrix3x2.CreateTranslation(0, (float)_size.y);
                transformation *= mirror * translation;
            }
            if (_turned90)
            {
                var rotation = Matrix3x2.CreateRotation(MathF.PI * 90 / 180);
                var translation = Matrix3x2.CreateTranslation((float)_size.y, 0);
                transformation *= rotation * translation;
            }
            var scaling = Matrix3x2.CreateScale(_scale);
            transformation *= scaling;

            if (_offsetX != 0 || _offsetY != 0)
            {
                var translating = Matrix3x2.CreateTranslation(_offsetX, _offsetY);
                transformation *= translating;
            }

            return new Matrix(transformation);
        }

        public IProcObject this[int index]
        {
            get
            {
                var enumerator = GetEnumerator();
                var current = -1;
                while (enumerator.MoveNext() & current != index) current++;
                return enumerator.Current;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_matrix != null)
                {
                    _matrix.Dispose();
                    _matrix = null;
                }
            }
        }

        public void Add(IProcObject item)
        {
            _procObjects.Add(item);
        }

        public void Clear()
        {
            _procObjects.Clear();
        }

        public bool Contains(IProcObject item)
        {
            return _procObjects.Contains(item);
        }

        public void CopyTo(IProcObject[] array, int arrayIndex)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = this[arrayIndex++];
            }
        }


        public bool Remove(IProcObject item)
        {
            return _procObjects.Remove(item);
        }
    }
}
