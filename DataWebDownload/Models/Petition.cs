using System.ComponentModel.DataAnnotations;

namespace DataWebDownload.Models
{
    public class Petition
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