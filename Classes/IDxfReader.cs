using MachineClassLibrary.Laser.Entities;
using System.Collections.Generic;

namespace MachineClassLibrary.Classes
{
    public interface IDxfReader
    {
        IEnumerable<PCircle> GetCircles();
        IEnumerable<PLine> GetLines();
        IDictionary<string, int> GetLayers();
        IEnumerable<PLine> GetAllSegments();
    }
}