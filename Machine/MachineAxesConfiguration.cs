namespace MachineClassLibrary.Machine
{
    public class MachineAxesConfiguration
    {
        public string LineComment { get => "Line coefficient for coordinates equiped with encoder. Can be negative. Set 0 if no encoder"; }
        public double XLine { get; set; }
        public double YLine { get; set; }
        public double ZLine { get; set; }
        public double ULine { get; set; }
        public string RightDirectionComment { get => "Set true if moving right is the positive direction"; }
        public bool XRightDirection { get; set; }
        public bool YRightDirection { get; set; }
        public bool ZRightDirection { get; set; }
        public bool URightDirection { get; set; }
        public string ZToObjAproxComment { get => "Set true if z move toward the objective in the positive direction"; }
        public bool ZToObjectiveAproximation { get; set; }

    }
}
