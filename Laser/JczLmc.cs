using MachineClassLibrary.Laser.Parameters;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;


namespace MachineClassLibrary.Laser
{

    public class JczLmc
    {
        public static string GetErrorText(int nErr)
        {
            switch (nErr)
            {
                case 0: return "іЙ№¦";
                case 1: return "·ўПЦEZCADТСѕ­ФЪФЛРР";
                case 2: return "ХТІ»µЅEZCAD.CFG";
                case 3: return "ґтїЄLMCК§°Ь";
                case 4: return "Г»УРLMCЙи±ё";
                case 5: return "lmcЙи±ё°ж±ѕґнОу";
                case 6: return "ХТІ»µЅЙи±ёЕдЦГОДјюMarkCfg";
                case 7: return "УР±ЁѕЇРЕєЕ";
                case 8: return "УГ»§ЦР¶П";
                case 9: return "І»ГчґнОу";
                case 10: return "і¬К±";
                case 11: return "ОґіхКј»Ї";
                case 12: return "¶БОДјюК§°Ь";
                case 13: return "ґ°їЪОЄїХ";
                case 14: return "ХТІ»µЅЧЦМе";
                case 15: return "±КєЕґнОу";
                case 16: return "Цё¶Ё¶ФПуІ»КЗОД±ѕ¶ФПу";
                case 17: return "±ЈґжОДјюК§°Ь";
                case 18: return "±ЈґжОДјюК§°ЬХТІ»µЅЦё¶Ё¶ФПу";
                case 19: return "µ±З°ЧґМ¬І»ДЬЦґРРґЛІЩЧч";
                case 20: return "ІОКэКдИлґнОу";
                default:
                    break;
            }
            return "ІОКэКдИлґнОу";
        }
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
        public static string GetErrorText(int nErr, bool English)
        {
            if (English)
            {
                switch (nErr)
                {
                    case 0: return "Success";
                    case 1: return "Now have a working EACAD";
                    case 2: return "No found EZCAD.CFG";
                    case 3: return "Open LMC faild";
                    case 4: return "No LMC Board";
                    case 5: return "Lmc vision Error";
                    case 6: return " No found MarkCfg in Plug ";
                    case 7: return "Error Signal";
                    case 8: return "User Stop";
                    case 9: return "unknown error";
                    case 10: return "out time";
                    case 11: return "No Initialization";
                    case 12: return "Read File Error";
                    case 13: return "full Windows";
                    case 14: return "No found font";
                    case 15: return "Pen error";
                    case 16: return "object is not text";
                    case 17: return "save file fail";
                    case 18: return "save file fail because same object is no found";
                    case 19: return "Now state can not work as command";
                    case 20: return "error Parameter";
                    default:
                        break;
                }
                return "error Parameter";
            }
            else
            {
                switch (nErr)
                {
                    case 0: return "іЙ№¦";
                    case 1: return "·ўПЦEZCADТСѕ­ФЪФЛРР";
                    case 2: return "ХТІ»µЅEZCAD.CFG";
                    case 3: return "ґтїЄLMCК§°Ь";
                    case 4: return "Г»УРLMCЙи±ё";
                    case 5: return "lmcЙи±ё°ж±ѕґнОу";
                    case 6: return "ХТІ»µЅЙи±ёЕдЦГОДјюMarkCfg";
                    case 7: return "УР±ЁѕЇРЕєЕ";
                    case 8: return "УГ»§ЦР¶П";
                    case 9: return "І»ГчґнОу";
                    case 10: return "і¬К±";
                    case 11: return "ОґіхКј»Ї";
                    case 12: return "¶БОДјюК§°Ь";
                    case 13: return "ґ°їЪОЄїХ";
                    case 14: return "ХТІ»µЅЧЦМе";
                    case 15: return "±КєЕґнОу";
                    case 16: return "Цё¶Ё¶ФПуІ»КЗОД±ѕ¶ФПу";
                    case 17: return "±ЈґжОДјюК§°Ь";
                    case 18: return "±ЈґжОДјюК§°ЬХТІ»µЅЦё¶Ё¶ФПу";
                    case 19: return "µ±З°ЧґМ¬І»ДЬЦґРРґЛІЩЧч";
                    case 20: return "ІОКэКдИлґнОу";
                    default:
                        break;
                }
                return "ІОКэКдИлґнОу";
            }
        }

        #region Йи±ё

