namespace MachineClassLibrary.BehaviourTree
{
    public class Block
    {
        public Block BlockMe()
        {
            NotBlocked = false;
            return this;            
        }
        public Block UnBlockMe()
        {
            NotBlocked = true;
            return this;
        }
        public bool NotBlocked { get; private set; } = true;
    }
}
