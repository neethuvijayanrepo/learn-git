namespace TestProject.Business.Helpers
{
    using AutoMapper;
    using BusinessObjects.Security;
    using Data.DataObjects;

    /// <summary>
    /// Mapping profile for Business objects and Data objects.
    /// </summary>
    public class BusinessDataObjectMappingProfile : Profile
    {
        /// <summary>
        /// Mapping profile Constructor.
        /// </summary>
        public BusinessDataObjectMappingProfile()
        {
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<usp_Security_Login_Result, LoginDto>().ReverseMap();

        }
    }
}
