using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices; //For Marshal
using System.Xml.Serialization;

namespace MachineClassLibrary.Laser
{
    /*internal*/public  class Lmc
    {
        public enum EzCad_Error_Code
        {
            LMC1_ERR_SUCCESS = 0, // Success
            LMC1_ERR_EZCADRUN, //1 // Find EZCAD running
            LMC1_ERR_NOFINDCFGFILE, //2 // Can not find EZCAD.CFG
            LMC1_ERR_FAILEDOPEN, //3 // Open LMC1 board failed
            LMC1_ERR_NODEVICE, //4 // Can not find valid lmc1 device
            LMC1_ERR_HARDVER, //5 // Lmc1’s version is error.
            LMC1_ERR_DEVCFG, //6 // Can not find configuration files
            LMC1_ERR_STOPSIGNAL, //7 // Alarm signal
            LMC1_ERR_USERSTOP, //8 // User stops
            LMC1_ERR_UNKNOW, //9 // Unknown error
            LMC1_ERR_OUTTIME, //10 // Overtime 
            LMC1_ERR_NOINITIAL, //11 // Un-initialized
            LMC1_ERR_READFILE, //12 // Read file error
            LMC1_ERR_OWENWNDNULL, //13 // Window handle is NULL
            LMC1_ERR_NOFINDFONT, //14 // Can not find designated font 
            LMC1_ERR_PENNO, //15 // Wrong pen number 
            LMC1_ERR_NOTTEXT, //16 // Object is not text 
            LMC1_ERR_SAVEFILE, //17 // Save file failed 
            LMC1_ERR_NOFINDENT, //18 // Can not find designated object
            LMC1_ERR_STATUE //19 // Can not run the operation in 
        }
        #region Device functions
        [DllImport(@"MarkEzd.dll", EntryPoint = "lmc1_Close", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_Close();
        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_Initial", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_Initial(string strEzCadPath, int bTestMode, IntPtr hOwenWnd);




        #endregion

        #region File functions
        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_LoadEzdFile", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_LoadEzdFile(string strFileName);
        #endregion

        #region External Axis
        [DllImport(@"MarkEzd.dll", EntryPoint = "lmc1_AxisCorrectOrigin", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_AxisCorrectOrigin(int axis);
        #endregion

        #region Text functions
        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_ChangeTextByName", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_ChangeTextByName(string strTextName, string strTextNew);
        #endregion

        #region Add or delete object
        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_DeleteEnt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_DeleteEnt(string strEntName);
        #endregion

        #region Marking functions
        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_Mark", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_Mark(bool bFlyMark);

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_RedLightMark", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int lmc1_RedLightMark(); //显示一次红光对标
        #endregion

        #region Hatch
        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_SetHatchParam", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_SetHatchParam(bool bEnableContour,    //enable the contour of object to be marked
                                                            int bEnableHatch1, //enable hatch NO. 1
                                                            int nPenNo1, //set the pen of hatch NO. 1
                                                            int nHatchAttrib1, //set the attribute of hatch NO. 1
                                                            double dHatchEdgeDist1, //set the distance between hatch line and contour of hatch NO. 1
                                                            double dHatchLineDist1, //set the distance between two line of hatch NO. 1 .
                                                            double dHatchStartOffset1, //set the start offset of hatch NO. 1
                                                            double dHatchEndOffset1, //set the end offset of hatch NO. 1
                                                            double dHatchAngle1, //set the hatch angle of hatch NO. 1
                                                            int bEnableHatch2, //enable hatch1 NO.2
                                                            int nPenNo2,
                                                            int nHatchAttrib2,
                                                            double dHatchEdgeDist2,
                                                            double dHatchLineDist2,
                                                            double dHatchStartOffset2,
                                                            double dHatchEndOffset2,
                                                            double dHatchAngle2);

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_HatchEnt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_HatchEnt(string pEntName, string bHatch);

        //        bEnableContour // enable contour or not
        // bEnableHatch1 //enable hatch
        // nPenNo1 //hatch pen no
        //nHatchAttrib1: //attribute of hatch，which is a combination of 
        //the following values:
        public const int HATCHATTRIB_ALLCALC = 0x01; //compute all object as one
        public const int HATCHATTRIB_BIDIR = 0x08; // reciprocating hatch
        public const int HATCHATTRIB_EDGE = 0x02; // re-mark the edge
        public const int HATCHATTRIB_LOOP = 0x10; // ring-like hatch 
        public const int HATCHATTRIB_SNAKE_LINES = 0x28; 

        //        dHatchEdgeDist1 // hatch edge distance
        //         dHatchLineDist1 //hatch line distance
        //dHatchStartOffset1 //hatch start offset distance
        //dHatchEndOffset1 //hatch end offset distance
        //dHatchAngle1 //angle of hatch line

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_SetHatchEntParam", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_SetHatchEntParam(string pHatchName, //name of hatch object
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
                                                        double dHatchRotateAngle //enable rotate angle
                                                                                  );

        public static int HatchObject(string objectName, HatchParams hatchParams) => lmc1_SetHatchEntParam(
                                               objectName,
                                               hatchParams.bEnableContour,
                                               hatchParams.nParamIndex,
                                               hatchParams.bEnableHatch,
                                               hatchParams.nPenNo,
                                               hatchParams.nHatchType,
                                               hatchParams.bHatchAllCalc,
                                               hatchParams.bHatchEdge,
                                               hatchParams.bHatchAverageLine,
                                               hatchParams.dHatchLineDist,
                                               hatchParams.dHatchEdgeDist,
                                               hatchParams.dHatchStartOffset,
                                               hatchParams.dHatchEndOffset,
                                               hatchParams.dHatchLineReduction,
                                               hatchParams.dHatchLoopDist,
                                               hatchParams.nEdgeLoop,
                                               hatchParams.nHatchLoopRev,
                                               hatchParams.bHatchAutoRotate,
                                               hatchParams.dHatchRotateAngle);


        public static int SetHatchParams(HatchParams hatchParams) => lmc1_SetHatchParam(hatchParams.bEnableContour,
            hatchParams.bEnableHatch,
            hatchParams.nPenNo,
            HATCHATTRIB_LOOP,//40
            hatchParams.dHatchEdgeDist,
            hatchParams.dHatchLineDist,
            hatchParams.dHatchStartOffset,
            hatchParams.dHatchEndOffset,
            hatchParams.dHatchRotateAngle,
            0,
            0,
            48,
            0,
            0,
            0,
            0,
            0);

        public static int SetPenParams(PenParams penParams) => Lmc.lmc1_SetPenParam(penParams.nPenNo,
                                              penParams.nMarkLoop,
                                              penParams.dMarkSpeed,
                                              penParams.dPowerRatio,
                                              penParams.dCurrent,
                                              penParams.nFreq,
                                              penParams.nQPulseWidth,
                                              penParams.nStartTC,
                                              penParams.nLaserOnTC,
                                              penParams.nLaserOffTC,
                                              penParams.nEndTC,
                                              penParams.nPolyTC,
                                              penParams.dJumpSpeed,
                                              penParams.nJumpPosTC,
                                              penParams.nJumpDistTC,
                                              penParams.dEndComp,
                                              penParams.dAccDist,
                                              penParams.dPointTime,
                                              penParams.bPulsePointMode,
                                              penParams.nPulseNum,
                                              penParams.dFlySpeed);
        #endregion










        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_SetDevCfg", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_SetDevCfg();

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_SetDevCfg2", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_SetDevCfg2(bool axis1Show, bool axis2Show);

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_AddCurveToLib", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_AddCurveToLib(double[,] ptBuf, int ptNum, string pEntName, int nPenNo, int bHatch);

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_SaveEntLibToFile", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_SaveEntLibToFile(string strFileName);



        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_AddCircleToLib", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_AddCircleToLib(double posX, double posY, double Radius, string pEntName, int nPenNo);



        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_SetPenParam", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_SetPenParam(int nPenNo,    // Pen’s NO. (0-255)
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

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_MarkEntity", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_MarkEntity(string pEntName);

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_MarkPoint", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_MarkPoint(double x, double y, double delay, int pen);

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_MarkLine", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_MarkLine(double x1, double y1, double x2, double y2, int pen);

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_SetRotateParam", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_SetRotateParam(double dCenterX, double dCenterY, double dRotateAng);

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_ClearEntLib", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_ClearEntLib();

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_AddFileToLib", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_AddFileToLib(string pFileName, //ÎÄ¼þÃû³æ
                                                   string pEntName,//×ö·û´®¶ÔÏÓÃû³æ
                                                   double dPosX, //ÎÄ¼þ×ÓÏÂ½Ç»ùµÃx×Ø±Ê
                                                   double dPosY, //ÎÄ¼þ×ÓÏÂ½Ç»ùµÃy×Ø±Ê
                                                   double dPosZ, //ÎÄ¼þz×Ø±Ê
                                                   int nAlign,//¶ÔæË·½Ê½0£­8
                                                   double dRatio,//ÎÄ¼þËÕ·Å±èÀý  
                                                   int nPenNo,//¶ÔÏÓÊ¹ÓÃµÄ¼Ó¹¤²ÎÊý
                                                   bool bHatchFile);

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_UnGroupEnt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_UnGroupEnt(string pEntName);

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_GroupEnt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_GroupEnt(string pEntName1,
                                                string pEntName2,
                                                string pEntNameNew,
                                                int pen);

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_RedLightMarkContour", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_RedLightMarkContour();

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_RedLightMarkByEnt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_RedLightMarkByEnt(string strEntName, bool bContour);

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_SetEntAllChildPen", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_SetEntAllChildPen(string strEntName, int nPenNo);

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_AddTextToLib", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_AddTextToLib(
                                                    string pStr,
                                                    string pEntName,
                                                    double dPosX,
                                                    double dPosY,
                                                    double dPosZ,
                                                    int nAlign,
                                                    double dTextRotateAngle,
                                                    int nPenNo,
                                                    bool bHatchText    //hatch the text object or not. 
                                                    );

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_SetFontParam", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_SetFontParam(string strFontName,
                                                    double dCharHeight,
                                                    double dCharWidth,
                                                    double dCharAngle,
                                                    double dCharSpace,
                                                    double dLineSpace,
                                                    bool bEqualCharWidth);

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_MirrorEnt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_MirrorEnt(string pEntName,
                                                double dCenx,
                                                double dCeny,
                                                bool bMirrorX,
                                                bool bMirrorY);

        [DllImport("MarkEzd.dll", EntryPoint = "lmc1_RotateEnt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int lmc1_RotateEnt(string pEntName,
                                                double dCenx,
                                                double dCeny,
                                                double dAngle);
    }
}