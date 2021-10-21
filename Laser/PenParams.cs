namespace MachineClassLibrary.Laser
{
    public record PenParams
    (
        int nPenNo,    // Pen’s NO. (0-255)
        int nMarkLoop,   //mark times
        double dMarkSpeed,   //speed of marking mm/s
        double dPowerRatio, // power ratio of laser (0-100%)	
        double dCurrent,    //current of laser (A)
        int nFreq,  // frequency of laser HZ
        int nQPulseWidth,    //width of Q pulse (us)	
        int nStartTC,   // Start delay (us)
        int nLaserOnTC,
        int nLaserOffTC,        //delay before laser off (us)
        int nEndTC,     // marking end delay (us)
        int nPolyTC,        //delay for corner (us)
        double dJumpSpeed,  //speed of jump without laser (mm/s)
        int nJumpPosTC,     //delay about jump position (us)
        int nJumpDistTC,    //delay about the jump distance (us)	
        double dEndComp,        //compensate for end (mm)
        double dAccDist,    // distance of speed up (mm)	
        double dPointTime,  //delay for point mark (ms) 
        bool bPulsePointMode,   //pulse for point mark mode
        int nPulseNum,  //the number of pulse
        double dFlySpeed    //speed of production line
    );
}
