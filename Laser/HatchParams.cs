namespace MachineClassLibrary.Laser
{
    public record HatchParams
    (         
         bool bEnableContour, //enable the contour of object to be marked
         int nParamIndex, //hatch order number is 1,2,3
         int bEnableHatch, //enable hatch
         int nPenNo, //hatch pen no
         int nHatchType, // Hatch type:0 unidirectional, 1 bidirectional, 2 return, 3 bow, 4 bow not reverse
         bool bHatchAllCalc, // compute all object or not
         bool bHatchEdge, //around edge once time
         bool bHatchAverageLine,// Automatic average distribution line double dHatchAngle, //hatch line angle
         double dHatchLineDist, // hatch edge distance
         double dHatchEdgeDist, // hatch line distance 
         double dHatchStartOffset, // hatch start offset distance
         double dHatchEndOffset, // hatch end offset distance
         double dHatchLineReduction,//line reduction
         double dHatchLoopDist, //ring line distance
         int nEdgeLoop, //ring count
         bool nHatchLoopRev, //loop reverse
         bool bHatchAutoRotate, //enable auto rotate angle or not
         double dHatchRotateAngle                                                
    );
}
