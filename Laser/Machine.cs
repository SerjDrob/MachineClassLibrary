using Advantech.Motion;
using AForge;
using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;
using CommonMethods;
using netDxf;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;



namespace MachineClassLibrary.Laser
{
    delegate void GetNumOfCycle(int n);

    partial class Machine
    {
        public GetNumOfCycle GetNum;
        public Machine(string configfilepath)
        {

            Configs = new Features().ReadFeatures(configfilepath);
            DevicesConnection();

        }
        public bool laserInit { get; set; } = false;
        public Features Configs = new Features();

        public bool MachineInit { get; set; } = false;

        Thread DevEvents;
        Dispatcher dispatcher;


        #region Методы

        private bool DevicesConnection()
        {
            uint Result;
            string strTemp;
            int ResAvlb;
            uint i = 0;
            uint[] slaveDevs = new uint[16];
            uint AxesPerDev = new uint();
            uint deviceCount = 0;
            uint DeviceNum = 0;
            DEV_LIST[] CurAvailableDevs = new DEV_LIST[Motion.MAX_DEVICES];


            ResAvlb = Motion.mAcm_GetAvailableDevs(CurAvailableDevs, Motion.MAX_DEVICES, ref deviceCount);

            if (ResAvlb != (int)ErrorCode.SUCCESS)
            {
                strTemp = "Get Device Numbers Failed With Error Code: [0x" + Convert.ToString(ResAvlb, 16) + "]";
                MessageBox.Show(strTemp + " " + ResAvlb);
                return false;
            }

            if (deviceCount > 0)
            {
                DeviceNum = CurAvailableDevs[0].DeviceNum;
            }

            DeviceNum = CurAvailableDevs[0].DeviceNum;

            Result = Motion.mAcm_DevOpen(DeviceNum, ref m_DeviceHandle);
            if (Result != (uint)ErrorCode.SUCCESS)
            {
                strTemp = "Open Device Failed With Error Code: [0x" + Convert.ToString(Result, 16) + "]";
                MessageBox.Show(strTemp + Result);
                return false;
            }

            Result = Motion.mAcm_GetU32Property(m_DeviceHandle, (uint)PropertyID.FT_DevAxesCount, ref AxesPerDev);
            if (Result != (uint)ErrorCode.SUCCESS)
            {
                strTemp = "Get Axis Number Failed With Error Code: [0x" + Convert.ToString(Result, 16) + "]";
                MessageBox.Show(strTemp + " " + Result);
                return false;
            }

            m_ulAxisCount = AxesPerDev;

            for (i = 0; i < m_ulAxisCount; i++)
            {

                Result = Motion.mAcm_AxOpen(m_DeviceHandle, (ushort)i, ref m_Axishand[i]);
                if (Result != (uint)ErrorCode.SUCCESS)
                {
                    strTemp = "Open Axis Failed With Error Code: [0x" + Convert.ToString(Result, 16) + "]";
                    MessageBox.Show(strTemp + " " + Result);
                    return false;
                }

                double cmdPosition = new double();
                cmdPosition = 0;
                //Set command position for the specified axis
                Motion.mAcm_AxSetCmdPosition(m_Axishand[i], cmdPosition);
                //Set actual position for the specified axis
                Motion.mAcm_AxSetActualPosition(m_Axishand[i], cmdPosition);
            }
            m_bInit = true;


            if (MessageBox.Show("Подключить лазерный источник?", "Подключение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                IntPtr Handle = new WindowInteropHelper(new Window()).Handle;
                int c = Lmc.lmc1_Initial(Directory.GetCurrentDirectory(), 0, Handle);  //初始化激光雕刻机

                if (c != 0)
                {
                    MessageBox.Show("初始化Lmc失败\r\n错误代码：" + c + "\r\n错误原因:"/* + EzCad_Error_Code.ErrorMessage[c]*/);
                    Lmc.lmc1_Close();
                }
                else laserInit = true;
            }
            else laserInit = false;

            if (m_bInit & laserInit) MachineInit = true;
            return true;
        }
        private void SetLaserParams(Parameter SovmSetParams, Parameter OtvSetParams)
        {

            //--------------SOVM----------------------

            int nMarkLoop = SovmSetParams.nMarkLoop;	 //mark times
            double dMarkSpeed = SovmSetParams.dMarkSpeed;	 //speed of marking mm/s
            double dPowerRatio = SovmSetParams.dPowerRatio;	// power ratio of laser (0-100%)	
            double dCurrent = 0;	//current of laser (A)
            int nFreq = 50000;	// frequency of laser HZ
            int nQPulseWidth = 0;	 //width of Q pulse (us)	
            int nStartTC = 0;   // Start delay (us)
            int nLaserOnTC = 0;
            int nLaserOffTC = 100;		//delay before laser off (us)
            int nEndTC = 300;		// marking end delay (us)
            int nPolyTC = 100;		//delay for corner (us)
            double dJumpSpeed = 4000; 	//speed of jump without laser (mm/s)
            int nJumpPosTC = 500;		//delay about jump position (us)
            int nJumpDistTC = 100;	//delay about the jump distance (us)	
            double dEndComp = 0;		//compensate for end (mm)
            double dAccDist = 0;	// distance of speed up (mm)	
            double dPointTime = 0.1;	//delay for point mark (ms) 
            bool bPulsePointMode = false;	//pulse for point mark mode
            int nPulseNum = 1;	//the number of pulse
            double dFlySpeed = 1;

            Lmc.lmc1_SetPenParam(0,
                                                   nMarkLoop,	 //mark times
                                                   dMarkSpeed,	 //speed of marking mm/s
                                                   dPowerRatio,	// power ratio of laser (0-100%)	
                                                   dCurrent,	//current of laser (A)
                                                   nFreq,	// frequency of laser HZ
                                                   nQPulseWidth,	 //width of Q pulse (us)	
                                                   nStartTC,	// Start delay (us)
                                                   nLaserOnTC,
                                                   nLaserOffTC,		//delay before laser off (us)
                                                   nEndTC,		// marking end delay (us)
                                                   nPolyTC,		//delay for corner (us)
                                                   dJumpSpeed, 	//speed of jump without laser (mm/s)
                                                   nJumpPosTC,		//delay about jump position (us)
                                                   nJumpDistTC,	//delay about the jump distance (us)	
                                                   dEndComp,		//compensate for end (mm)
                                                   dAccDist,	// distance of speed up (mm)	
                                                   dPointTime,	//delay for point mark (ms) 
                                                   bPulsePointMode,	//pulse for point mark mode
                                                   nPulseNum,	//the number of pulse
                                                   dFlySpeed);

            Lmc.lmc1_SetHatchParam(true, 1, 0, 0x10, 0.01, 0.01, 0, 0, 0, 0, 0, 0, 0.01, 0.01, 0, 0, 0);



            // lmc1_AddCircleToLib(0, 0, techData.dSOVMin/(2 * dxfScale), "SOVM", 0);
            //lmc1_HatchEnt("SOVM", "SOVM");


            //-------------------------OTV--------------------------------------

            nMarkLoop = OtvSetParams.nMarkLoop;	 //mark times
            dMarkSpeed = OtvSetParams.dMarkSpeed;	 //speed of marking mm/s
            dPowerRatio = OtvSetParams.dPowerRatio; // power ratio of laser (0-100%)	


            Lmc.lmc1_SetPenParam(1, nMarkLoop,	 //mark times
                                                   dMarkSpeed,	 //speed of marking mm/s
                                                   dPowerRatio,	// power ratio of laser (0-100%)	
                                                   dCurrent,	//current of laser (A)
                                                   nFreq,	// frequency of laser HZ
                                                   nQPulseWidth,	 //width of Q pulse (us)	
                                                   nStartTC,	// Start delay (us)
                                                   nLaserOnTC,
                                                   nLaserOffTC,		//delay before laser off (us)
                                                   nEndTC,		// marking end delay (us)
                                                   nPolyTC,		//delay for corner (us)
                                                   dJumpSpeed, 	//speed of jump without laser (mm/s)
                                                   nJumpPosTC,		//delay about jump position (us)
                                                   nJumpDistTC,	//delay about the jump distance (us)	
                                                   dEndComp,		//compensate for end (mm)
                                                   dAccDist,	// distance of speed up (mm)	
                                                   dPointTime,	//delay for point mark (ms) 
                                                   bPulsePointMode,	//pulse for point mark mode
                                                   nPulseNum,	//the number of pulse
                                                   dFlySpeed);

            Lmc.lmc1_SetHatchParam(true, 1, 1, 0x10, 0.025, 0.01, 0, 0, 0, 0, 0, 0, 0.01, 0.01, 0, 0, 0);

        }

        /// <summary>
        /// Устанавливает параметры прошивки
        /// </summary>
        /// <param name="parameter">Параметры прошивки</param>
        public void SetLaserParams(Parameter parameter)
        {
            double dCurrent = 0;    //current of laser (A)
            int nFreq = 50000;  // frequency of laser HZ
            int nQPulseWidth = 0;    //width of Q pulse (us)	
            int nStartTC = 0;   // Start delay (us)
            int nLaserOnTC = 0;
            int nLaserOffTC = 100;      //delay before laser off (us)
            int nEndTC = 300;       // marking end delay (us)
            int nPolyTC = 100;      //delay for corner (us)
            double dJumpSpeed = 4000;   //speed of jump without laser (mm/s)
            int nJumpPosTC = 500;       //delay about jump position (us)
            int nJumpDistTC = 100;  //delay about the jump distance (us)	
            double dEndComp = 0;        //compensate for end (mm)
            double dAccDist = 0;    // distance of speed up (mm)	
            double dPointTime = 0.1;    //delay for point mark (ms) 
            bool bPulsePointMode = false;   //pulse for point mark mode
            int nPulseNum = 1;  //the number of pulse
            double dFlySpeed = 1;

            Lmc.lmc1_SetPenParam(0, parameter.nMarkLoop,   //mark times
                                    parameter.dMarkSpeed,   //speed of marking mm/s
                                    parameter.dPowerRatio, // power ratio of laser (0-100%)	
                                    dCurrent,    //current of laser (A)
                                    nFreq,   // frequency of laser HZ
                                    nQPulseWidth,     //width of Q pulse (us)	
                                    nStartTC,    // Start delay (us)
                                    nLaserOnTC,
                                    nLaserOffTC,     //delay before laser off (us)
                                    nEndTC,      // marking end delay (us)
                                    nPolyTC,     //delay for corner (us)
                                    dJumpSpeed,  //speed of jump without laser (mm/s)
                                    nJumpPosTC,      //delay about jump position (us)
                                    nJumpDistTC, //delay about the jump distance (us)	
                                    dEndComp,        //compensate for end (mm)
                                    dAccDist,    // distance of speed up (mm)	
                                    dPointTime,  //delay for point mark (ms) 
                                    bPulsePointMode, //pulse for point mark mode
                                    nPulseNum,   //the number of pulse
                                    dFlySpeed);

            Lmc.lmc1_SetHatchParam(true, 1, 0, 0x10, 0.025, 0.01, 0, 0, 0, 0, 0, 0, 0.01, 0.01, 0, 0, 0);

        }
        public async Task ScanLine()
        {
            lineXCoefficients = new List<(double, double)>();
            ushort state = new ushort();
            double actPosition = new double();
            double cmdPosition = new double();
            double initCmdPosition = new double();
            double tempCmdPosition = 0;
            double tempActPosition = 0;
            SetVelocity("Low");
            Motion.mAcm_AxGetCmdPosition(m_Axishand[0], ref initCmdPosition);
            Motion.mAcm_AxSetActualPosition(m_Axishand[0], 0);

            await Task.Run(() =>
            {
                Motion.mAcm_AxMoveVel(m_Axishand[0], 1);
                Motion.mAcm_AxGetState(m_Axishand[0], ref state);
                while (state == (ushort)AxisState.STA_AX_CONTI_MOT)
                {
                    Motion.mAcm_AxGetCmdPosition(m_Axishand[0], ref cmdPosition);
                    if (Math.Abs(cmdPosition - tempCmdPosition) >= 1)
                    {
                        Motion.mAcm_AxGetActualPosition(m_Axishand[0], ref actPosition);
                        lineXCoefficients.Add((cmdPosition, (cmdPosition - initCmdPosition) / actPosition));
                        tempCmdPosition = cmdPosition;
                        tempActPosition = actPosition;
                    }
                    Motion.mAcm_AxGetState(m_Axishand[0], ref state);
                }
                Methods.SavePairs(lineXCoefficients, "/pairs.txt");
                MessageBox.Show(lineXCoefficients.Average(a => a.Item2).ToString());
            });
        }
        public async Task PierceObject(ProcObject procObject, Parameter parameter, HatchParams hatchParams)
        {
            var layerName = "MyEntity";
            var pazFilePath = "/piercePaz.dxf";


            if (procObject is netDxf.Entities.Circle)
            {
                var circle = procObject.Object as netDxf.Entities.Circle;
                var innerR = circle.Radius - hatchParams.ContourWidth;
                var outerR = circle.Radius + parameter.Taper / 2;

                if (innerR <= 0)
                {
                    Lmc.lmc1_AddCircleToLib(0, 0, outerR, layerName, 0);
                }
                else
                {
                    Lmc.lmc1_AddCircleToLib(0, 0, outerR, layerName, 0);
                    Lmc.lmc1_AddCircleToLib(0, 0, innerR, layerName + "1", 0);
                    Lmc.lmc1_GroupEnt(layerName, layerName + "1", layerName, 0);
                }
            }
            else if (procObject is netDxf.Entities.LwPolyline)
            {
                var dxf = new DxfDocument();
                var polyline = procObject.Object as netDxf.Entities.LwPolyline;
                dxf.AddEntity(polyline);
                dxf.Save(pazFilePath);
                Lmc.lmc1_AddFileToLib(pazFilePath, layerName, 0, 0, 0, 0, 1, 0, false);
                Lmc.lmc1_SaveEntLibToFile(pazFilePath);
                dxf.RemoveEntity(polyline);
            }

            Lmc.lmc1_SetHatchEntParam(layerName,
                                      bEnableContour: hatchParams.EnableContour, //enable the contour of object to be marked
                                      nParamIndex: hatchParams.ParamIndex, //hatch order number is 1,2,3
                                      bEnableHatch: hatchParams.EnableHatch, //enable hatch
                                      nPenNo: hatchParams.PenNo, //hatch pen no
                                      nHatchType: hatchParams.HatchType, // Hatch type:0 unidirectional, 1 bidirectional, 2 return, 3 bow, 4 bow not reverse
                                      bHatchAllCalc: hatchParams.HatchAllCalc, // compute all object or not
                                      bHatchEdge: hatchParams.HatchEdge, //around edge once time
                                      bHatchAverageLine: hatchParams.HatchAverageLine,// Automatic average distribution line double dHatchAngle, //hatch line angle
                                      dHatchLineDist: hatchParams.HatchLineDist, // hatch edge distance
                                      dHatchEdgeDist: hatchParams.HatchEdgeDist, // hatch line distance 
                                      dHatchStartOffset: hatchParams.HatchStartOffset, // hatch start offset distance
                                      dHatchEndOffset: hatchParams.HatchEndOffset, // hatch end offset distance
                                      dHatchLineReduction: hatchParams.HatchLineReduction,//line reduction
                                      dHatchLoopDist: hatchParams.HatchLoopDist, //ring line distance
                                      nEdgeLoop: hatchParams.EdgeLoop, //ring count
                                      nHatchLoopRev: hatchParams.HatchLoopRev, //loop reverse
                                      bHatchAutoRotate: hatchParams.HatchAutoRotate, //enable auto rotate angle or not
                                      dHatchRotateAngle: hatchParams.HatchRotateAngle //enable rotate angle
                                      );
            SetLaserParams((Parameter)parameter.Clone());

            await Task.Run(() =>
            {
                for (int i = 0; i < parameter.SeriesCount; i++)
                {
                    Lmc.lmc1_SetEntAllChildPen(layerName, 0);
                    Lmc.lmc1_MarkEntity(layerName);
                    GetNum(i + 1);
                    Task.Delay(parameter.SeriesPause).RunSynchronously();
                }
            });

            Lmc.lmc1_DeleteEnt(layerName);
        }

        public async Task Pierce(object obj, Parameter parameter)
        {
            Parameter par = (Parameter)parameter.Clone();

            SetLaserParams(par);
            if (obj.GetType() == typeof(string))
            {
                string str = (string)obj;
                switch (str)
                {
                    case "point":
                        {
                            Lmc.lmc1_MarkPoint(0, 0, 100, 0);
                            break;
                        }
                    case "line":
                        {
                            Lmc.lmc1_SetRotateParam(0, 0, 0);
                            Lmc.lmc1_MarkLine(-30, 0, 30, 0, 0);
                            break;
                        }
                }
            }

            if (obj.GetType() == typeof(ProcObject))
            {
                double dop = parameter.Taper;
                string layer = "OTV";
                ProcObject procObject = (ProcObject)obj;
                if (procObject.Object.GetType() == typeof(netDxf.Entities.Circle))
                {
                    netDxf.Entities.Circle circle = (netDxf.Entities.Circle)procObject.Object;
                    if (circle.Radius > 0.75)
                    {
                        Lmc.lmc1_AddCircleToLib(0, 0, circle.Radius + dop / 2, layer, 0);
                        Lmc.lmc1_AddCircleToLib(0, 0, circle.Radius - 0.1 + dop / 2, layer + "1", 0);
                        Lmc.lmc1_GroupEnt(layer, layer + "1", layer, 0);
                    }
                    else Lmc.lmc1_AddCircleToLib(0, 0, circle.Radius + (dop + 0.08) / 2, layer, 0);
                    Lmc.lmc1_HatchEnt(layer, layer);
                    await Task.Run(() =>
                    {
                        for (int i = 0; i < parameter.SeriesCount; i++)
                        {
                            Lmc.lmc1_SetEntAllChildPen(layer, 0);
                            Lmc.lmc1_MarkEntity(layer);
                            GetNum(i + 1);
                            Thread.Sleep(parameter.SeriesPause);
                        }
                    });
                    Lmc.lmc1_DeleteEnt(layer);
                }
                if (procObject.Object.GetType() == typeof(netDxf.Entities.LwPolyline))
                {
                    netDxf.Entities.LwPolyline polyline = (netDxf.Entities.LwPolyline)procObject.Object;
                    DxfDocument dxf = new DxfDocument();
                    dxf.AddEntity(polyline);
                    dxf.Save("/piercePaz.dxf");
                    Lmc.lmc1_AddFileToLib("/piercePaz.dxf", "paz", 0, 0, 0, 0, 1, 0, false);
                    Lmc.lmc1_SaveEntLibToFile("/piercePaz.ezd");

                    await Task.Run(() =>
                    {
                        for (int i = 0; i < parameter.SeriesCount; i++)
                        {
                            Lmc.lmc1_SetEntAllChildPen("paz", 0);
                            Lmc.lmc1_MarkEntity("paz");
                            Thread.Sleep(parameter.SeriesPause);
                            par.dMarkSpeed -= 2;
                            SetLaserParams(par);
                        }
                    });
                    dxf.RemoveEntity(polyline);
                    Lmc.lmc1_DeleteEnt("paz");
                }
            }
        }
        #endregion
        public void MarkName(string name, double angle, double textAngle, double textSize)
        {
            int err = Lmc.lmc1_SetFontParam("Cambria", textSize, 0.625 * textSize, textAngle, 0, 0, false);
            Lmc.lmc1_AddTextToLib(name, "text", 0, 0, 0, 8, angle, 0, true);
            //Lmc.lmc1_SetTextEntParam("text", 1, 1, 0, 0, 0, true);
            Lmc.lmc1_MarkEntity("text");
            Lmc.lmc1_DeleteEnt("text");
            //Lmc.lmc1_SaveEntLibToFile("/testEZ.ezd");

        }

    }
    public record DeviceParams
    {

    }
}
