using MachineClassLibrary.Laser.Entities;
using System;
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
    internal class DxfCurvePerforator : IPerforating
    {
        private readonly MarkLaserParams _markLaserParams;
        private readonly IProcObject<DxfCurve> _procObject;
        private readonly IParamsAdapting _paramsAdapter;

        public DxfCurvePerforator(MarkLaserParams markLaserParams, IProcObject<DxfCurve> procObject, IParamsAdapting paramsAdapter)
        {
            _markLaserParams = markLaserParams;
            _procObject = procObject;
            _paramsAdapter = paramsAdapter;
        }

        public async Task PierceObjectAsync()
        {
            var param = _paramsAdapter.Adapt();
            //var preparator = new CirclePreparator(param[0], param[1], param[2], _markLaserParams);
            //var entName = preparator.EntityPreparing(_procObject);

            var curve = _procObject.PObject;

            Lmc.lmc1_AddFileToLib(curve.FilePath, "curve", 0, 0, 0, 0, curve.Scaling, _markLaserParams.PenParams.nPenNo, false);
            Lmc.lmc1_MirrorEnt("curve", 0, 0, curve.MirrorX, false);
            Lmc.lmc1_RotateEnt("curve", 0, 0, curve.Turn90 ? 90 : 0);

            await Task.Run(() =>
            {
                var result = Lmc.lmc1_MarkEntity("curve");
                if (result != 0)
                {
                    Lmc.lmc1_DeleteEnt("curve");
                    throw new Exception($"Marking failed with code {(Lmc.EzCad_Error_Code)result}");
                }
            }
            );
            Lmc.lmc1_DeleteEnt("curve");
        }
    }
}