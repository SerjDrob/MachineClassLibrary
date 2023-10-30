namespace MachineClassLibrary.Machine
{
    public class LaserMachineConfiguration:BoardConfiguration
    {
        private const string UF = "UF";
        private const string IR = "IR";
        private const string LASERMOCK = "LASERMOCK";
        
        public string MarkDevTypeNote => $"Choose from following types: {UF}, {IR}, {LASERMOCK}";
        public string MarkDevType
        {
            get; set;
        }
        
        public bool IsUF => MarkDevType == UF;
        public bool IsIR => MarkDevType == IR;
        public bool IsLaserMock => MarkDevType == LASERMOCK;

    }
}
