using AutoMapper;


namespace Mango.Services.EmailAPI
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps() 
        {
            var mappingConfig = new MapperConfiguration(config => 
            { 
                
            });
            return mappingConfig; 
        }
    }
}
