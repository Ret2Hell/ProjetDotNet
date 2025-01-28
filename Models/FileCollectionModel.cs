using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetDotNet.Models
{

    public class FileCollectionModel
    {
        [Key] 
        public int Id { get; set; }

        [Required] 
        [MaxLength(255)] 
        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<FileModel> Files { get; set; }

        public ICollection<FileCollectionModel> SubCollections { get; set; }

        public int? ParentCollectionId { get; set; }

        [ForeignKey("ParentCollectionId")] 
        public FileCollectionModel ParentCollection { get; set; }
    }
}