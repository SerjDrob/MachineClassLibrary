namespace NewLaserProject.Classes
{
    public class MachineConfiguration
    {
        private const string PCI1240U = "PCI1240U";
        private const string PCI1245E = "PCI1245E";
        private const string MOCKBOARD = "MOCKBOARD";

        private const string UF = "UF";
        private const string IR = "IR";
        private const string LASERMOCK = "LASERMOCK";

        public string MotionBoardNote => $"Choose from following boards: {PCI1240U}, {PCI1245E}, {MOCKBOARD}";
        public string MotionBoard { get; set; }
        public string MarkDevTypeNote => $"Choose from following types: {UF}, {IR}, {LASERMOCK}";
        public string MarkDevType { get; set; }
        public bool IsPCI1240U => MotionBoard == PCI1240U;
        public bool IsPCI1245E => MotionBoard == PCI1245E;
        public bool IsMOCKBOARD => MotionBoard == MOCKBOARD;
        public bool IsUF => MarkDevType == UF;
        public bool IsIR => MarkDevType == IR;
        public bool IsLaserMock => MarkDevType == LASERMOCK;

    }
}
