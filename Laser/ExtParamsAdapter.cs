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
                QPulseWidth = _extendedParams.QPulseWidth
            };
            var hatchParams = markLaserParams.HatchParams with
            {
                HatchLineDist = _extendedParams.HatchLineDistance / 1000d
            };

            return new MarkLaserParams(penParams, hatchParams);
        }
    }
}
