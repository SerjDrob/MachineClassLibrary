namespace MachineClassLibrary.Machine
{
    public class DicingMachineConfiguration:BoardConfiguration
    {
        private const string EM225 = "EM225";
        private const string O4PP100 = "O4PP100";
        private const string DR150 = "DR150";
        
        private const string Spindle3 = "Spindle3";
        private const string MD520 = "MD520";
        private const string CommanderSK = "CommanderSK";
        private const string MOCKSPINDLE = "MOCKSPINDLE";

        public string DicingDevTypeNote => $"Choose from following types: {EM225}, {O4PP100}, {DR150}";
        public string DicingDevType { get; set; }

        public string SpindleDevTypeNote => $"Choose from following types: {Spindle3}, {MD520}, {CommanderSK}, {MOCKSPINDLE}";
        public string SpindleDevType { get;set; }


        public bool CameraEnable { get; set; }
        public bool IsEM225 => DicingDevType == EM225;
        public bool IsO4PP100 => DicingDevType == O4PP100;
        public bool IsDR150 => DicingDevType == DR150;
        
        public bool IsSpindle3 => SpindleDevType == Spindle3;
        public bool IsMD520 => SpindleDevType == MD520;
        public bool IsCommanderSK => SpindleDevType == CommanderSK;
        public bool IsMockSpindle => SpindleDevType == MOCKSPINDLE;
        public string SpindlePort { get; set; } = "COM1";
        public int SpindleBaudRate { get; set; } = 19200;
        public string AxesOrder { get; set; }
    }
}
