namespace MachineClassLibrary.Laser
{
    public class CircleParamsAdapter : IParamsAdapting
    {

        private readonly PierceParams pierceParams;

        public CircleParamsAdapter(PierceParams pierceParams)
        {
            this.pierceParams = pierceParams;
        }

        public double[] Adapt()
        {
            //formula for calculating parameters
            var innerOffset = 0.001;
            var outerOffset = 0.001;
            var step = 0.001;

            var result = new double[] {innerOffset, outerOffset, step};
            return result;
        }
    }
}
