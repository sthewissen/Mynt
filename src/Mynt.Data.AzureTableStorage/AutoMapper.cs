using System;
using AutoMapper;
using Mynt.Core.Models;

namespace Mynt.Data.AzureTableStorage
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
            
            CreateMap<Trade, TradeAdapter>()
                .ForMember(dest => dest.RowKey, opt => opt.MapFrom(src => src.OpenDate.ToString("yyyyMMddHHmmss")))
                .ForMember(dest => dest.PartitionKey, opt => opt.MapFrom(src => src.Market));

            CreateMap<TraderAdapter, Trader>()
                .ForMember(dest => dest.Identifier, opt => opt.MapFrom(src => src.RowKey));

            CreateMap<Trader, TraderAdapter>()
                .ForMember(dest => dest.RowKey, opt => opt.MapFrom(src => src.Identifier))
                .ForMember(dest => dest.PartitionKey, opt => opt.MapFrom(src => "TRADER"));
        }
    }
}
