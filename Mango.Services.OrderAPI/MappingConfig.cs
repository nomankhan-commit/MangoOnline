﻿using AutoMapper;


namespace Mango.Services.OrderAPI
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps() 
        {
            var mappingConfig = new MapperConfiguration(config => 
            { 
                //config.CreateMap<CartHeader, CartHeaderDto>().ReverseMap();
                //config.CreateMap<CartDetails, CartDetailsDto>().ReverseMap(); 
            });
            return mappingConfig; 
        }
    }
}
