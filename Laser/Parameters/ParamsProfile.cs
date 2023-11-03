using AutoMapper;

namespace MachineClassLibrary.Laser.Parameters
{
    public class ParamsProfile : Profile
    {
        public ParamsProfile()
        {
            CreateMap<MarkLaserParams, ExtendedParams>()
                .IncludeMembers(s => s.PenParams, s => s.HatchParams);

            CreateMap<PenParams, ExtendedParams>(MemberList.None)
                .ForMember(ext=>ext.EnablePWM,
                opt=>opt.MapFrom(h=>h.IsModulated))
                .ForMember(ext=>ext.PWMFrequency,
                opt=>opt.MapFrom(h=>h.ModFreq))
                .ForMember(ext=>ext.PWMDutyCycle,
                opt=>opt.MapFrom(h=>h.ModDutyCycle));
            
            CreateMap<HatchParams, ExtendedParams>(MemberList.None)
                .ForMember(ext=>ext.HatchLineDistance, 
                opt => opt.MapFrom(h=> (int)(h.HatchLineDist * 1000)))
                .ForMember(ext=>ext.HatchEdgeDist,
                opt=>opt.MapFrom(h=>(int)(h.HatchEdgeDist * 1000)));
        }
    }
}