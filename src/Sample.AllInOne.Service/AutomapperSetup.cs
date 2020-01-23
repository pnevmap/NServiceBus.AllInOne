using AutoMapper;
using Sample.AllInOne.Service.Controllers;
using Sample.AllInOne.Service.DataAccess;

namespace Sample.AllInOne.Service
{
    public class AutomapperSetup : Profile
    {
        public AutomapperSetup()
        {
            CreateMap<ValueEntity, string>().ConvertUsing(s => s.Value);

        }
    }
}