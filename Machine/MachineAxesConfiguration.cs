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



        public string HomeModeComment =>
            " [0：MODE1_Abs]" +
            " [1：MODE2_Lmt]" +
            " [2：MODE3_Ref]" +
            " [3：MODE4_Abs_Ref]" +
            " [4：MODE5_Abs_NegRef]" +
            " [5：MODE6_Lmt_Ref]" +
            " [6：MODE7_AbsSearch]" +
            " [7：MODE8_LmtSearch]" +
            " [8：MODE9_AbsSearch_Ref]" +
            " [9：MODE10_AbsSearch_NegRef]" +
            " [10：MODE11_LmtSearch_Ref]" +
            " [11：MODE12_AbsSearchReFind]" +
            " [12：MODE13_LmtSearchReFind]" +
            " [13：MODE14_AbsSearchReFind_Ref]" +
            " [14：MODE15_AbsSearchReFind_NegRef]" +
            " [15：MODE16_LmtSearchReFind_Re]";
        public string HomeResetComment =>
            " HOME_RESET_DIS = 0," +
            " HOME_RESET_EN = 1," +
            " NOT_SUPPORT = 65535";

        public string HomeDirectionComment =>
            " Pos = 0," +
            " Neg = 1";

       
        public int XHomeMode { get; set; }
        public int XHomeReset { get; set; }
        public int XHomeDirection { get; set;}
        public int YHomeMode { get; set; }
        public int YHomeReset { get; set; }
        public int YHomeDirection { get; set; }
        public int ZHomeMode { get; set; }
        public int ZHomeReset { get; set; }
        public int ZHomeDirection { get; set; }
        public string ZToObjApproxComment => "Set true if z move toward the objective in the positive direction";
        public bool ZToObjectiveApproximation { get; set; }

        public bool XMirrorCamera { get; set; }
        public bool YMirrorCamera { get; set; }


    }
}
