using MachineClassLibrary.Laser.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MachineClassLibrary.Laser
{
    public interface ITechnologySequence
    {
        public Task PerformSequence<T>(IProcObject<T> procObject, IPerforating perforator) where T:IShape;
    }
}