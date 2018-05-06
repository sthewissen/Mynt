using System;
using AutoMapper;
using Mynt.Core.Models;

namespace Mynt.Data.SqlServer
{
    public static class Mapping
    {
        private static readonly Lazy<IMapper> Lazy = new Lazy<IMapper>(() =>
        {
            var config = new MapperConfiguration(cfg =>
            {
                // This line ensures that internal properties are also mapped over.
                cfg.ShouldMapProperty = p => p.GetMethod.IsPublic || p.GetMethod.IsAssembly;
                cfg.AddProfile<MappingProfile>();
            });

            var mapper = config.CreateMapper();
            return mapper;
        });

        public static IMapper Mapper => Lazy.Value;
    }

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TradeAdapter, Trade>();

            CreateMap<Trade, TradeAdapter>();

            CreateMap<TraderAdapter, Trader>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TraderId));

            CreateMap<Trader, TraderAdapter>()
                .ForMember(dest => dest.TraderId, opt => opt.MapFrom(src => src.Id));
        }
    }
}
