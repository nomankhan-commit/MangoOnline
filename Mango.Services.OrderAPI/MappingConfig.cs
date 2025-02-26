﻿using AutoMapper;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Models.Dto;


namespace Mango.Services.OrderAPI
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps() 
        {
            var mappingConfig = new MapperConfiguration(config => 
            { 
                config.CreateMap<OrderHeaderDto, CartHeaderDto>()
                .ForMember(dest=>dest.CartTotal,u=>u.MapFrom(src=>src.OrderTotal)).ReverseMap();
                
                config.CreateMap<CartDetailsDto, OrderDetailsDto>()
                  .ForMember(dest => dest.ProductName, u => u.MapFrom(src => src.ProductDto.ProductName))
                  .ForMember(dest => dest.ProductPrice, u => u.MapFrom(src => src.ProductDto.Price));

                config.CreateMap<OrderDetailsDto, CartDetailsDto>();
                config.CreateMap<OrderHeader, OrderHeaderDto>().ReverseMap();
                config.CreateMap<OrderDetailsDto, OrderDetails>().ReverseMap();


 
            });
            return mappingConfig; 
        }
    }
}
