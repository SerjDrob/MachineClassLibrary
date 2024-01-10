namespace MachineClassLibrary.Machine
{
    public abstract class BoardConfiguration
    {
        private const string PCI1240U = "PCI1240U";
        private const string PCI1245E = "PCI1245E";
        private const string MOCKBOARD = "MOCKBOARD";
        public string MotionBoardNote => $"Choose from following boards: {PCI1240U}, {PCI1245E}, {MOCKBOARD}";
        public string MotionBoard
        {
            get; set;
        }
        public double AxesTolerance { get; set; } = 0.001;
        public bool IsPCI1240U => MotionBoard == PCI1240U;
        public bool IsPCI1245E => MotionBoard == PCI1245E;
        public bool IsMOCKBOARD => MotionBoard == MOCKBOARD;
    }
}
