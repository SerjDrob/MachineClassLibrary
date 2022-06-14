using MachineClassLibrary.Laser.Entities;
using System;
using System.IO;
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
        private readonly double _curveAngle;

        public DxfCurvePerforator(MarkLaserParams markLaserParams, IProcObject<DxfCurve> procObject, IParamsAdapting paramsAdapter, double curveAngle = 0)
        {
            _markLaserParams = markLaserParams;
            _procObject = procObject;
            _paramsAdapter = paramsAdapter;
            _curveAngle = curveAngle;
        }

        public async Task PierceObjectAsync()
        {
            var param = _paramsAdapter.Adapt();
            //var preparator = new CirclePreparator(param[0], param[1], param[2], _markLaserParams);
            //var entName = preparator.EntityPreparing(_procObject);

            var curve = _procObject.PObject;
            Lmc.SetPenParams(_markLaserParams.PenParams);
            
           
            Lmc.lmc1_AddFileToLib(curve.FilePath, "curve", 0, 0, 0, 0, 1, _markLaserParams.PenParams.nPenNo, false);
            Lmc.SetHatchParams(_markLaserParams.HatchParams);
            Lmc.lmc1_HatchEnt("curve", "curve");

            var hatch90 = _markLaserParams.HatchParams with { dHatchRotateAngle = 90 };

            Lmc.lmc1_AddFileToLib(curve.FilePath, "curve1", 0, 0, 0, 0, 1, _markLaserParams.PenParams.nPenNo, false);
            Lmc.SetHatchParams(hatch90);
            Lmc.lmc1_HatchEnt("curve1", "curve1");

            Lmc.lmc1_RotateEnt("curve", 0, 0, _curveAngle * 180 / Math.PI);
            Lmc.lmc1_RotateEnt("curve1", 0, 0, _curveAngle * 180 / Math.PI);




            await Task.Run(() =>
            {
                var result = Lmc.lmc1_MarkEntity("curve");
                result += Lmc.lmc1_MarkEntity("curve1");
                if (result != 0)
                {                    
                    Lmc.lmc1_DeleteEnt("curve");
                    Lmc.lmc1_DeleteEnt("curve1");

                    throw new Exception($"Marking failed with code {(Lmc.EzCad_Error_Code)result}");
                }
            }
            );
            Lmc.lmc1_SaveEntLibToFile("D:/testCurve.ezd");
            Lmc.lmc1_DeleteEnt("curve");
            Lmc.lmc1_DeleteEnt("curve1");
        }
    }
}