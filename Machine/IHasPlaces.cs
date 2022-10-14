using MachineClassLibrary.Classes;
using MachineClassLibrary.Machine.Machines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineClassLibrary.Machine
{
    public interface IHasPlaces<TPlace> where TPlace : Enum
    {
        public double GetGeometry(TPlace place, int arrNum);
        public double GetGeometry(TPlace place, Ax axis);
        public Task GoThereAsync(TPlace place, bool precisely = false);
        public Task MoveGpInPlaceAsync(Groups group, TPlace place, bool precisely = false);
        public Task MoveAxesInPlaceAsync(TPlace place);
        public (Ax, double)[] TranslateActualCoors(TPlace place);
        public double TranslateActualCoors(TPlace place, Ax axis);
        public (Ax, double)[] TranslateActualCoors(TPlace place, (Ax axis, double pos)[] position);
        public double TranslateSpecCoor(TPlace place, double position, Ax axis);
        public void ConfigureGeometry(Dictionary<TPlace, (Ax, double)[]> places);//TODO take it away
        public void ConfigureGeometry(Dictionary<TPlace, double> places);//TODO take it away
        IGeometryBuilder<TPlace> ConfigureGeometryFor(TPlace place);
    }
}
