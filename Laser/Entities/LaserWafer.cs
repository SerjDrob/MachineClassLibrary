using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;

namespace MachineClassLibrary.Laser.Entities
{
    public class LaserWafer<TObject> : IEnumerable<IProcObject<TObject>>, IDisposable
    {
        private readonly (double x, double y) _size;
        private readonly IEnumerable<IProcObject<TObject>> _procObjects;
        private Matrix _matrix;
        private bool _turned90;
        private bool _mirroredX;
        private bool _mirroredY;
        private float _scale = 1;
        private float _offsetX = 0;
        private float _offsetY = 0;

        public LaserWafer(IEnumerable<IProcObject<TObject>> procObjects, (double x, double y) size)// add scale here
        {
            Guard.IsNotNull(procObjects, nameof(procObjects));
            Guard.IsGreaterThan(size.x, 0, nameof(size.x));
            Guard.IsGreaterThan(size.y, 0, nameof(size.y));

            _procObjects = procObjects;
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
        public LaserWafer<TObject> Turn90()
        {
            _turned90 ^= true;
            return this;
        }
        public LaserWafer<TObject> OffsetX(float offset)
        {
            _offsetX = offset;
            return this;
        }
        public LaserWafer<TObject> OffsetY(float offset)
        {
            _offsetY = offset;
            return this;
        }
        public LaserWafer<TObject> MirrorX()
        {
            _mirroredX ^= true;
            return this;
        }
        public LaserWafer<TObject> MirrorY()
        {
            _mirroredY ^= true;
            return this;
        }
        public LaserWafer<TObject> Scale(float scale)
        {
            _scale *= scale;
            return this;
        }

        public IEnumerator<IProcObject<TObject>> GetEnumerator()
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

            _matrix = new Matrix(transformation);

            foreach (var pobject in _procObjects)
            {
                var points = new PointF[] { new((float)pobject.X, (float)pobject.Y) };
                _matrix.TransformPoints(points);
                yield return pobject.CloneWithPosition(points[0].X, points[0].Y);
            }
        }
        public IProcObject<TObject> this[int index]
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
    }
}
