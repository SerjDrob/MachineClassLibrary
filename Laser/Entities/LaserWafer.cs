using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineClassLibrary.Laser.Entities
{
    public class LaserWafer<TObject> : IEnumerable<IProcObject<TObject>>, IDisposable
    {
        private readonly (double x, double y) _size;
        private readonly IEnumerable<IProcObject<TObject>> _procObjects;
        private Matrix _matrix;
        public LaserWafer(IEnumerable<IProcObject<TObject>> procObjects, (double x, double y) size)// add scale here
        {
            Guard.IsNotNull(procObjects, nameof(procObjects));
            Guard.IsGreaterThan(size.x,0,nameof(size.x));
            Guard.IsGreaterThan(size.y, 0, nameof(size.y));

            _procObjects = procObjects;
            _size = size;   
            _matrix = new Matrix();            
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
            _matrix.Rotate(90);
            _matrix.Translate(0, (float)_size.y);
            return this;
        }
        public LaserWafer<TObject> MirrorX()
        {
            _matrix.Scale(-1, 1);
            _matrix.Translate((float)_size.x, 0);
            return this;
        }
        public LaserWafer<TObject> MirrorY()
        {
            _matrix.Scale(1, -1);
            _matrix.Translate(0, (float)_size.y);
            return this;
        }
        public LaserWafer<TObject> Scale(float scale)
        {
            _matrix.Scale(scale, scale);
            return this;
        }

        public IEnumerator<IProcObject<TObject>> GetEnumerator()
        {
            foreach (var pobject in _procObjects)
            {
                var points = new PointF[] { new((float)pobject.X, (float)pobject.Y) };
                _matrix.TransformPoints(points);
                yield return pobject.CloneWithPosition(points[0].X, points[0].Y);
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
