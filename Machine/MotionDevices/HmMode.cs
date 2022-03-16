using Advantech.Motion;

namespace MachineClassLibrary.Machine.MotionDevices
{
    public enum HmMode
    {
        MODE1_Abs = (int)HomeMode.MODE1_Abs,
        MODE2_Lmt = (int)HomeMode.MODE2_Lmt,
        MODE3_Ref = (int)HomeMode.MODE3_Ref,
        MODE4_Abs_Ref = (int)HomeMode.MODE4_Abs_Ref,
        MODE5_Abs_NegRef = (int)HomeMode.MODE5_Abs_NegRef,
        MODE6_Lmt_Ref = (int)HomeMode.MODE6_Lmt_Ref,
        MODE7_AbsSearch = (int)HomeMode.MODE7_AbsSearch,
        MODE8_LmtSearch = (int)HomeMode.MODE8_LmtSearch,
        MODE9_AbsSearch_Ref = (int)HomeMode.MODE9_AbsSearch_Ref,
        MODE10_AbsSearch_NegRef = (int)HomeMode.MODE10_AbsSearch_NegRef,
        MODE11_LmtSearch_Ref = (int)HomeMode.MODE11_LmtSearch_Ref,
        MODE12_AbsSearchReFind = (int)HomeMode.MODE12_AbsSearchReFind,
        MODE13_LmtSearchReFind = (int)HomeMode.MODE13_LmtSearchReFind,
        MODE14_AbsSearchReFind_Ref = (int)HomeMode.MODE14_AbsSearchReFind_Ref,
        MODE15_AbsSearchReFind_NegRef = (int)HomeMode.MODE15_AbsSearchReFind_NegRef,
        MODE16_LmtSearchReFind_Ref = (int)HomeMode.MODE16_LmtSearchReFind_Ref
    }
}
