using MachineClassLibrary.Laser.Entities;
using System;

namespace MachineClassLibrary.Laser
{
    public class PerforatorBuilder<T> : IPerforatorBuilder where T : IShape
    {
        private readonly IProcObject<T> _procObject;
        private readonly MarkLaserParams _markLaserParams;
        private readonly IParamsAdapting _paramsAdapting;

        public PerforatorBuilder(IProcObject<T> procObject, MarkLaserParams markLaserParams, IParamsAdapting paramsAdapting)
        {
            _procObject = procObject;
            _markLaserParams = markLaserParams;
            _paramsAdapting = paramsAdapting;
        }

        public IPerforating Build()
        {

            if (typeof(T) == typeof(Circle))
            {
                return new CirclePerforator(_markLaserParams, (IProcObject<Circle>)_procObject, _paramsAdapting);
            }
            //if (typeof(T) is Curve)
            //{
            //    return new CurvePerforator(_hatchParams, (IProcObject<Curve>)_procObject, _paramsAdapting);
            //}
            else
            {
                throw new ArgumentException($"{typeof(T)} is mismatch to Circle or Curve");
            }
            
        }
    }
}