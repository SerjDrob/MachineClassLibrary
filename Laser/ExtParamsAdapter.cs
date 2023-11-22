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
                EnableContour = _extendedParams.EnableHatch ? _extendedParams.EnableContour : true,
                HatchStartOffset = _extendedParams.EnableHatch & _extendedParams.EnableMilling ? _extendedParams.ContourOffset / 1000d : 0,
                HatchLineDist = _extendedParams.HatchLineDistance / 1000d,
                EnableHatch = _extendedParams.EnableHatch,
                HatchAttribute = _extendedParams.HatchAttribute,
                HatchEdgeDist = _extendedParams.HatchEdgeDist / 1000d,
                HatchContourFirst = _extendedParams.HatchContourFirst,
                HatchEdge = false,//TODO add it extended params
                HatchAutoRotate = _extendedParams.HatchAutoRotate,
                HatchRotateAngle = _extendedParams.HatchRotateAngle
            };

            return new MarkLaserParams(penParams, hatchParams);
        }
    }
}
