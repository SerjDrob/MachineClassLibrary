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
            public CoorSystem<TPlace> Build()
            {
                return new CoorSystem<TPlace>(_workMatrix);
            }
        }
    }
}

