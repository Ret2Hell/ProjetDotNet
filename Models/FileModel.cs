using System.ComponentModel.DataAnnotations;

namespace ProjetDotNet.Models
{
    public class FileModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public string FilePath { get; set; }

        public string UploadedBy { get; set; }
        
        public DateTime UploadedOn { get; set; } = DateTime.Now;
    }
}