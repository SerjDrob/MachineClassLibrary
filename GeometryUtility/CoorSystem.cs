using Microsoft.Toolkit.Diagnostics;
using netDxf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace MachineClassLibrary.GeometryUtility
{
    public partial class CoorSystem<TPlaceEnum> : CoorSystemBase<TPlaceEnum> where TPlaceEnum : Enum
    {
        private CoorSystem(Matrix3 mainMatrix) : base(mainMatrix) { }
        public static ThreePointCoorSystemBuilder GetThreePointSystemBuilder() => new ThreePointCoorSystemBuilder();
        public static WorkMatrixCoorSystemBuilder<TPlaceEnum> GetWorkMatrixSystemBuilder() => new WorkMatrixCoorSystemBuilder<TPlaceEnum>();
        public static CoorSystem<TPlaceEnum> GetFromSystem(ICoorSystem coorSystem) => 
            GetWorkMatrixSystemBuilder()
            .SetWorkMatrix(coorSystem.GetMainMatrixElements())
            .Build();

    }
}

