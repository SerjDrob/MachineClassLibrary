﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MachineClassLibrary.Laser.Parameters;

namespace MachineClassLibrary.Laser.Markers
{
    public class MockLaser : IMarkLaser
    {
        public bool IsMarkDeviceInit { get; private set; }

        public Task<bool> CancelMarkingAsync()
        {
            Debug.WriteLine("Marking has been canceled.");
            return Task.FromResult(true);
        }

        public Task<bool> ChangePWMBaudRateReinitMarkDevice(int baudRate, string initDirPath)
        {
            throw new NotImplementedException();
        }

        public void CloseMarkDevice()
        {
            IsMarkDeviceInit = false;
        }

        public Task<bool> InitMarkDevice(string initDirPath)
        {
            IsMarkDeviceInit = true;
            return Task.FromResult(IsMarkDeviceInit);
        }

        public Task<(double x, double y)[]> MarkCircleArrayAsync(double diameter, double arrayWidth, int arraySize)
        {
            throw new NotImplementedException();
        }

        public Task<bool> MarkTextAsync(string text, double textSize, double angle)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PierceCircleAsync(double diameter)
        {
            Debug.WriteLine($"Laser is piercing circle d = {diameter}");
            return Task.FromResult(true);
        }

        public Task<bool> PierceDxfObjectAsync(string filePath)
        {
            return Task.FromResult(true); // throw new NotImplementedException();
        }

        public Task<bool> PierceLineAsync(double x1, double y1, double x2, double y2)
        {
            Debug.WriteLine($"Laser is piercing line ({x1},{y1}),({x2},{y2})");
            return Task.FromResult(true);
        }

        public Task<bool> PierceObjectAsync(IPerforating perforator)
        {
            Debug.WriteLine($"Laser is piercing object {perforator.GetType().Name}");
            return Task.FromResult(true);
        }

        public Task<bool> PiercePointAsync(double x = 0, double y = 0)
        {
            Debug.WriteLine($"Laser is piercing point ({x},{y})");
            return Task.FromResult(true);
        }

        public bool SetDevConfig()
        {
            //throw new NotImplementedException();
            return true;
        }

        public void SetExtMarkParams(ExtParamsAdapter paramsAdapter)
        {
            //throw new NotImplementedException();
        }

        public void SetMarkDeviceParams()
        {
            Debug.WriteLine($"Device params is set");
        }

        public void SetMarkParams(MarkLaserParams markLaserParams)
        {
            //throw new NotImplementedException();
        }

        public void SetSystemAngle(double angle)
        {
            throw new NotImplementedException();
        }

        public Task<bool> StopPWMAsync()
        {
            throw new NotImplementedException();
        }
    }
}
