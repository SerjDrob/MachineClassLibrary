namespace MachineClassLibrary.Laser
{
    public class MarkLaserParams
    {
        public MarkLaserParams(PenParams penParams, HatchParams hatchParams)
        {
            PenParams = penParams;
            HatchParams = hatchParams;
        }

        public PenParams PenParams { get; init; }
        public HatchParams HatchParams { get; init; }
    }
}
