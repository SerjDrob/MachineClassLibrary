namespace MachineClassLibrary.Laser.Parameters
{
    public record HatchParams
    (
         bool EnableContour, //enable the contour of object to be marked
         int ParamIndex, //hatch order number is 1,2,3
         bool EnableHatch, //enable hatch
         int PenNo, //hatch pen no
         int HatchType, // Hatch type:0 unidirectional, 1 bidirectional, 2 return, 3 bow, 4 bow not reverse
         bool HatchAllCalc, // compute all object or not
         bool HatchEdge, //around edge once time
         bool HatchAverageLine,// Automatic average distribution line double dHatchAngle, //hatch line angle
         double HatchLineDist, // hatch edge distance
         double HatchEdgeDist, // hatch line distance 
         double HatchStartOffset, // hatch start offset distance
         double HatchEndOffset, // hatch end offset distance
         double HatchLineReduction,//line reduction
         double HatchLoopDist, //ring line distance
         int EdgeLoop, //ring count
         bool HatchLoopRev, //loop reverse
         bool HatchAutoRotate, //enable auto rotate angle or not
         double HatchRotateAngle,
         int HatchAttribute,
         bool HatchContourFirst
    );
}
