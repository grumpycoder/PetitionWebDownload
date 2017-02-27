using AutoMapper;
using Heroic.AutoMapper;
using System.ComponentModel.DataAnnotations;

namespace DataWebDownload
{
    public class PersonViewModel : IMapFrom<Person>, IHaveCustomMappings
    {
        public string RecordId { get; set; }
        public string Created { get; set; }
        public string field_email { get; set; }
        public string field_2_first_name { get; set; }
        public string field_3_last_name { get; set; }
        public string field_zip_code { get; set; }

        public void CreateMappings(IMapperConfiguration cfg)
        {
            cfg.CreateMap<PersonViewModel, Person>()
                .ForMember(d => d.Email, src => src.MapFrom(s => s.field_email))
                .ForMember(d => d.Firstname, src => src.MapFrom(s => s.field_2_first_name))
                .ForMember(d => d.Lastname, src => src.MapFrom(s => s.field_3_last_name))
                .ForMember(d => d.Zipcode, src => src.MapFrom(s => s.field_zip_code))
                .ReverseMap();

            //cfg.CreateMap<Person, PersonViewModel>()
            //   .ForMember(d => d.field_email, src => src.MapFrom(s => s.Email))
            //   .ForMember(d => d.field_2_first_name, src => src.MapFrom(s => s.Firstname))
            //   .ForMember(d => d.field_3_last_name, src => src.MapFrom(s => s.Lastname))
            //   .ForMember(d => d.field_zip_code, src => src.MapFrom(s => s.Zipcode))
            //   ;
        }
    }

    public class Person
    {
        [Key]
        public int Id { get; set; }
        public string RecordId { get; set; }
        public string Created { get; set; }
        public string Email { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Zipcode { get; set; }
        public string Discriminator { get; set; }
    }

}