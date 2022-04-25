using MachineClassLibrary.Laser.Entities;
using System.Threading.Tasks;

namespace MachineClassLibrary.Laser
{
    internal class CurvePerforator : IPerforating
    {
        private readonly MarkLaserParams _markLaserParams;
        private readonly IProcObject<Curve> _procObject;
        private readonly IParamsAdapting _paramsAdapter;

        public CurvePerforator(MarkLaserParams markLaserParams, IProcObject<Curve> procObject, IParamsAdapting paramsAdapter)
        {
            _markLaserParams = markLaserParams;
            _procObject = procObject;
            _paramsAdapter = paramsAdapter;
        }

        public Task PierceObjectAsync()
        {
            //var param = _paramsAdapter.Adapt();
            //var preparator = new CirclePreparator(param[0], param[1], param[2], _markLaserParams);
            //var entName = preparator.EntityPreparing(_procObject);



            //await Task.Run(() => {
            //    var result = Lmc.lmc1_MarkEntity(entName);
            //    if (result != 0)
            //    {
            //        Lmc.lmc1_DeleteEnt(entName);
            //        throw new Exception($"Marking faild with code {result}");
            //    }
            //}
            //);
            //Lmc.lmc1_DeleteEnt(entName);
            throw new System.NotImplementedException();
        }
    }
}