﻿using AutoMapper;

namespace MachineClassLibrary.Laser.Parameters
{
    public class ParamsProfile : Profile
    {
        public ParamsProfile()
        {
            CreateMap<MarkLaserParams, ExtendedParams>()
                .IncludeMembers(s => s.PenParams, s => s.HatchParams);

            CreateMap<PenParams, ExtendedParams>(MemberList.None);
            CreateMap<HatchParams, ExtendedParams>(MemberList.None)
                .ForMember(ext=>ext.HatchLineDistance, opt => opt.MapFrom(h=> (int)(h.HatchLineDist * 1000))) ;
        }
    }
}