namespace MachineClassLibrary.Laser.Entities
{
    public static class LaserEntDxfTypeAdapter
    {
        const string CURVE = "LWPOLYLINE";
        const string CIRCLE = "CIRCLE";
        const string LINE = "LINE";
        const string POINT = "POINT";


        public static LaserEntity GetLaserEntity(string dxfEntName)
        {
            return dxfEntName switch
            {
                CURVE => LaserEntity.Curve,
                CIRCLE => LaserEntity.Circle,
                LINE => LaserEntity.Line,
                POINT => LaserEntity.Point,
                _ => LaserEntity.None
            };
        }
    }
}
