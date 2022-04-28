using MachineClassLibrary.Laser.Entities;
using System;

namespace MachineClassLibrary.Laser
{

    public class PerforatorFactory<T> : IPerforatorBuilder where T : IShape
    {
        private readonly IProcObject<T> _procObject;
        private readonly MarkLaserParams _markLaserParams;
        private readonly IParamsAdapting _paramsAdapting;

        public PerforatorFactory(IProcObject<T> procObject, MarkLaserParams markLaserParams, IParamsAdapting paramsAdapting)
        {
            _procObject = procObject;
            _markLaserParams = markLaserParams;
            _paramsAdapting = paramsAdapting;
        }


        public IPerforating GetPerforator()
        {

            if (typeof(T) == typeof(Circle))
            {
                return new CirclePerforator(_markLaserParams, (IProcObject<Circle>)_procObject, _paramsAdapting);
            }
            if (typeof(T) == typeof(Curve))
            {
                return new CurvePerforator(_markLaserParams, (IProcObject<Curve>)_procObject, _paramsAdapting);
            }
            if (typeof(T) == typeof(DxfCurve))
            {
                return new DxfCurvePerforator(_markLaserParams, (IProcObject<DxfCurve>)_procObject, _paramsAdapting);
            }
            else
            {
                throw new ArgumentException($"{typeof(T)} is mismatch to Circle or Curve");
            }            
        }

        public IPerforating GetPerforator(double angle)
        {
            if (typeof(T) == typeof(DxfCurve))
            {
                return new DxfCurvePerforator(_markLaserParams, (IProcObject<DxfCurve>)_procObject, _paramsAdapting, angle);
            }
            else
            {
                return GetPerforator();
            }
        }
    }
}