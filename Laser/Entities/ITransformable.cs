namespace MachineClassLibrary.Laser.Entities
{
    public interface ITransformable
    {
        void MirrorX();
        void MirrorY();
        void OffsetX(float offset);
        void OffsetY(float offset);
        void Scale(float scale);
        void SetRestrictingArea(double x, double y, double width, double height);
        void Turn90();
    }
}