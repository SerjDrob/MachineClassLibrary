using netDxf;
using System;
using System.Numerics;

namespace MachineClassLibrary.GeometryUtility
{
    public partial class CoorSystemBase<TPlaceEnum> : ICoorSystem<TPlaceEnum> where TPlaceEnum : Enum
    {
        public class RelatedSystemBuilder<TPlace> where TPlace : Enum
        {
            private Matrix3 _mainMatrix;
            private readonly ICoorSystem<TPlace> _parentSystem;

            public RelatedSystemBuilder(Matrix3x2 mainMatrix, ICoorSystem<TPlace> parentSystem)
            {
                _mainMatrix = mainMatrix.ConvertMatrix();
                _parentSystem = parentSystem;
            }
            /// <summary>
            /// Rotate initial matrix by the angle
            /// </summary>
            /// <param name="angle">Rotation angle in radian</param>
            /// <returns>RelatedSystemBuilder</returns>
            public RelatedSystemBuilder<TPlace> Rotate(double angle)
            {
                var R = new Matrix3(m11: Math.Cos(angle), m12: -Math.Sin(angle), m13: 0,
                                    m21: Math.Sin(angle), m22: Math.Cos(angle), m23: 0,
                                    m31: 0, m32: 0, m33: 1);
                _mainMatrix = R * _mainMatrix;
                return this;
            }
            public RelatedSystemBuilder<TPlace> Translate(double offsetX, double offsetY)
            {
                var Translate = new Matrix3(m11: 1, m12: 0, m13: offsetX,
                                            m21: 0, m22: 1, m23: offsetY,
                                            m31: 0, m32: 0, m33: 1);
                _mainMatrix = Translate * _mainMatrix;
                return this;
            }
            public RelatedSystemBuilder<TPlace> Scale(double scale)
            {
                var Translate = new Matrix3(m11: scale, m12: 0, m13: 0,
                                                  m21: 0, m22: scale, m23: 0,
                                                  m31: 0, m32: 0, m33: 1);
                _mainMatrix = Translate * _mainMatrix;
                return this;
            }
            public void Build(TPlace place)
            {
                _parentSystem.SetRelatedSystem(place, _mainMatrix.ConvertMatrix());
            }
        }
    }
}

