using MachineClassLibrary.Laser.Entities;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Text;


namespace MachineClassLibrary.Laser
{
    public class CirclePreparator : IMarkEntityPreparing<Circle>
    {
        private const string namePrefix = "circle";
        private readonly double _innerOffset;
        private readonly double _outerOffset;
        private readonly double _step;
        private readonly HatchParams _hatchParams;
        private readonly PenParams _penParams;

        public CirclePreparator(double innerOffset, double outerOffset, double step, MarkLaserParams markParams)
        {
            Guard.IsNotNull(markParams, nameof(markParams));
            Guard.IsGreaterThan(innerOffset, 0, $"{nameof(innerOffset)} must be positive");
            Guard.IsGreaterThan(outerOffset, 0, $"{nameof(outerOffset)} must be positive");
            this._innerOffset = innerOffset;
            this._outerOffset = outerOffset;
            this._step = step;
            _penParams = markParams.PenParams;
            _hatchParams = markParams.HatchParams;
        }

        public string EntityPreparing(IProcObject<Circle> procObject)
        {
            var rand = new Random();
            var builder = new StringBuilder();
            var penNo = 0;
            var name = builder.AppendJoin("-",namePrefix, rand.Next(1000,10000));
            var name1 = builder.AppendJoin("-", namePrefix, rand.Next(1000, 10000));
            int result = 0;
            result = Lmc.lmc1_AddCircleToLib(0, 0, procObject.PObject.Radius + _outerOffset, name.ToString(), 0);
            if (_innerOffset<procObject.PObject.Radius)
            {
                result = Lmc.lmc1_AddCircleToLib(0, 0, procObject.PObject.Radius - _innerOffset, name1.ToString(), 0);
                result = Lmc.lmc1_GroupEnt(name.ToString(), name1.ToString(), name.ToString(), 0);
            }
            result = Lmc.SetPenParams(_penParams with { PenNo = penNo});
            if (result != 0)
            {
                throw new Exception($"Set pen params failed with {(Lmc.EzCad_Error_Code)result}");
            }
            //result = Lmc.HatchObject(name.ToString(), _hatchParams with { nPenNo = penNo});
            result = Lmc.SetHatchParams(_hatchParams with { PenNo = penNo });            
            if (result != 0)
            {
                throw new Exception($"Set hatch params failed with {(Lmc.EzCad_Error_Code)result}");
            }
            result = Lmc.lmc1_HatchEnt(name.ToString(), name.ToString());
            if (result != 0)
            {
                throw new Exception($"Hatch entity failed with {(Lmc.EzCad_Error_Code)result}");
            }
            return name.ToString();
        }
    }
}
