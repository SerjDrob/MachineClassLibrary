using MachineClassLibrary.Laser.Entities;
using System;
using System.Threading.Tasks;


namespace MachineClassLibrary.Laser
{
    public class CirclePerforator : IPerforating
    {
        private readonly MarkLaserParams _markLaserParams;        
        private readonly IProcObject<Circle> _procObject;
        private readonly IParamsAdapting _paramsAdapter;

        public CirclePerforator(MarkLaserParams markLaserParams, IProcObject<Circle> procObject, IParamsAdapting paramsAdapting)
        {
            _markLaserParams = markLaserParams;            
            _procObject = procObject;
            _paramsAdapter = paramsAdapting;
        }

        public async Task PierceObjectAsync()
        {
            var param = _paramsAdapter.Adapt();            
            var preparator = new CirclePreparator(param[0], param[1], param[2], _markLaserParams);
            var entName = preparator.EntityPreparing(_procObject);
            await Task.Run(() => {
                var result = Lmc.lmc1_MarkEntity(entName);
                if (result!=0)
                {
                    Lmc.lmc1_DeleteEnt(entName);
                    throw new Exception($"Marking faild with code {result}");
                }
                }
            );
            Lmc.lmc1_DeleteEnt(entName);
        }
    }
}
