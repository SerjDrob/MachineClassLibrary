using netDxf;
using System;
using System.Numerics;

namespace MachineClassLibrary.GeometryUtility
{
    public partial class CoorSystem<TPlaceEnum> where TPlaceEnum : Enum
    {
        public class WorkMatrixCoorSystemBuilder<TPlace> where TPlace : Enum
        {
            private Matrix3 _workMatrix;

            public WorkMatrixCoorSystemBuilder<TPlace> SetWorkMatrix(Matrix3x2 workMatrix)
            {

                _workMatrix = workMatrix.ConvertMatrix();
                return this;
            }
            public WorkMatrixCoorSystemBuilder<TPlace> SetWorkMatrix(float[] elements)
            {
                if (elements.Length != 6) throw new ArgumentException("The count of the elements must be 6");
                return SetWorkMatrix(new Matrix3x2(elements[0],
                                                   elements[1],
                                                   elements[2],
                                                   elements[3],
                                                   elements[4],
                                                   elements[5]));
            }
            
            public CoorSystem<TPlace> Build()
            {
                return new CoorSystem<TPlace>(_workMatrix);
            }
        }
    }
}

