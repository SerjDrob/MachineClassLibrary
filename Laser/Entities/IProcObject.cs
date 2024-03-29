﻿using System;

namespace MachineClassLibrary.Laser.Entities
{
    public interface IProcObject<TObject> : IProcObject where TObject : IShape
    {
        public TObject PObject { get; init; }
        public new IProcObject<TObject> CloneWithPosition(double x, double y);
    }
    public interface IProcObject
    {
        public Guid Id { get; }
        public bool IsBeingProcessed { get; set; }
        public bool IsProcessed { get; set; }
        public bool ToProcess { get; set; }
        public int ARGBColor { get; set; }
        public string LayerName { get; set; }
        public double X { get; init; }
        public double Y { get; init; }
        public double Angle { get; init; }
        public (double x, double y) GetSize();
        public double Scaling { get; }
        public bool MirrorX { get; }
        public bool Turn90 { get; }
        void Scale(double scale);
        void SetMirrorX(bool mirror);
        void SetTurn90(bool turn);
        public IProcObject CloneWithPosition(double x, double y);
    }   
}
