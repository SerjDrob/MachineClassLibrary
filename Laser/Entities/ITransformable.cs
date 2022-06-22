namespace MachineClassLibrary.Laser.Entities
{
    public interface ITransformable
    {
        void MirrorX();
        void MirrorY();
        void OffsetX(float offset);
        void OffsetY(float offset);
        void Scale(float scale);
        void Turn90();
    }
}