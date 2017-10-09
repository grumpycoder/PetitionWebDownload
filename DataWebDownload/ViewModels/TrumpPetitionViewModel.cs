using AutoMapper;
using DataWebDownload.Models;
using Heroic.AutoMapper;

namespace DataWebDownload.ViewModels
{
    public class TrumpPetitionViewModel : IMapFrom<Petition>, IHaveCustomMappings
    {
        public string RecordId { get; set; }
        public string Created { get; set; }
        public string field_email { get; set; }
        public string field_2_first_name { get; set; }
        public string field_3_last_name { get; set; }
        public string field_zip_code { get; set; }

        public void CreateMappings(IMapperConfiguration cfg)
        {
            cfg.CreateMap<TrumpPetitionViewModel, Petition>()
                .ForMember(d => d.Email, src => src.MapFrom(s => s.field_email))
                .ForMember(d => d.Firstname, src => src.MapFrom(s => s.field_2_first_name))
                .ForMember(d => d.Lastname, src => src.MapFrom(s => s.field_3_last_name))
                .ForMember(d => d.Zipcode, src => src.MapFrom(s => s.field_zip_code))
                .ReverseMap();
        }
    }
}