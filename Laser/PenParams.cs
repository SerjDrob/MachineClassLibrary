namespace MachineClassLibrary.Laser
{
    public record PenParams
    (
        int PenNo,    // Pen’s NO. (0-255)
        int MarkLoop,   //mark times
        double MarkSpeed,   //speed of marking mm/s
        double PowerRatio, // power ratio of laser (0-100%)	
        double Current,    //current of laser (A)
        int Freq,  // frequency of laser HZ
        int QPulseWidth,    //width of Q pulse (us)	
        int StartTC,   // Start delay (us)
        int LaserOnTC,
        int LaserOffTC,        //delay before laser off (us)
        int EndTC,     // marking end delay (us)
        int PolyTC,        //delay for corner (us)
        double JumpSpeed,  //speed of jump without laser (mm/s)
        int JumpPosTC,     //delay about jump position (us)
        int JumpDistTC,    //delay about the jump distance (us)	
        double EndComp,        //compensate for end (mm)
        double AccDist,    // distance of speed up (mm)	
        double PointTime,  //delay for point mark (ms) 
        bool PulsePointMode,   //pulse for point mark mode
        int PulseNum,  //the number of pulse
        double FlySpeed    //speed of production line
    );
}
