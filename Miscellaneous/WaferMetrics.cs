using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MachineClassLibrary.Laser.Entities;

namespace MachineClassLibrary.Miscellaneous;
internal class WaferMetrics
{
}
internal static class ProcObjectExtensions
{
    
}


public class ProcObjectByPObjEqComparer : EqualityComparer<IProcObject>
{
    public override bool Equals(IProcObject x, IProcObject y)
    {
        if (x == null || y==null || x.GetType() != y.GetType())
        {
            return false;
        }

        return x switch
        {
            PCircle circle1 when y is PCircle circle2 => circle1.PObject == circle2.PObject,
            PCurve curve1 when y is PCurve curve2 => curve1.PObject == curve2.PObject,
            _ => false
        };
    }
    public override int GetHashCode([DisallowNull] IProcObject obj) => GetHashCode();
}
