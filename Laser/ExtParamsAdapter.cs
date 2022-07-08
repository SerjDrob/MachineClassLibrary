using MachineClassLibrary.Laser.Parameters;

namespace MachineClassLibrary.Laser
{
    public class ExtParamsAdapter
    {
        private readonly ExtendedParams _extendedParams;

        public ExtParamsAdapter(ExtendedParams extendedParams)
        {
            _extendedParams = extendedParams;
        }
        public MarkLaserParams MixParams(MarkLaserParams markLaserParams)
        {
            var penParams = markLaserParams.PenParams with
            {
                MarkLoop = _extendedParams.MarkLoop,
                MarkSpeed = _extendedParams.MarkSpeed,
                PowerRatio = _extendedParams.PowerRatio,
                Freq = _extendedParams.Freq,
                QPulseWidth = _extendedParams.QPulseWidth,
                IsModulated = _extendedParams.EnablePWM,
                ModFreq=_extendedParams.PWMFrequency,
                ModDutyCycle=_extendedParams.PWMDutyCycle
            };
            var hatchParams = markLaserParams.HatchParams with
            {
                HatchLineDist = _extendedParams.HatchLineDistance / 1000d,
                EnableHatch = _extendedParams.EnableHatch
            };

            return new MarkLaserParams(penParams, hatchParams);
        }
    }
}
