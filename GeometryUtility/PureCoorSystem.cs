using netDxf;
using System;

namespace MachineClassLibrary.GeometryUtility
{
    public partial class PureCoorSystem<TPlace>:CoorSystemBase<TPlace> where TPlace : Enum
    {
        public PureCoorSystem(Matrix3 matrix):base(matrix) { }
        public TwoPointCoorSystemBuilder GetTwoPointSystemBuilder() => new TwoPointCoorSystemBuilder(_workTransformation);
    }
}

