using MachineClassLibrary.Classes;
using System;

namespace MachineClassLibrary.Machine.Machines
{
    public interface IGeometryBuilder<TPlace> where TPlace : Enum
    {
        void Build();
        IGeometryBuilder<TPlace> SetCoordinateForPlace(Ax axis, double coordinate);
    }
}