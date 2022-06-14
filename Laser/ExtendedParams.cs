﻿using System.ComponentModel;

namespace MachineClassLibrary.Laser
{
    public class ExtendedParams
    {
        /// <summary>
        /// mark times
        /// </summary>
        [Category("Луч")]
        [DisplayName("Количество проходов")]
        public int MarkLoop { get; set; }
        /// <summary>
        /// speed of marking mm/s
        /// </summary>
        [Category("Луч")]
        [DisplayName("Скорость, мм/с")]
        public double MarkSpeed { get; set; }
        /// <summary>
        /// power ratio of laser (0-100%)
        /// </summary>
        [Category("Луч")]
        [DisplayName("Мощность, %")]
        public int PowerRatio { get; set; }
        /// <summary>
        /// frequency of laser HZ
        /// </summary>
        [Category("Луч")]
        [DisplayName("Частота, Гц")]
        public int Freq { get; set; }
        /// <summary>
        /// width of Q pulse (us)
        /// </summary>
        [Category("Луч")]
        [DisplayName("Ширина импульса, мкс")]
        public int QPulseWidth { get; set; }
        [Category("Модуляция ШИМ")]
        [DisplayName("Применить модуляцию")]
        public bool EnablePWM { get; set; }
        /// <summary>
        /// Hz
        /// </summary>
        [Category("Модуляция ШИМ")]
        [DisplayName("Частота модуляции, Гц")]
        public int PWMFrequency { get; set; }
        /// <summary>
        /// percentage < 100%
        /// </summary>
        [Category("Модуляция ШИМ")]
        [DisplayName("Скважность, %")]
        public int PWMDutyCycle { get; set; }
        [Category("Штриховка")]
        [DisplayName("Штриховать")]
        public bool EnableHatch { get; set; }
        /// <summary>
        /// um
        /// </summary>
        [Category("Штриховка, мкм")]
        [DisplayName("Ширина контура")]
        public int HatchWidth { get; set; }
        /// <summary>
        /// um
        /// </summary>
        [Category("Штриховка, мкм")]
        [DisplayName("Шаг")]
        public int HatchLineDistance { get; set; }
    }

    public class ExtParamsAdapter
    {
        private readonly ExtendedParams _extendedParams;

        public ExtParamsAdapter(ExtendedParams extendedParams)
        {
            _extendedParams = extendedParams;
        }
        public MarkLaserParams MixParams(MarkLaserParams markLaserParams)
        {
            var penParams = markLaserParams.PenParams with
            {
                MarkLoop = _extendedParams.MarkLoop,
                MarkSpeed = _extendedParams.MarkSpeed,
                PowerRatio = _extendedParams.PowerRatio,
                Freq = _extendedParams.Freq,
                QPulseWidth = _extendedParams.QPulseWidth
            };
            var hatchParams = markLaserParams.HatchParams with
            {
                HatchLineDist = _extendedParams.HatchLineDistance
            };

            return new MarkLaserParams(penParams, hatchParams);
        }
    }
}