namespace MachineClassLibrary.Machine.Machines
{
    public class LaserMachineAxesConfiguration
    {
        public string LineComment => "Line coefficient for coordinates equipped with encoder. Can be negative. Set 0 if no encoder";
        public double XLine
        {
            get; set;
        }
        public double YLine
        {
            get; set;
        }
        public double ZLine
        {
            get; set;
        }
        public string RightDirectionComment => "Set true if moving right is the positive direction";
        public bool XRightDirection
        {
            get; set;
        }
        public bool YRightDirection
        {
            get; set;
        }
        public bool ZRightDirection
        {
            get; set;
        }
        public string ZToObjApproxComment => "Set true if z move toward the objective in the positive direction";
        public bool ZToObjectiveApproximation
        {
            get; set;
        }

    }
}