        /// <summary>
        /// іхКј»ЇєЇКэїв
        /// </summary>
        /// <param name="ezcad2.exeЛщґ¦µДДїВјµДИ«В·ѕ¶ГыіЖ"></param>
        /// <param name="ЦёКЗ·сКЗІвКФДЈКЅ,(ІвКФДЈКЅїШЦЖїЁПа№ШєЇКэОЮ·Ё№¤Чч)"></param>
        /// <param name="ЦёУµУРУГ»§КдИлЅ№µгµДґ°їЪЈ¬УГУЪјмІвУГ»§ФЭНЈПыПўЎЈ"></param>
        /// <returns></returns>
        [DllImport("MarkEzd", EntryPoint = "lmc1_Initial", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int InitializeTotal(string PathName, bool bTestMode, IntPtr MailForm);

        /// <summary>
        /// іхКј»ЇєЇКэїв
        /// PathName КЗMarkEzd.dllЛщФЪµДДїВј
        /// </summary>     
        [DllImport("MarkEzd", EntryPoint = "lmc1_Initial2", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int Initialize(string PathName, bool bTestMode);

        /// <summary>
        /// КН·ЕєЇКэїв
        /// </summary>     
        [DllImport("MarkEzd", EntryPoint = "lmc1_Close", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int Close();

        /// <summary>
        /// µГµЅЙи±ёІОКэЕдЦГ¶Ф»°їт  
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_SetDevCfg", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetDevCfg();

        /// <summary>
        /// µГµЅЙи±ёІОКэЕдЦГ¶Ф»°їт+А©Х№Цб  
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_SetDevCfg2", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetDevCfg2(bool bAxisShow0, bool bAxisShow1);

        /// <summary>
        /// ЙиЦГКэѕЭївµДЛщУР¶ФПуµДРэЧЄІОКэ,І»У°ПмКэѕЭµДПФКѕ,Ц»КЗјУ№¤К±ІЕ¶Ф¶ФПуЅшРРРэЧЄ
        /// dMoveX x·ЅПтТЖ¶ЇѕаАл
        /// dMoveY y·ЅПтТЖ¶ЇѕаАл
        /// dCenterXРэЧЄЦРРДµДxЧш±к
        /// dCenterYРэЧЄЦРРДµДyЧш±к      
        /// dRotateAngОЄРэЧЄЅЗ¶И,µҐО»ОЄ»Ў¶ИЦµ
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_SetRotateMoveParam", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetRotateMoveParam(double dMoveX, double dMoveY, double dCenterX, double dCenterY, double dRotateAng);

        #endregion

        #region јУ№¤

        /// <summary>
        /// ±кїМµ±З°КэѕЭївАпГжЛщУРКэѕЭТ»ґО
        /// Fly ±нКѕЅшРР·Й¶Ї±кїМ
        /// </summary>   
        [DllImport("MarkEzd", EntryPoint = "lmc1_Mark", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int Mark(bool Fly);

        /// <summary>
        /// ±кїМЦё¶Ё¶ФПуТ»ґО
        /// EntName Цё¶Ё¶ФПуµДГыЧЦ
        /// </summary>   
        [DllImport("MarkEzd", EntryPoint = "lmc1_MarkEntity", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int MarkEntity(string EntName);
        /// <summary>
        /// ±кїМЦё¶Ё¶ФПуТ»ґО
        /// EntName Цё¶Ё¶ФПуµДГыЧЦ
        /// </summary>   
        [DllImport("MES", EntryPoint = "MES_Login", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern bool MES_Login(char[] EntName);

        [DllImport("MES", EntryPoint = "MES_Init", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern bool MES_Init(char[] EntName);

        [DllImport("MES", EntryPoint = "MES_LogReset", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern bool MES_LogReset();


        [DllImport("MES", EntryPoint = " MES_Free", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern bool MES_Free(char[] EntName);

        [DllImport("MES", EntryPoint = "MES_CheckSerialNum", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern bool MES_CheckSerialNum(string ent, char[] name);

        /// <summary>
        ///ёщѕЭКдИлРЕєЕ·ЙРР±кїМ
        /// </summary>
        /// <returns></returns>
        [DllImport("MarkEzd", EntryPoint = "lmc1_MarkFlyByStartSignal", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int MarkFlyByStartSignal();

        /// <summary>
        /// ·ЙРР±кїМЦё¶Ё¶ФПуТ»ґО
        /// EntName Цё¶Ё¶ФПуµДГыЧЦ
        /// </summary>   
        [DllImport("MarkEzd", EntryPoint = "lmc1_MarkEntityFly", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int MarkEntityFly(string EntName);

        //јУ№¤Ц±ПЯ
        [DllImport("MarkEzd", EntryPoint = "lmc1_MarkLine", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int MarkLine(double X1, double Y1, double X2, double Y2, int Pen);

        /// <summary>
        /// ±кїМµг
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Delay"></param>
        /// <param name="Pen"></param>
        /// <returns></returns>
        [DllImport("MarkEzd", EntryPoint = "lmc1_MarkPoint", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int MarkPoint(double X, double Y, double Delay, int Pen);

        /// <summary>
        /// јУ№¤µг¶ФПу
        /// </summary>
        /// <param name="nPointNum">µгёцКэ</param>
        /// <param name="ptbuf">ptBuf[][2]ОЄГїёцµгµДМшЧЄЛЩ¶ИСУК±К±јдµҐО»ОЄus</param>
        /// <param name="dJumpSpeed">МшЧЄЛЩ¶И</param>
        /// <param name="dLaserOnTimeMs">іц№вК±јдµҐО»єБГлЧоРЎ0.01MS</param>
        /// <returns></returns>
        [DllImport("MarkEzd", EntryPoint = "lmc1_MarkPointBuf2", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int MarkPointBuf2(int nPointNum, [MarshalAs(UnmanagedType.LPArray)] double[,] ptbuf, double dJumpSpeed, double dLaserOnTimeMs);

        [DllImport("MarkEzd", EntryPoint = "lmc1_IsMarking", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern bool IsMarking();

        /// <summary>
        /// ЗїЦЖНЈЦ№±кїМ  
        /// </summary>   
        [DllImport("MarkEzd", EntryPoint = "lmc1_StopMark", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int StopMark();

        /// <summary>
        /// єм№вФ¤ААµ±З°КэѕЭївАпГжЛщУРКэѕЭТ»ґО
        /// </summary>   
        [DllImport("MarkEzd", EntryPoint = "lmc1_RedLightMark", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RedMark();

        /// <summary>
        /// єм№вФ¤ААµ±З°КэѕЭївАпГжЛщУРКэѕЭВЦАЄТ»ґО
        /// </summary>   
        [DllImport("MarkEzd", EntryPoint = "lmc1_RedLightMarkContour", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RedMarkContour();

        /// <summary>
        /// єм№вФ¤ААµ±З°КэѕЭївАпГжЦё¶Ё¶ФПу
        /// </summary>   
        [DllImport("MarkEzd", EntryPoint = "lmc1_RedLightMarkByEnt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RedLightMarkByEnt(string EntName, bool bContour);

        /// <summary>
        /// »сИЎБчЛ®ПЯЛЩ¶И
        /// </summary>
        /// <param name="FlySpeed"></param>
        /// <returns></returns>
        [DllImport("MarkEzd", EntryPoint = "lmc1_GetFlySpeed", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetFlySpeed(ref double FlySpeed);


        /// <summary>
        /// їШЦЖХсѕµЦ±ЅУФЛ¶ЇµЅЦё¶ЁЧш±к
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        [DllImport("MarkEzd", EntryPoint = "lmc1_GotoPos", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GotoPos(double x, double y);



        /// <summary>
        /// »сИЎµ±З°ХсѕµµДГьБоЧш±к
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        [DllImport("MarkEzd", EntryPoint = "lmc1_GetCurCoor", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetCurCoor(ref double x, ref double y);

        #endregion

        #region ОДјю

        /// <summary>
        /// ФШИлezdОДјюµЅµ±З°КэѕЭївАпГж,ІўЗеіэѕЙµДКэѕЭїв
        /// </summary>   
        [DllImport("MarkEzd", EntryPoint = "lmc1_LoadEzdFile", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int LoadEzdFile(string FileName);

        /// <summary>
        /// ±Јґжµ±З°КэѕЭµЅЦё¶ЁОДјю
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_SaveEntLibToFile", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SaveEntLibToFile(string strFileName);

        [DllImport("gdi32.dll")]
        internal static extern bool DeleteObject(IntPtr hObject);

        [DllImport("MarkEzd", EntryPoint = "lmc1_GetPrevBitmap2", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr GetCurPrevBitmap(int bmpwidth, int bmpheight);
        /// <summary>
        /// µГµЅµ±З°КэѕЭївАпГжКэѕЭµДФ¤ААНјЖ¬
        /// </summary>  
        public static Image GetCurPreviewImage(int bmpwidth, int bmpheight)
        {
            IntPtr pBmp = GetCurPrevBitmap(bmpwidth, bmpheight);
            Image img = Image.FromHbitmap(pBmp);
            DeleteObject(pBmp);
            return img;
        }

        [DllImport("MarkEzd", EntryPoint = "lmc1_GetPrevBitmapByName2", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr GetPrevBitmapByName(string EntName, int bmpwidth, int bmpheight);
        /// <summary>
        /// µГµЅµ±З°КэѕЭївАпГжЦё¶Ё¶ФПуµДФ¤ААНјЖ¬
        /// </summary>
        public static Image GetCurPreviewImageByName(string Entname, int bmpwidth, int bmpheight)
        {
            IntPtr pBmp = GetPrevBitmapByName(Entname, bmpwidth, bmpheight);
            Image img = Image.FromHbitmap(pBmp);
            DeleteObject(pBmp);
            return img;
        }

        #endregion

        #region ¶ФПу

        /// <summary>
        /// µГµЅЦё¶Ё¶ФПуµДіЯґзРЕПў
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_GetEntSize", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetEntSize(string strEntName, ref double dMinx, ref double dMiny, ref double dMaxx, ref double dMaxy, ref double dz);

        /// <summary>
        /// ТЖ¶ЇКэѕЭївЦРЦё¶ЁГыіЖµД¶ФПу
        /// dMoveX x·ЅПтТЖ¶ЇѕаАл
        /// dMoveY y·ЅПтТЖ¶ЇѕаАл
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_MoveEnt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int MoveEnt(string strEntName, double dMovex, double dMovey);

        /// <summary>
        /// ¶ФКэѕЭївЦРЦё¶ЁГыіЖµД¶ФПуЅшРР±ИАэ±д»»
        /// ±д»»З°ЛщУР№№іЙ¶ФПуµДµгµЅ±д»»ЦРРДѕаАл°ґ±д»»±ИАэ±д»»Ј¬¶ФУ¦ОЄ±д»»єуµДНјРОЧш±кЎЈ
        /// dCenx,dCeny±д»»ЦРРДµДЧш±к
        /// dScaleX x·ЅПт±д»»±ИАэ
        /// dScaleY y·ЅПт±д»»±ИАэ
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_ScaleEnt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ScaleEnt(string strEntName, double dCenx, double dCeny, double dScaleX, double dScaleY);

        /// <summary>
        /// ¶ФКэѕЭївЦРЦё¶ЁГыіЖµД¶ФПуЅшРРѕµПс±д»»
        /// dCenx,dCenyѕµПсЦРРДµДЧш±к
        /// bMirrorX= true XѕµПс 
        /// bMirrorY= true YѕµПс
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_MirrorEnt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int MirrorEnt(string strEntName, double dCenx, double dCeny, bool bMirrorX, bool bMirrorY);


        /// <summary>
        /// ¶ФКэѕЭївЦРЦё¶ЁГыіЖµД¶ФПуЅшРРРэЧЄ±д»»
        /// dCenxРэЧЄЦРРДµДxЧш±к
        /// dCenyРэЧЄЦРРДµДyЧш±к      
        /// dAngleОЄРэЧЄЅЗ¶И,µҐО»ОЄ»Ў¶ИЦµ
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_RotateEnt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RotateEnt(string strEntName, double dCenx, double dCeny, double dAngle);

        ///<summary>
        /// ёґЦЖЦё¶ЁГыіЖ¶ФПуЈ¬ІўГьГы
        ///<summary>
        [DllImport("MarkEzd", EntryPoint = "lmc1_CopyEnt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int CopyEnt(string strEntName, string strNewEntName);

        /// <summary>
        /// µГµЅ¶ФПуЧЬКэ
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_GetEntityCount", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern ushort GetEntityCount();

        [DllImport("MarkEzd", EntryPoint = "lmc1_GetEntityName", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        internal static extern int lmc1_GetEntityNameByIndex(int nEntityIndex, StringBuilder entname);


        /// <summary>
        /// µГµЅЦё¶ЁЛчТэєЕµД¶ФПуµДГыіЖЎЈ
        /// </summary>     
        public static string GetEntityNameByIndex(int nEntityIndex)
        {
            StringBuilder str = new StringBuilder("", 255);
            lmc1_GetEntityNameByIndex(nEntityIndex, str);
            return str.ToString();
        }

        /// <summary>
        /// Йи¶ЁЦё¶ЁЛчТэєЕµД¶ФПуµДГыіЖЎЈ
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_SetEntityName", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetEntityNameByIndex(int nEntityIndex, string entname);

        ///<summary>
        /// ЦШГьГыЦё¶ЁГыіЖ¶ФПу
        ///<summary>
        [DllImport("MarkEzd", EntryPoint = "lmc1_ChangeEntName", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ChangeEntName(string strEntName, string strNewEntName);


        /// <summary>
        /// ¶ФКэѕЭївЦРЦё¶ЁГыіЖµД¶ФПуИєЧй
        /// strEntName1±»ИєЧйµД¶ФПу1
        /// strEntName2±»ИєЧйµД¶ФПу2
        /// strGroupNameИєЧйєу¶ФПуГы      
        /// nGroupPenИєЧйєу¶ФПу±КєЕ
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_GroupEnt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GroupEnt(string strEntName1, string strEntName2, string strGroupName, int nGroupPen);

        /// <summary>
        /// ¶ФКэѕЭївЦРЦё¶ЁГыіЖµД¶ФПуЅвЙўИєЧй
        /// strEntName1±»ЅвЙўИєЧйµД¶ФПу
        /// </summary>
        [DllImport("MarkEzd", EntryPoint = "lmc1_UnGroupEnt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int UnGroupEnt(string strGroupName);

        [DllImport("MarkEzd", EntryPoint = "lmc1_GroupEnt2", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int lmc1_GroupEnt2(string[] strEntName, int nEntCount, string strGroupName, int nGroupPen);

        /// <summary>
        /// і№µЧЅвЙўИєЧй¶ФПуОЄЗъПЯ
        /// </summary>
        /// <param name="GroupName"></param>
        /// <param name="nFlag"></param>
        /// <returns></returns>
        [DllImport("MarkEzd", EntryPoint = "lmc1_UnGroupEnt2", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int UnGroupEnt2(string GroupName, int nFlag);

        /// <summary>
        /// »сИЎµ±З°НјЖ¬¶ФПуІОКэ
        /// </summary> 
        /// <param name="strEntName">О»Нј¶ФПуГыіЖ</param>
        /// <param name="strImageFileName">О»Нј¶ФПуВ·ѕ¶</param>
        /// <param name="nBmpAttrib">О»НјІОКэ</param>
        /// <param name="nScanAttrib">ЙЁГиІОКэ</param>
        /// <param name="dBrightness">ББ¶ИЙиЦГ[-1,1]</param>
        /// <param name="dContrast">¶Ф±И¶ИЙиЦГ[-1,1]</param>
        /// <param name="dPointTime">ґтµгК±јдЙиЦГ</param>
        /// <param name="nImportDpi">DPI</param>
        /// <returns></returns>
        [DllImport("MarkEzd", EntryPoint = "lmc1_GetBitmapEntParam2", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetBitmapEntParam2(string strEntName,
                                                        StringBuilder strImageFileName,
                                                        ref int nBmpAttrib,
                                                        ref int nScanAttrib,
                                                        ref double dBrightness,
                                                        ref double dContrast,
                                                        ref double dPointTime,
                                                        ref int nImportDpi,
                                                        ref int bDisableMarkLowGrayPt,
                                                        ref int nMinLowGrayPt
                                                        );

        [DllImport("MarkEzd", EntryPoint = "lmc1_SetBitmapEntParam2", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetBitmapEntParam(string strEntName,
                                                         string strImageFileName,
                                                         int nBmpAttrib,
                                                         int nScanAttrib,
                                                         double dBrightness,
                                                         double dContrast,
                                                         double dPointTime,
                                                         int nImportDpi,
                                                         bool bDisableMarkLowGrayPt,
                                                         int nMinLowGrayPt
                                                          );

        /// <summary>
        /// Ѕ«Цё¶Ё¶ФПуТЖ¶ЇµЅМШ¶Ф¶ФПуЗ°Гж
        /// </summary>
        /// <param name="nMoveEnt"></param>
        /// <param name="GoalEnt"></param>
        /// <returns></returns>
        [DllImport("MarkEzd", EntryPoint = "lmc1_MoveEntityBefore", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int MoveEntityBefore(int nMoveEnt, int GoalEnt);

        [DllImport("MarkEzd", EntryPoint = "lmc1_SetBitmapEntParam3", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetBitmapEntParam3(string strEntName,
                                                        double dDpiX,
                                                        double dDpiY,
                                                        [MarshalAs(UnmanagedType.LPArray)] byte[] bGrayScaleBuf);
        [DllImport("MarkEzd", EntryPoint = "lmc1_GetBitmapEntParam3", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetBitmapEntParam3(string strEntName,
                                                          ref double dDpiX,
                                                          ref double dDpiY,
                                                          byte[] bGrayScaleBuf);

        /// <summary>
        /// Ѕ«Цё¶Ё¶ФПуТЖ¶ЇµЅМШ¶Ё¶ФПуєуГж
        /// </summary>
        /// <param name="nMoveEnt"></param>
        /// <param name="GoalEnt"></param>
        /// <returns></returns>
        [DllImport("MarkEzd", EntryPoint = "lmc1_MoveEntityAfter", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int MoveEntityAfter(int nMoveEnt, int GoalEnt);


        /// <summary>
        /// 20140228 Ѕ«ЛщУР¶ФПуЛіРтµЯµ№
        /// </summary>
        /// <param name="nMoveEnt"></param>
        /// <param name="GoalEnt"></param>
        /// <returns></returns>
        [DllImport("MarkEzd", EntryPoint = "lmc1_ReverseAllEntOrder", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ReverseAllEntOrder();


        #endregion

        #region ¶ЛїЪ

        /// <summary>
        /// ¶Бµ±З°КдИл¶ЛїЪ
        /// data ОЄµ±З°КдИл¶ЛїЪµДЦµ,
        /// Bit0КЗIn0µДЧґМ¬,Bit0=1±нКѕIn0ОЄёЯµзЖЅ,Bit0=0±нКѕIn0ОЄµНµзЖЅ
        /// Bit1КЗIn1µДЧґМ¬,Bit1=1±нКѕIn1ОЄёЯµзЖЅ,Bit1=0±нКѕIn1ОЄµНµзЖЅ
        /// ........
        /// Bit15КЗIn15µДЧґМ¬,Bit15=1±нКѕIn15ОЄёЯµзЖЅ,Bit15=0±нКѕIn15ОЄµНµзЖЅ
        /// </summary>   
        [DllImport("MarkEzd", EntryPoint = "lmc1_ReadPort", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ReadPort(ref ushort data);

        /// <summary>
        /// ЙиЦГµ±З°Кдіц¶ЛїЪКдіц
        /// data ОЄµ±З°КдїЪ¶ЛїЪТЄЙиЦГµДЦµ,
        /// Bit0КЗOut0µДЧґМ¬,Bit0=1±нКѕOut0ОЄёЯµзЖЅ,Bit0=0±нКѕOut0ОЄµНµзЖЅ
        /// Bit1КЗOut1µДЧґМ¬,Bit1=1±нКѕOut1ОЄёЯµзЖЅ,Bit1=0±нКѕOut1ОЄµНµзЖЅ
        /// ........
        /// Bit15КЗOut15µДЧґМ¬,Bit15=1±нКѕOut15ОЄёЯµзЖЅ,Bit15=0±нКѕOut15ОЄµНµзЖЅ
        /// </summary>  
        [DllImport("MarkEzd", EntryPoint = "lmc1_WritePort", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int WritePort(ushort data);

        // »сИЎµ±З°Йи±ёКдіцїЪЧґМ¬Цµ
        [DllImport("MarkEzd", EntryPoint = "lmc1_GetOutPort", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetOutPort(ref ushort data);

        //Ц±ЅУґтїЄј¤№в
        [DllImport("MarkEzd", EntryPoint = "lmc1_LaserOn", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int LaserOn(bool bOpen);

        #endregion

        #region ОД±ѕ

        /// <summary>
        /// ёьёДКэѕЭївЦРЦё¶ЁГыіЖµДОД±ѕ¶ФПуµДДЪИЭОЄЦё¶ЁОД±ѕ
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_ChangeTextByName", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ChangeTextByName(string EntName, string NewText);

        ///<summary>
        ///µГµЅЦё¶Ё¶ФПуµДОД±ѕ
        ///<summary>
        [DllImport("MarkEzd", EntryPoint = "lmc1_GetTextByName", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetTextByName(string strEntName, StringBuilder Text);

        /// <summary>
        /// ЦШЦГРтБРєЕ¶ФПу
        /// </summary>
        /// <param name="TextName"></param>
        /// <returns></returns>
        [DllImport("MarkEzd", EntryPoint = "lmc1_TextResetSn", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int TextResetSn(string TextName);

        #region ЧЦМе

        public const uint LMC1_FONT_JSF = 1; //JczSingleLineЧЦМе
        public const uint LMC1_FONT_TTF = 2; //TrueTypeЧЦМе
        public const uint LMC1_FONT_DMF = 4; //DotMatrixЧЦМе
        public const uint LMC1_FONT_BCF = 8; //BarcodeЧЦМе

        public struct FontRecord
        {
            public string fontname;//ЧЦМеГыіЖ
            public uint fontattrib;//ЧЦМеКфРФ
        }

        /// <summary>
        /// µГµЅКэѕЭївЦРЧЦМејЗВјµДЧЬКэ
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_GetFontRecordCount", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetFontRecordCount(ref int fontCount);

        /// <summary>
        /// µГµЅПµНіЦРЦё¶ЁРтєЕµДЧЦМејЗВјµДГыіЖєНКфРФ
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_GetFontRecord", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetFontRecordByIndex(int fontIndex, StringBuilder fontName, ref uint fontAttrib);

        /// <summary>
        /// µГµЅПµНіЦРЛщУРїЙТФК№УГµДЧЦМеГыіЖєНКфРФ
        /// </summary> 
        public static bool GetAllFontRecord(ref FontRecord[] fonts)
        {
            int fontnum = 0;
            if (GetFontRecordCount(ref fontnum) != 0)
            {
                return false;
            }
            if (fontnum == 0)
            {
                return true;
            }
            fonts = new FontRecord[fontnum];
            StringBuilder str = new StringBuilder("", 255);
            uint fontAttrib = 0;
            for (int i = 0; i < fontnum; i++)
            {
                GetFontRecordByIndex(i, str, ref fontAttrib);
                fonts[i].fontname = str.ToString(); ;
                fonts[i].fontattrib = fontAttrib;
            }
            return true;
        }

        #endregion

        /// <summary>
        /// »сИЎЧЦМеµДІОКэЈ¬І»КЗХл¶Ф¶ФПуµДЈ¬¶шКЗХл¶ФЧЦМе
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_GetFontParam3", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetFontParam3(string strFontName,
                                                     ref double CharHeight,
                                                     ref double CharWidthRatio,
                                                     ref double CharAngle,
                                                     ref double CharSpace,
                                                     ref double LineSpace,
                                                     ref bool EqualCharWidth,
                                                     ref int nTextAlign,
                                                     ref bool bBold,
                                                     ref bool bItalic);

        /// <summary>
        /// ЙиЦГЧЦМеµДІОКэЈ¬ФЪПВґОМнјУОД±ѕµДК±єтЖрР§
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_SetFontParam3", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetFontParam3(string fontname,
                                                    double CharHeight,
                                                    double CharWidthRatio,
                                                    double CharAngle,
                                                    double CharSpace,
                                                    double LineSpace,
                                                    double spaceWidthRatio,
                                                    bool EqualCharWidth,
                                                    int nTextAlign,
                                                    bool bBold,
                                                    bool bItalic);



        /// <summary>
        /// µГµЅЦё¶ЁОД±ѕ¶ФПуІОКэ
        /// </summary>
        /// <param name="EntName"></param>¶ФПуГыіЖ
        /// <param name="FontName"></param>ЧЦМеГыіЖ
        /// <param name="CharHeight"></param>ЧЦ·ыёЯ¶И
        /// <param name="CharWidthRatio"></param>ЧЦ·ыїн¶И
        /// <param name="CharAngle"></param>ЧЦ·ыЗгЅЗ
        /// <param name="CharSpace"></param>ЧЦ·ыјдѕа
        /// <param name="LineSpace"></param>РРјдѕаlmc1_GetEzdFilePrevBitmap
        /// <param name="EqualCharWidth"></param>µИЧЦ·ыїн¶И
        /// <returns></returns>
        [DllImport("MarkEzd", EntryPoint = "lmc1_GetTextEntParam", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetTextEntParam(string EntName,
                                                         StringBuilder FontName,
                                                      ref double CharHeight,
                                                      ref double CharWidthRatio,
                                                      ref double CharAngle,
                                                      ref double CharSpace,
                                                      ref double LineSpace,
                                                      ref bool EqualCharWidth);

        [DllImport("MarkEzd", EntryPoint = "lmc1_SetTextEntParam", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetTextEntParam(string EntName,
                                                        double CharHeight,
                                                        double CharWidthRatio,
                                                        double CharAngle,
                                                        double CharSpace,
                                                        double LineSpace,
                                                        bool EqualCharWidth);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="EntName"></param>
        /// <param name="FontName"></param>
        /// <param name="CharHeight"></param>
        /// <param name="CharWidthRatio"></param>
        /// <param name="CharAngle"></param>
        /// <param name="CharSpace"></param>
        /// <param name="LineSpace"></param>
        /// <param name="spaceWidthRatio"></param>
        /// <param name="EqualCharWidth"></param>
        /// <returns></returns>
        [DllImport("MarkEzd", EntryPoint = "lmc1_GetTextEntParam2", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetTextEntParam2(string EntName,
                                                        StringBuilder FontName,
                                                     ref double CharHeight,
                                                     ref double CharWidthRatio,
                                                     ref double CharAngle,
                                                     ref double CharSpace,
                                                     ref double LineSpace,
                                                     ref double spaceWidthRatio,
                                                     ref bool EqualCharWidth);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="EntName"></param>
        /// <param name="fontname"></param>
        /// <param name="CharHeight"></param>
        /// <param name="CharWidthRatio"></param>
        /// <param name="CharAngle"></param>
        /// <param name="CharSpace"></param>
        /// <param name="LineSpace"></param>
        /// <param name="spaceWidthRatio"></param>
        /// <param name="EqualCharWidth"></param>
        /// <returns></returns>
        [DllImport("MarkEzd", EntryPoint = "lmc1_SetTextEntParam2", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetTextEntParam2(string EntName,
                                                         string fontname,
                                                        double CharHeight,
                                                        double CharWidthRatio,
                                                        double CharAngle,
                                                        double CharSpace,
                                                        double LineSpace,
                                                        double spaceWidthRatio,
                                                        bool EqualCharWidth);


        [DllImport("MarkEzd", EntryPoint = "lmc1_GetTextEntParam4", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetTextEntParam4(string EntName,
                                                       StringBuilder FontName,
                                                       ref int nTextSpaceMode,
                                                       ref double dTextSpace,
                                                ref double CharHeight,
                                                ref double CharWidthRatio,
                                                ref double CharAngle,
                                                ref double CharSpace,
                                                ref double LineSpace,
                                                ref double dNullCharWidthRatio,
                                                ref int nTextAlign,
                                                ref bool bBold,
                                                ref bool bItalic);



        [DllImport("MarkEzd", EntryPoint = "lmc1_SetTextEntParam4", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetTextEntParam4(string EntName,
                                                   string fontname,
                                                   int nTextSpaceMode,
                                                  double dTextSpace,
                                                  double CharHeight,
                                                  double CharWidthRatio,
                                                  double CharAngle,
                                                  double CharSpace,
                                                  double LineSpace,
                                                  double spaceWidthRatio,
                                                    int nTextAlign,
                                                    bool bBold,
                                                    bool bItalic);

        [DllImport("MarkEzd", EntryPoint = "lmc1_GetCircleTextParam", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetCircleTextParam(string pEntName,
                                                  ref double dCenX,
                                                  ref double dCenY,
                                                  ref double dCenZ,
                                                  ref double dCirDiameter,
                                                  ref double dCirBaseAngle,
                                                  ref bool bCirEnableAngleLimit,
                                                  ref double dCirAngleLimit,
                                                  ref int nCirTextFlag);

        [DllImport("MarkEzd", EntryPoint = "lmc1_SetCircleTextParam", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetCircleTextParam(string pEntName,
                                                    double dCenX,
                                                    double dCenY,
                                                    double dCenZ,
                                                    double dCirDiameter,
                                                    double dCirBaseAngle,
                                                    bool bCirEnableAngleLimit,
                                                    double dCirAngleLimit,
                                                    int nCirTextFlag);
        #endregion

        #region ±КєЕ

        /// <summary>
        /// µГµЅЦё¶Ё±КєЕµДІОКэ
        /// nPenNoТЄЙиЦГµД±КєЕ0-255
        /// nMarkLoop±кїМґОКэ
        /// dMarkSpeed јУ№¤ЛЩ¶И mm/s»тХЯinch/mm,ИЎѕцУЪmarkdll.dllµДµ±З°µҐО»
        /// dPowerRatio№¦ВК°Щ·Ц±И 0-100%
        /// dCurrentµзБчA
        /// nFreqЖµВКHz
        /// nQPulseWidth QВціеїн¶И us
        /// nStartTC їЄ№вСУК± us
        /// nLaserOffTC №Ш№вСУК± us
        /// nEndTC  ЅбКшСУК± us
        /// nPolyTC ¶а±ЯРО№ХЅЗСУК±us
        /// dJumpSpeed МшЧЄЛЩ¶И mm/s»тХЯinch/mm,ИЎѕцУЪmarkdll.dllµДµ±З°µҐО» 
        /// nJumpPosTC МшЧЄО»ЦГСУК± us
        /// nJumpDistTC МшЧЄѕаАлСУК± us
        /// dEndComp Д©µгІ№іҐѕаАл mm»тХЯinch,ИЎѕцУЪmarkdll.dllµДµ±З°µҐО» 
        /// dAccDist јУЛЩѕаАл mm»тХЯinch
        /// dPointTime ґтµгК±јдms
        /// bPulsePointMode ґтµгДЈКЅ trueК№ДЬ
        /// nPulseNum ґтµгёцКэ
        /// dFlySpeed БчЛ®ПЯЛЩ¶И mm/s»тХЯinch/mm
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_GetPenParam", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetPenParam(int nPenNo,
                     ref int nMarkLoop,
                     ref double dMarkSpeed,
                     ref double dPowerRatio,
                     ref double dCurrent,
                     ref int nFreq,
                     ref double dQPulseWidth,
                     ref int nStartTC,
                     ref int nLaserOffTC,
                     ref int nEndTC,
                     ref int nPolyTC,
                     ref double dJumpSpeed,
                     ref int nJumpPosTC,
                     ref int nJumpDistTC,
                     ref double dEndComp,
                     ref double dAccDist,
                     ref double dPointTime,
                     ref bool bPulsePointMode,
                     ref int nPulseNum,
                     ref double dFlySpeed);

        /// <summary>
        /// 20111201 МнјУ
        /// µГµЅЦё¶Ё±КєЕµДІОКэ
        /// nPenNoТЄЙиЦГµД±КєЕ0-255
        /// nMarkLoop±кїМґОКэ
        /// dMarkSpeed јУ№¤ЛЩ¶И mm/s»тХЯinch/mm,ИЎѕцУЪmarkdll.dllµДµ±З°µҐО»
        /// dPowerRatio№¦ВК°Щ·Ц±И 0-100%
        /// dCurrentµзБчA
        /// nFreqЖµВКHz
        /// nQPulseWidth QВціеїн¶И us
        /// nStartTC їЄ№вСУК± us
        /// nLaserOffTC №Ш№вСУК± us
        /// nEndTC  ЅбКшСУК± us
        /// nPolyTC ¶а±ЯРО№ХЅЗСУК±us
        /// dJumpSpeed МшЧЄЛЩ¶И mm/s»тХЯinch/mm,ИЎѕцУЪmarkdll.dllµДµ±З°µҐО» 
        /// nJumpPosTC МшЧЄО»ЦГСУК± us
        /// nJumpDistTC МшЧЄѕаАлСУК± us
        /// dEndComp Д©µгІ№іҐѕаАл mm»тХЯinch,ИЎѕцУЪmarkdll.dllµДµ±З°µҐО» 
        /// dAccDist јУЛЩѕаАл mm»тХЯinch
        /// dPointTime ґтµгК±јдms
        /// bPulsePointMode ґтµгДЈКЅ trueК№ДЬ
        /// nPulseNum ґтµгёцКэ
        /// dFlySpeed БчЛ®ПЯЛЩ¶И mm/s»тХЯinch/mm
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_GetPenParam2", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetPenParam2(int nPenNo,
                     ref int nMarkLoop,
                     ref double dMarkSpeed,
                     ref double dPowerRatio,
                     ref double dCurrent,
                     ref int nFreq,
                     ref double dQPulseWidth,
                     ref int nStartTC,
                     ref int nLaserOffTC,
                     ref int nEndTC,
                     ref int nPolyTC,
                     ref double dJumpSpeed,
                     ref int nJumpPosTC,
                     ref int nJumpDistTC,
                     ref double dPointTime,
                     ref int nSpiWave,
                     ref bool bWobbleMode,
                     ref double bWobbleDiameter,
                     ref double bWobbleDist);


        [DllImport("MarkEzd", EntryPoint = "lmc1_GetPenParam4", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetPenParam4(int nPenNo,
                                                        StringBuilder pStrName,
                                                        ref int clr,
                                                        ref bool bDisableMark,
                                                        ref bool bUseDefParam,
                                                        ref int nMarkLoop,
                                                        ref double dMarkSpeed,
                                                        ref double dPowerRatio,
                                                        ref double dCurrent,
                                                        ref int nFreq,
                                                        ref double dQPulseWidth,
                                                        ref int nStartTC,
                                                        ref int nLaserOffTC,
                                                        ref int nEndTC,
                                                        ref int nPolyTC,
                                                        ref double dJumpSpeed,
                                                        ref int nMinJumpDelayTCUs,
                                                        ref int nMaxJumpDelayTCUs,
                                                        ref double dJumpLengthLimit,
                                                        ref double dPointTimeMs,
                                                        ref bool nSpiSpiContinueMode,
                                                        ref int nSpiWave,
                                                        ref int nYagMarkMode,
                                                        ref bool bPulsePointMode,
                                                        ref int nPulseNum,
                                                        ref bool bEnableACCMode,
                                                        ref double dEndComp,
                                                        ref double dAccDist,
                                                        ref double dBreakAngle,
                                                        ref bool bWobbleMode,
                                                        ref double bWobbleDiameter,
                                                        ref double bWobbleDist);


        /// <summary>
        /// ЙиЦГЦё¶Ё±КєЕµДІОКэ
        /// nPenNoТЄЙиЦГµД±КєЕ0-255
        /// nMarkLoop±кїМґОКэ
        /// dMarkSpeed јУ№¤ЛЩ¶И mm/s»тХЯinch/mm,ИЎѕцУЪmarkdll.dllµДµ±З°µҐО»
        /// dPowerRatio№¦ВК°Щ·Ц±И 0-100%
        /// dCurrentµзБчA
        /// nFreqЖµВКHz
        /// nQPulseWidth QВціеїн¶И us
        /// nStartTC їЄ№вСУК± us
        /// nLaserOffTC №Ш№вСУК± us
        /// nEndTC  ЅбКшСУК± us
        /// nPolyTC ¶а±ЯРО№ХЅЗСУК±us
        /// dJumpSpeed МшЧЄЛЩ¶И mm/s»тХЯinch/mm,ИЎѕцУЪmarkdll.dllµДµ±З°µҐО» 
        /// nJumpPosTC МшЧЄО»ЦГСУК± us
        /// nJumpDistTC МшЧЄѕаАлСУК± us
        /// dEndComp Д©µгІ№іҐѕаАл mm»тХЯinch,ИЎѕцУЪmarkdll.dllµДµ±З°µҐО» 
        /// dAccDist јУЛЩѕаАл mm»тХЯinch
        /// dPointTime ґтµгК±јдms
        /// bPulsePointMode ґтµгДЈКЅ trueК№ДЬ
        /// nPulseNum ґтµгёцКэ
        /// dFlySpeed БчЛ®ПЯЛЩ¶И mm/s»тХЯinch/mm
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_SetPenParam", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetPenParam(int nPenNo,
                             int nMarkLoop,
                             double dMarkSpeed,
                             double dPowerRatio,
                             double dCurrent,
                             int nFreq,
                             double dQPulseWidth,
                             int nStartTC,
                             int nLaserOffTC,
                             int nEndTC,
                             int nPolyTC,
                             double dJumpSpeed,
                             int nJumpPosTC,
                             int nJumpDistTC,
                             double dEndComp,
                             double dAccDist,
                             double dPointTime,
                             bool bPulsePointMode,
                             int nPulseNum,
                             double dFlySpeed);


        public static int SetPenParams(PenParams penParams) => SetPenParam(
                                             nPenNo: penParams.PenNo,
                                             nMarkLoop: penParams.MarkLoop,
                                             dMarkSpeed: penParams.MarkSpeed,
                                             dPowerRatio: penParams.PowerRatio,
                                             dCurrent: penParams.Current,
                                             nFreq: penParams.Freq,
                                             dQPulseWidth: penParams.QPulseWidth,
                                             nStartTC: penParams.StartTC,
                                             nLaserOffTC: penParams.LaserOffTC,
                                             nEndTC: penParams.EndTC,
                                             nPolyTC: penParams.PolyTC,
                                             dJumpSpeed: penParams.JumpSpeed,
                                             nJumpPosTC: penParams.JumpPosTC,
                                             nJumpDistTC: penParams.JumpDistTC,
                                             dEndComp: penParams.EndComp,
                                             dAccDist: penParams.AccDist,
                                             dPointTime: penParams.PointTime,
                                             bPulsePointMode: penParams.PulsePointMode,
                                             nPulseNum: penParams.PulseNum,
                                             dFlySpeed: penParams.FlySpeed);


        //////////////////////////////////////
        ///20110329МнјУёьёДЙиЦГ±КєЕІОКэ
        /////////////////////////////////////////
        /// <summary>
        /// ЙиЦГЦё¶Ё±КєЕµДІОКэ
        /// nPenNoТЄЙиЦГµД±КєЕ0-255
        /// nMarkLoop±кїМґОКэ
        /// dMarkSpeed јУ№¤ЛЩ¶И mm/s»тХЯinch/mm,ИЎѕцУЪmarkdll.dllµДµ±З°µҐО»
        /// dPowerRatio№¦ВК°Щ·Ц±И 0-100%
        /// dCurrentµзБчA
        /// nFreqЖµВКHz
        /// nQPulseWidth QВціеїн¶И us
        /// nStartTC їЄ№вСУК± us
        /// nLaserOffTC №Ш№вСУК± us
        /// nEndTC  ЅбКшСУК± us
        /// nPolyTC ¶а±ЯРО№ХЅЗСУК±us
        /// dJumpSpeed МшЧЄЛЩ¶И mm/s»тХЯinch/mm,ИЎѕцУЪmarkdll.dllµДµ±З°µҐО» 
        /// nJumpPosTC МшЧЄО»ЦГСУК± us
        /// nJumpDistTC МшЧЄѕаАлСУК± us
        /// nSpiWave SPIІЁРОСЎФс
        /// bWobbleMode ¶¶¶ЇДЈКЅ
        /// bWobbleDiameter ¶¶¶ЇЦ±ѕ¶
        /// bWobbleDist ¶¶¶Їјдѕа
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_SetPenParam2", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetPenParam2(int nPenNo,
                                                        int nMarkLoop,
                                                        double dMarkSpeed,
                                                        double dPowerRatio,
                                                        double dCurrent,
                                                        int nFreq,
                                                        double dQPulseWidth,
                                                        int nStartTC,
                                                        int nLaserOffTC,
                                                        int nEndTC,
                                                        int nPolyTC,
                                                        double dJumpSpeed,
                                                        int nJumpPosTC,
                                                        int nJumpDistTC,
                                                        double dPointTime,
                                                        int nSpiWave,
                                                        bool bWobbleMode,
                                                        double bWobbleDiameter,
                                                        double bWobbleDist);


        public static int ColorToCOLORREF(Color color)
        {
            return ((color.R | (color.G << 8)) | (color.B << 0x10));
        }

        public static Color COLORREFToColor(int colorRef)
        {
            byte[] _IntByte = BitConverter.GetBytes(colorRef);
            return Color.FromArgb(_IntByte[0], _IntByte[1], _IntByte[2]);
        }

        [DllImport("MarkEzd", EntryPoint = "lmc1_SetPenParam4", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetPenParam4(int nPenNo,//±КєЕ
                                                            string pStrName,    // ГыіЖ
                                                            int clr,//СХЙ«
                                                            bool bDisableMark,//К№ДЬјУ№¤
                                                            bool bUseDefParam,//К№УГД¬ИПІОКэ
                                                            int nMarkLoop,//јУ№¤ґОКэ
                                                            double dMarkSpeed,//јУ№¤ЛЩ¶И
                                                            double dPowerRatio,//№¦ВК %
                                                            double dCurrent,//µзБч,A
                                                            int nFreq,//ЖµВК HZ
                                                            double dQPulseWidth,//ВцїнЈ¬yag us    ylpm ns
                                                            int nStartTC,//їЄ№вСУК±
                                                            int nLaserOffTC,//№Ш№вСУК±
                                                            int nEndTC,//ЅбКшСУК±
                                                            int nPolyTC,//№ХЅЗСУК±
                                                            double dJumpSpeed,//МшЧЄЛЩ¶И
                                                            int nMinJumpDelayTCUs,//ЧоРЎМшЧЄСУК±
                                                            int nMaxJumpDelayTCUs,//ЧоґуМшЧЄСУК±
                                                            double dJumpLengthLimit,//МшЧЄѕаАлгРЦµ
                                                            double dPointTimeMs,//ґтµгК±јд
                                                            bool nSpiSpiContinueMode,//SPIБ¬РшДЈКЅ
                                                            int nSpiWave,//SPIІЁРО±аєЕ
                                                            int nYagMarkMode,//YAGУЕ»ЇДЈКЅ
                                                            bool bPulsePointMode,//ВціеґтµгДЈКЅ
                                                            int nPulseNum,//ВціеґтµгВціеКэБї
                                                            bool bEnableACCMode,//ЖфУГјУјхЛЩУЕ»Ї
                                                            double dEndComp,//јУЛЩ
                                                            double dAccDist,//јхЛЩ
                                                            double dBreakAngle,//ЦР¶ПЅЗ¶И
                                                            bool bWobbleMode,//¶¶¶ЇДЈКЅ
                                                            double bWobbleDiameter,//¶¶¶ЇЦ±ѕ¶
                                                            double bWobbleDist);//¶¶¶ЇПЯјдѕа


        /// <summary>
        ///ЙиЦГ±КєЕКЗ·с±кїМ
        /// </summary>
        /// <param name="nPenNo"></param>
        /// <param name="bDisableMark"></param>
        /// <returns></returns>
        [DllImport("MarkEzd", EntryPoint = "lmc1_SetPenDisableState", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetPenDisableState(int nPenNo, bool bDisableMark);


        /// <summary>
        /// »сИЎ±КєЕКЗ·с±кїМ
        /// </summary>
        /// <param name="nPenNo"></param>
        /// <param name="bDisableMark"></param>
        /// <returns></returns>
        [DllImport("MarkEzd", EntryPoint = "lmc1_GetPenDisableState", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetPenDisableState(int nPenNo, ref bool bDisableMark);

        ///<summary>
        ///»сИЎЦё¶ЁГыіЖ¶ФПу±КєЕ
        ///<summary>
        [DllImport("MarkEzd", EntryPoint = "lmc1_GetPenNumberFromName", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetPenNumberFromName(string strEntName);

        /// <summary>
        /// »сИЎ¶ФПу±КєЕ
        /// </summary>
        /// <param name="strEntName"></param>
        /// <returns></returns>
        [DllImport("MarkEzd", EntryPoint = "lmc1_GetPenNumberFromEnt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetPenNumberFromEnt(string strEntName);

        ///<summary>
        ///¶ФПуУ¦УГ±КєЕЙиЦГЈЁХл¶ФКёБїНјОДјюЈ©
        ///<summary>
        [DllImport("MarkEzd", EntryPoint = "lmc1_SetEntAllChildPen", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetEntAllChildPen(string strEntName, int nPenNo);

        #endregion

        #region Моід
        public const int HATCHATTRIB_ALLCALC = 0x01;//И«Ії¶ФПуХыМејЖЛг
        public const int HATCHATTRIB_EDGE = 0x02;//ИЖ±ЯЧЯТ»ґО
        public const int HATCHATTRIB_MINUP = 0x04;//ЧоЙЩЖр±К
        public const int HATCHATTRIB_BIDIR = 0x08;//Л«ПтМоід
        public const int HATCHATTRIB_LOOP = 0x10;//»·РРМоід
        public const int HATCHATTRIB_OUT = 0x20;//»·РРУЙДЪПтНв
        public const int HATCHATTRIB_AUTOROT = 0x40;//ЧФ¶ЇЅЗ¶ИРэЧЄ
        public const int HATCHATTRIB_AVERAGELINE = 0x80;//ЧФ¶Ї·ЦІјМоідПЯ
        public const int HATCHATTRIB_CROSSLINE = 0x400;//Ѕ»ІжМоід

        /// <summary>
        /// ЙиЦГµ±З°µДМоідІОКэ,Из№ыПтКэѕЭївМнјУ¶ФПуК±К№ДЬМоід,ПµНі»бУГґЛєЇКэЙиЦГµДІОКэАґМоід
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_SetHatchParam", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetHatchParam(bool bEnableContour,
                          int bEnableHatch1,
                          int nPenNo1,
                          int nHatchAttrib1,
                          double dHatchEdgeDist1,
                          double dHatchLineDist1,
                          double dHatchStartOffset1,
                          double dHatchEndOffset1,
                          double dHatchAngle1,
                          int bEnableHatch2,
                          int nPenNo2,
                          int nHatchAttrib2,
                          double dHatchEdgeDist2,
                          double dHatchLineDist2,
                          double dHatchStartOffset2,
                          double dHatchEndOffset2,
                          double dHatchAngle2);

        public static int SetHatchParams(HatchParams hatchParams) => SetHatchParam(
           bEnableContour: hatchParams.EnableContour,
           bEnableHatch1: hatchParams.EnableHatch ? 1:0,
           nPenNo1: hatchParams.PenNo,
           nHatchAttrib1: hatchParams.HatchAttribute,//HATCHATTRIB_LOOP,//40
           dHatchEdgeDist1: hatchParams.HatchEdgeDist,
           dHatchLineDist1: hatchParams.HatchLineDist,
           dHatchStartOffset1: hatchParams.HatchStartOffset,
           dHatchEndOffset1: hatchParams.HatchEndOffset,
           dHatchAngle1: hatchParams.HatchRotateAngle,
           bEnableHatch2: 0,
           nPenNo2: 0,
           nHatchAttrib2: 48,
           dHatchEdgeDist2: 0,
           dHatchLineDist2: 0,
           dHatchStartOffset2: 0,
           dHatchEndOffset2: 0,
           dHatchAngle2: 0);

        /// <summary>
        /// ЙиЦГµ±З°µДМоідІОКэ2,Из№ыПтКэѕЭївМнјУ¶ФПуК±К№ДЬМоід,ПµНі»бУГґЛєЇКэЙиЦГµДІОКэАґМоід  20120911add  2.7.2
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_SetHatchParam2", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetHatchParam2(bool bEnableContour,//К№ДЬВЦАЄ±ѕЙн
                                                          int nParamIndex,//МоідІОКэРтєЕЦµОЄ1,2,3
                                                          int bEnableHatch,//К№ДЬМоід
                                                          int nPenNo,//МоідІОКэ±КєЕ
                                                          int nHatchType,//МоідАаРН 0µҐПт 1Л«Пт 2»ШРО 3№­РО 4№­РОІ»·ґПт
                                                          bool bHatchAllCalc,//КЗ·сИ«Ії¶ФПуЧчОЄХыМеТ»ЖрјЖЛг
                                                          bool bHatchEdge,//ИЖ±ЯТ»ґО
                                                          bool bHatchAverageLine,//ЧФ¶ЇЖЅѕщ·ЦІјПЯ
                                                          double dHatchAngle,//МоідПЯЅЗ¶И 
                                                          double dHatchLineDist,//МоідПЯјдѕа
                                                          double dHatchEdgeDist,//МоідПЯ±Яѕа    
                                                          double dHatchStartOffset,//МоідПЯЖрКјЖ«ТЖѕаАл
                                                          double dHatchEndOffset,//МоідПЯЅбКшЖ«ТЖѕаАл
                                                          double dHatchLineReduction,//Ц±ПЯЛхЅш
                                                          double dHatchLoopDist,//»·јдѕа
                                                          int nEdgeLoop,//»·Кэ
                                                          bool nHatchLoopRev,//»·РО·ґЧЄ
                                                          bool bHatchAutoRotate,//КЗ·сЧФ¶ЇРэЧЄЅЗ¶И
                                                          double dHatchRotateAngle//ЧФ¶ЇРэЧЄЅЗ¶И   
                                                       );

        /// <summary>
        /// ЙиЦГµ±З°µДМоідІОКэ3,Из№ыПтКэѕЭївМнјУ¶ФПуК±К№ДЬМоід,ПµНі»бУГґЛєЇКэЙиЦГµДІОКэАґМоід  20170330add  2.14.9
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_SetHatchParam3", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetHatchParam3(bool bEnableContour,//К№ДЬВЦАЄ±ѕЙн
                                                          int nParamIndex,//МоідІОКэРтєЕЦµОЄ1,2,3
                                                          int bEnableHatch,//К№ДЬМоід
                                                          int nPenNo,//МоідІОКэ±КєЕ
                                                          int nHatchType,//МоідАаРН 0µҐПт 1Л«Пт 2»ШРО 3№­РО 4№­РОІ»·ґПт
                                                          bool bHatchAllCalc,//КЗ·сИ«Ії¶ФПуЧчОЄХыМеТ»ЖрјЖЛг
                                                          bool bHatchEdge,//ИЖ±ЯТ»ґО
                                                          bool bHatchAverageLine,//ЧФ¶ЇЖЅѕщ·ЦІјПЯ
                                                          double dHatchAngle,//МоідПЯЅЗ¶И 
                                                          double dHatchLineDist,//МоідПЯјдѕа
                                                          double dHatchEdgeDist,//МоідПЯ±Яѕа    
                                                          double dHatchStartOffset,//МоідПЯЖрКјЖ«ТЖѕаАл
                                                          double dHatchEndOffset,//МоідПЯЅбКшЖ«ТЖѕаАл
                                                          double dHatchLineReduction,//Ц±ПЯЛхЅш
                                                          double dHatchLoopDist,//»·јдѕа
                                                          int nEdgeLoop,//»·Кэ
                                                          bool nHatchLoopRev,//»·РО·ґЧЄ
                                                          bool bHatchAutoRotate,//КЗ·сЧФ¶ЇРэЧЄЅЗ¶И
                                                          double dHatchRotateAngle,//ЧФ¶ЇРэЧЄЅЗ¶И  
                                                          bool bHatchCross
                                                       );

        [DllImport("MarkEzd", EntryPoint = "lmc1_GetHatchParam3", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetHatchParam3(ref bool bEnableContour,
                                                         int nParamIndex,
                                                         ref int bEnableHatch,
                                                         ref int nPenNo,
                                                         ref int nHatchType,
                                                         ref bool bHatchAllCalc,
                                                         ref bool bHatchEdge,
                                                         ref bool bHatchAverageLine,
                                                         ref double dHatchAngle,
                                                         ref double dHatchLineDist,
                                                         ref double dHatchEdgeDist,
                                                         ref double dHatchStartOffset,
                                                         ref double dHatchEndOffset,
                                                         ref double dHatchLineReduction,//Ц±ПЯЛхЅш
                                                         ref double dHatchLoopDist,//»·јдѕа
                                                         ref int nEdgeLoop,//»·Кэ
                                                         ref bool nHatchLoopRev,//»·РО·ґЧЄ
                                                         ref bool bHatchAutoRotate,//КЗ·сЧФ¶ЇРэЧЄЅЗ¶И
                                                         ref double dHatchRotateAngle,
                                                         ref bool nHatchCross);



        [DllImport("MarkEzd", EntryPoint = "lmc1_SetHatchEntParam", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int
            SetHatchEntParam(string HatchName,
                                                        bool bEnableContour,
                                                        int nParamIndex,
                                                        int bEnableHatch,
                                                        int nPenNo,
                                                        int nHatchType,
                                                        bool bHatchAllCalc,
                                                        bool bHatchEdge,
                                                        bool bHatchAverageLine,
                                                        double dHatchAngle,
                                                        double dHatchLineDist,
                                                        double dHatchEdgeDist,
                                                        double dHatchStartOffset,
                                                        double dHatchEndOffset,
                                                        double dHatchLineReduction,//Ц±ПЯЛхЅш
                                                        double dHatchLoopDist,//»·јдѕа
                                                        int nEdgeLoop,//»·Кэ
                                                        bool nHatchLoopRev,//»·РО·ґЧЄ
                                                        bool bHatchAutoRotate,//КЗ·сЧФ¶ЇРэЧЄЅЗ¶И
                                                        double dHatchRotateAngle);

        [DllImport("MarkEzd", EntryPoint = "lmc1_GetHatchEntParam", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetHatchEntParam(string HatchName,
                         ref bool bEnableContour,
                         int nParamIndex,
                         ref int bEnableHatch,
                         ref int nPenNo,
                         ref int nHatchType,
                         ref bool bHatchAllCalc,
                         ref bool bHatchEdge,
                         ref bool bHatchAverageLine,
                         ref double dHatchAngle,
                         ref double dHatchLineDist,
                         ref double dHatchEdgeDist,
                         ref double dHatchStartOffset,
                         ref double dHatchEndOffset,
                         ref double dHatchLineReduction,//Ц±ПЯЛхЅш
                         ref double dHatchLoopDist,//»·јдѕа
                         ref int nEdgeLoop,//»·Кэ
                         ref bool nHatchLoopRev,//»·РО·ґЧЄ
                         ref bool bHatchAutoRotate,//КЗ·сЧФ¶ЇРэЧЄЅЗ¶И
                         ref double dHatchRotateAngle);

        [DllImport("MarkEzd", EntryPoint = "lmc1_SetHatchEntParam2", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetHatchEntParam2(string HatchName,
                            bool bEnableContour,
                            int nParamIndex,
                            int bEnableHatch,
                            bool bContourFirst,
                            int nPenNo,
                            int nHatchType,
                            bool bHatchAllCalc,
                            bool bHatchEdge,
                            bool bHatchAverageLine,
                            double dHatchAngle,
                            double dHatchLineDist,
                            double dHatchEdgeDist,
                            double dHatchStartOffset,
                            double dHatchEndOffset,
                            double dHatchLineReduction,//Ц±ПЯЛхЅш
                            double dHatchLoopDist,//»·јдѕа
                            int nEdgeLoop,//»·Кэ
                            bool nHatchLoopRev,//»·РО·ґЧЄ
                            bool bHatchAutoRotate,//КЗ·сЧФ¶ЇРэЧЄЅЗ¶И
                            double dHatchRotateAngle,
                            bool bHatchCrossMode,
                            int dCycCount);

        [DllImport("MarkEzd", EntryPoint = "lmc1_GetHatchEntParam2", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetHatchEntParam2(string HatchName,
                         ref bool bEnableContour,
                         int nParamIndex,
                         ref int bEnableHatch,
                         ref bool bContourFirst,
                         ref int nPenNo,
                         ref int nHatchType,
                         ref bool bHatchAllCalc,
                         ref bool bHatchEdge,
                         ref bool bHatchAverageLine,
                         ref double dHatchAngle,
                         ref double dHatchLineDist,
                         ref double dHatchEdgeDist,
                         ref double dHatchStartOffset,
                         ref double dHatchEndOffset,
                         ref double dHatchLineReduction,//Ц±ПЯЛхЅш
                         ref double dHatchLoopDist,//»·јдѕа
                         ref int nEdgeLoop,//»·Кэ
                         ref bool nHatchLoopRev,//»·РО·ґЧЄ
                         ref bool bHatchAutoRotate,//КЗ·сЧФ¶ЇРэЧЄЅЗ¶И
                         ref double dHatchRotateAngle,
                         ref bool bHatchCrossMode,
                         ref int dCycCount);


        /// <summary>
        /// ¶ФКэѕЭївЦРЦё¶ЁГыіЖµД¶ФПуМоід
        /// strEntName±»Моід¶ФПуГы      
        /// strHatchEntNameМоідєу¶ФПуГы
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_HatchEnt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int HatchEnt(string strEntName, string strHatchEntName);

        /// <summary>
        /// ¶ФКэѕЭївЦРЦё¶ЁГыіЖµД¶ФПуЙѕіэМоід
        /// strHatchEntNameМоід¶ФПуГы
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_UnHatchEnt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int UnHatchEnt(string strHatchEntName);

        #endregion

        #region МнјУЙѕіэ¶ФПу

        /// <summary>
        /// ЗеіэКэѕЭївАпЛщУР¶ФПу
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_ClearEntLib", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ClearLibAllEntity();

        ///<summary>
        /// ЙѕіэЦё¶ЁГыіЖ¶ФПу
        ///<summary>
        [DllImport("MarkEzd", EntryPoint = "lmc1_DeleteEnt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int DeleteEnt(string strEntName);

        /// <summary>
        /// ПтКэѕЭївАпМнјУОД±ѕ
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_AddTextToLib", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int AddTextToLib(string text, string EntName, double dPosX, double dPosY, double dPosZ, int nAlign, double dTextRotateAngle, int nPenNo, int bHatchText);

        [DllImport("MarkEzd", EntryPoint = "lmc1_AddCircleTextToLib", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int AddCircleTextToLib(string pStr,
                                                    string pEntName,
                                                    double dCenX,
                                                    double dCenY,
                                                    double dCenZ,
                                                    int nPenNo,
                                                    int bHatchText,
                                                    double dCirDiameter,
                                                    double dCirBaseAngle,
                                                    bool bCirEnableAngleLimit,
                                                    double dCirAngleLimit,
                                                    int nCirTextFlag);



        /// <summary>
        /// ПтКэѕЭївМнјУТ»МхЗъПЯ¶ФПу
        /// ЧўТвPtBuf±ШРлОЄ2О¬КэЧй,ЗТµЪТ»О¬ОЄ2,Из double[5,2],double[n,2],
        /// ptNumОЄPtBufКэЧйµДµЪ2О¬,ИзPtBufОЄdouble[5,2]КэЧй,ФтptNum=5
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_AddCurveToLib", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int AddCurveToLib([MarshalAs(UnmanagedType.LPArray)] double[,] PtBuf, int ptNum, string strEntName, int nPenNo);



        /// <summary>
        ///ФІ°лѕ¶
        ///ЗъПЯ¶ФПуГыіЖ
        ///ЗъПЯ¶ФПуК№УГµД±КєЕ
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_AddCircleToLib", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int lmc1_AddCircleToLib(double ptCenX, double ptCenY, double dRadius, string pEntName, int nPenNo);



        /// <summary>
        /// <summary>
        /// 
        /// ПтКэѕЭївМнјУТ»Чйµг¶ФПу
        /// ЧўТвPtBuf±ШРлОЄ2О¬КэЧй,ЗТµЪТ»О¬ОЄ2,Из double[5,2],double[n,2],
        /// ptNumОЄPtBufКэЧйµДµЪ2О¬,ИзPtBufОЄdouble[5,2]КэЧй,ФтptNum=5
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_AddPointToLib", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int AddPointToLib([MarshalAs(UnmanagedType.LPArray)] double[,] PtBuf, int ptNum, string strEntName, int nPenNo);

        /// <summary>
        /// МнјУСУК±ЖчµЅОДјюЦР
        /// </summary>
        /// <param name="dDelayMs">СУК±ЖчіЦРшК±јд</param>
        /// <returns></returns>
        [DllImport("MarkEzd", EntryPoint = "lmc1_AddDelayToLib", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int AddDelayToLib(double dDelayMs);

        /// <summary>
        /// МнјУКдіцїЪµЅОДјюЦР
        /// </summary>
        /// <param name="nOutPutBit">КдіцїЪ№ЬЅЕ</param>
        /// <param name="bHigh">КЗ·сёЯУРР§</param>
        /// <param name="bPulse">КЗ·сВціеДЈКЅ</param>
        /// <param name="dPulseTimeMs">ВціеіЦРшКАјд</param>
        /// <returns></returns>
        [DllImport("MarkEzd", EntryPoint = "lmc1_AddWritePortToLib", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int AddWritePortToLib(int nOutPutBit, bool bHigh, bool bPulse, double dPulseTimeMs);

        /// <summary>
        /// ФШИлЦё¶ЁКэѕЭОДјю
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_AddFileToLib", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int AddFileToLib(string strFileName, string strEntName, double dPosX, double dPosY, double dPosZ, int nAlign, double dRatio, int nPenNo, int bHatchFile);

        #region МхВл

        public enum BARCODETYPE
        {
            BARCODETYPE_39 = 0,
            BARCODETYPE_93 = 1,
            BARCODETYPE_128A = 2,
            BARCODETYPE_128B = 3,
            BARCODETYPE_128C = 4,
            BARCODETYPE_128OPT = 5,
            BARCODETYPE_EAN128A = 6,
            BARCODETYPE_EAN128B = 7,
            BARCODETYPE_EAN128C = 8,
            BARCODETYPE_EAN13 = 9,
            BARCODETYPE_EAN8 = 10,
            BARCODETYPE_UPCA = 11,
            BARCODETYPE_UPCE = 12,
            BARCODETYPE_25 = 13,
            BARCODETYPE_INTER25 = 14,
            BARCODETYPE_CODABAR = 15,
            BARCODETYPE_PDF417 = 16,
            BARCODETYPE_DATAMTX = 17,
            BARCODETYPE_USERDEF = 18,
            BARCODETYPE_QRCODE = 19,
            BARCODETYPE_MICROQRCODE = 20

        };

        public const ushort BARCODEATTRIB_CHECKNUM = 0x0004;//ЧФСйВл
        public const ushort BARCODEATTRIB_REVERSE = 0x0008;//·ґЧЄ
        public const ushort BARCODEATTRIB_SHORTMODE = 0x0040;//¶юО¬ВлЛх¶МДЈКЅ
        public const ushort BARCODEATTRIB_DATAMTX_DOTMODE = 0x0080;//¶юО¬ВлОЄµгДЈКЅ
        public const ushort BARCODEATTRIB_DATAMTX_CIRCLEMODE = 0x0100;//¶юО¬ВлОЄФІДЈКЅ
        public const ushort BARCODEATTRIB_DATAMTX_ENABLETILDE = 0x0200;//DataMatrixК№ДЬ~
        public const ushort BARCODEATTRIB_RECTMODE = 0x0400;//¶юО¬ВлОЄѕШРОДЈКЅ
        public const ushort BARCODEATTRIB_SHOWCHECKNUM = 0x0800;//ПФКѕРЈСйВлОДЧЦ
        public const ushort BARCODEATTRIB_HUMANREAD = 0x1000;//ПФКѕИЛК¶±рЧЦ·ы
        public const ushort BARCODEATTRIB_NOHATCHTEXT = 0x2000;//І»МоідЧЦ·ы
        public const ushort BARCODEATTRIB_BWREVERSE = 0x4000;//єЪ°Ч·ґЧЄ
        public const ushort BARCODEATTRIB_2DBIDIR = 0x8000;//2О¬ВлЛ«ПтЕЕБР

        public enum DATAMTX_SIZEMODE
        {
            DATAMTX_SIZEMODE_SMALLEST = 0,
            DATAMTX_SIZEMODE_10X10 = 1,
            DATAMTX_SIZEMODE_12X12 = 2,
            DATAMTX_SIZEMODE_14X14 = 3,
            DATAMTX_SIZEMODE_16X16 = 4,
            DATAMTX_SIZEMODE_18X18 = 5,
            DATAMTX_SIZEMODE_20X20 = 6,
            DATAMTX_SIZEMODE_22X22 = 7,
            DATAMTX_SIZEMODE_24X24 = 8,
            DATAMTX_SIZEMODE_26X26 = 9,
            DATAMTX_SIZEMODE_32X32 = 10,
            DATAMTX_SIZEMODE_36X36 = 11,
            DATAMTX_SIZEMODE_40X40 = 12,
            DATAMTX_SIZEMODE_44X44 = 13,
            DATAMTX_SIZEMODE_48X48 = 14,
            DATAMTX_SIZEMODE_52X52 = 15,
            DATAMTX_SIZEMODE_64X64 = 16,
            DATAMTX_SIZEMODE_72X72 = 17,
            DATAMTX_SIZEMODE_80X80 = 18,
            DATAMTX_SIZEMODE_88X88 = 19,
            DATAMTX_SIZEMODE_96X96 = 20,
            DATAMTX_SIZEMODE_104X104 = 21,
            DATAMTX_SIZEMODE_120X120 = 22,
            DATAMTX_SIZEMODE_132X132 = 23,
            DATAMTX_SIZEMODE_144X144 = 24,
            DATAMTX_SIZEMODE_8X18 = 25,
            DATAMTX_SIZEMODE_8X32 = 26,
            DATAMTX_SIZEMODE_12X26 = 27,
            DATAMTX_SIZEMODE_12X36 = 28,
            DATAMTX_SIZEMODE_16X36 = 29,
            DATAMTX_SIZEMODE_16X48 = 30,
        }

        /// <summary>
        /// ПтКэѕЭївАпМнјУМхВлОД±ѕ
        /// ЧўТв double[] dBarWidthScale єНdSpaceWidthScaleґуРЎ±ШРлОЄ4
        /// </summary> 

        [DllImport("MarkEzd", EntryPoint = "lmc1_AddBarCodeToLib", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int AddBarCodeToLib(string strText,
            string strEntName,
            double dPosX,
            double dPosY,
            double dPosZ,
            int nAlign,
            int nPenNo,
            int bHatchText,
            BARCODETYPE nBarcodeType,
            ushort wBarCodeAttrib,
            double dHeight,
            double dNarrowWidth,
            [MarshalAs(UnmanagedType.LPArray)] double[] dBarWidthScale,
            [MarshalAs(UnmanagedType.LPArray)] double[] dSpaceWidthScale,
              double dMidCharSpaceScale,
            double dQuietLeftScale,
            double dQuietMidScale,
            double dQuietRightScale,
            double dQuietTopScale,
            double dQuietBottomScale,
            int nRow,
            int nCol,
            int nCheckLevel,
           DATAMTX_SIZEMODE nSizeMode,
            double dTextHeight,
            double dTextWidth,
            double dTextOffsetX,
            double dTextOffsetY,
            double dTextSpace,
            double dDiameter,
            string TextFontName);


        [DllImport("MarkEzd", EntryPoint = "lmc1_GetBarcodeParam", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetBarcodeParam(string pEntName,
                                                    ref ushort wBarCodeAttrib,
                                                    ref int nSizeMode,
                                                    ref int nCheckLevel,
                                                    ref int nLangPage,
                                                    ref double dDiameter,
                                                    ref int nPointTimesN,
                                                    ref double dBiDirOffset);

        [DllImport("MarkEzd", EntryPoint = "lmc1_SetBarcodeParam", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetBarcodeParam(string pEntName,
                                                    ushort wBarCodeAttrib,
                                                    int nSizeMode,
                                                    int nCheckLevel,
                                                    int nLangPage,
                                                    double dDiameter,
                                                    int nPointTimesN,
                                                    double dBiDirOffset);


        #endregion

        #endregion

        #region А©Х№Цб

        /// <summary>
        /// ёґО»Ј¬К№ДЬА©Х№Цб
        /// ***К№УГА©Х№ЦбЦ®З°±ШРлК№УГПИµчУГґЛєЇКэАґіхКј»ЇА©Х№Цб*******
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_Reset", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ResetAxis(bool bEnAxis1, bool bEnAxis2);

        /// <summary>
        /// А©Х№ЦбТЖ¶ЇµЅДї±кО»ЦГ
        /// axis=0»т1
        /// GoalPosДї±кО»ЦГ,µҐО»mm»тinch
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_AxisMoveTo", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int AxisMoveTo(int axis, double GoalPos);

        /// <summary>
        /// А©Х№Цб»ШФ­µг(РЈХэФ­µг)
        /// axis=0»т1
        /// GoalPosДї±кО»ЦГ
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_AxisCorrectOrigin", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int AxisGoHome(int axis);

        /// <summary>
        /// µГµЅА©Х№ЦбµДµ±З°Чш±к
        /// axis=0»т1
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_GetAxisCoor", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern double GetAxisCoor(int axis);

        /// <summary>
        /// А©Х№ЦбТЖ¶ЇµЅВціеДї±кО»ЦГ
        /// axis=0»т1
        /// nGoalPosДї±кО»ЦГ,µҐО»:Вціе
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_AxisMoveToPulse", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int AxisMoveToPulse(int axis, int nGoalPos);

        /// <summary>
        /// µГµЅА©Х№ЦбµДµ±З°ВціеЧш±к
        /// axis=0»т1
        /// </summary> 
        [DllImport("MarkEzd", EntryPoint = "lmc1_GetAxisCoorPulse", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetAxisCoorPulse(int axis);

        #endregion

        #region УІјюЛшґж

        [DllImport("MarkEzd", EntryPoint = "lmc1_EnableLockInputPort", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int EnableLockInputPort(bool bLowToHigh);

        [DllImport("MarkEzd", EntryPoint = "lmc1_ClearLockInputPort", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ClearLockInputPort();

        [DllImport("MarkEzd", EntryPoint = "lmc1_ReadLockPort", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ReadLockPort(ref ushort data);
        #endregion

    }

}
