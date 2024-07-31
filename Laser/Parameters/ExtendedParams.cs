using System;
using System.ComponentModel;

namespace MachineClassLibrary.Laser.Parameters
{
    public class ExtendedParams : ICloneable
    {
        /// <summary>
        /// mark times
        /// </summary>
        [Category("Луч")]
        [DisplayName("Количество проходов")]
        public int MarkLoop
        {
            get; set;
        }
        /// <summary>
        /// speed of marking mm/s
        /// </summary>
        [Category("Луч")]
        [DisplayName("Скорость, мм/с")]
        public double MarkSpeed
        {
            get; set;
        }
        /// <summary>
        /// power ratio of laser (0-100%)
        /// </summary>
        [Category("Луч")]
        [DisplayName("Мощность, %")]
        public int PowerRatio
        {
            get; set;
        }
        /// <summary>
        /// frequency of laser HZ
        /// </summary>
        [Category("Луч")]
        [DisplayName("Частота, Гц")]
        public int Freq
        {
            get; set;
        }
        /// <summary>
        /// width of Q pulse (us)
        /// </summary>
        [Category("Луч")]
        [DisplayName("Ширина импульса, мкс")]
        public int QPulseWidth { get; set; } = 1;
        [Category("Модуляция ШИМ")]
        [DisplayName("Применить модуляцию")]
        public bool EnablePWM
        {
            get; set;
        }
        /// <summary>
        /// Hz
        /// </summary>
        [Category("Модуляция ШИМ")]
        [DisplayName("Частота модуляции, Гц")]
        public int PWMFrequency
        {
            get; set;
        }
        /// <summary>
        /// percentage < 100%
        /// </summary>
        [Category("Модуляция ШИМ")]
        [DisplayName("Скважность, %")]
        public int PWMDutyCycle
        {
            get; set;
        }
        [Category("Штриховка / фрезеровка")]
        [DisplayName("Штриховать")]
        public bool EnableHatch
        {
            get; set;
        }
        [Category("Штриховка / фрезеровка")]
        [DisplayName("Фрезеровать")]
        public bool EnableMilling
        {
            get;
            set;
        } = true;
        [Category("Штриховка / фрезеровка")]
        [DisplayName("Не проходить контур")]
        public bool EnableContour
        {
            get;
            set;
        }
        /// <summary>
        /// um
        /// </summary>
        [Category("Штриховка, мкм")]
        [DisplayName("Ширина контура")]
        [Browsable(false)]
        public int HatchWidth
        {
            get; set;
        }
        /// <summary>
        /// um
        /// </summary>
        [Category("Штриховка / фрезеровка")]
        [DisplayName("Смещение контура, мкм")]
        public int ContourOffset
        {
            get;
            set;
        }
        /// <summary>
        /// um
        /// </summary>
        [Category("Штриховка / фрезеровка")]
        [DisplayName("Шаг")]
        public int HatchLineDistance
        {
            get; set;
        }

        public int HatchAttribute
        {
            get;
            set;
        }
        public int HatchEdgeDist
        {
            get;
            set;
        }
        private bool _hatchContourFirst;
        public bool HatchContourFirst
        {
            get => _hatchContourFirst & EnableContour;
            set => _hatchContourFirst = value;
        }
        public double HatchAngle { get; set; }
        public bool HatchAutoRotate { get; set; }
        public double HatchRotateAngle { get; set; }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
