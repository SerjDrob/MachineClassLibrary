using System.Collections.Generic;

namespace MachineClassLibrary.VideoCapture;

public interface ICameraPropertyControl
{
    IReadOnlyCollection<CameraProperty> SupportedProperties { get; }

    bool TrySet(CameraProperty property, double value);

    bool TryGet(CameraProperty property, out double value);
}

