namespace MachineClassLibrary.Machine
{
    public class DicingMachineConfiguration:BoardConfiguration
    {
        private const string EM225 = "EM225";
        private const string O4PP100 = "O4PP100";
        private const string DR150 = "DR150";

        public string DicingDevTypeNote => $"Choose from following types: {EM225}, {O4PP100}, {DR150}";
        public string DicingDevType
        {
            get; set;
        }
        public bool CameraEnable
        {
            get; set;
        }
        public bool IsEM225 => DicingDevType == EM225;
        public bool IsO4PP100 => DicingDevType == O4PP100;
        public bool IsDR150 => DicingDevType == DR150;

    }
}
