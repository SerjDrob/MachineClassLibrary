namespace MachineClassLibrary.Laser
{
    public class CurveParamsAdapter : IParamsAdapting
    {
        private readonly PierceParams pierceParams;

        public CurveParamsAdapter(PierceParams pierceParams)
        {
            this.pierceParams = pierceParams;
        }
        public double[] Adapt()
        {
            return new double[] { 0.001, 0.001, 0.001 };
        }
    }   
}
