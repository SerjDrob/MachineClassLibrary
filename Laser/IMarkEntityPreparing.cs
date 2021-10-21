using MachineClassLibrary.Laser.Entities;

namespace MachineClassLibrary.Laser
{
    public interface IMarkEntityPreparing<TObject>
    {
        /// <summary>
        /// Prepares given entity for marking
        /// </summary>
        /// <param name="procObject">given entity</param>
        /// <returns>Name of entity to pierce</returns>

        public string EntityPreparing(IProcObject<TObject> procObject);
    }
}
