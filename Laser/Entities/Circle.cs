﻿using System.Windows;

namespace MachineClassLibrary.Laser.Entities
{
    public class Circle:IShape
    {
        public double Radius { get; set; }
        public double CenterX
        {
            get;
            set;
        }
        public double CenterY
        {
            get;
            set;
        }
        public Rect Bounds { get; init; }
    }
}
