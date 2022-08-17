namespace MachineClassLibrary.Laser.Entities
{
    public static class LaserEntTypeAdapter
    {
        public static LaserEntity GetLaserEntity(this IProcObject procObject)
        {
            return procObject switch
            {
                PCircle => LaserEntity.Circle,
                PCurve or PDxfCurve or PDxfCurve2 => LaserEntity.Curve,
                PLine => LaserEntity.Line,
                PPoint => LaserEntity.Point,
                _ => LaserEntity.None
            };
        }
    }
}
