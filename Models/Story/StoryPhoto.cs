using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace truyenchu.Models {
    [Table("StoryPhoto")]
    public class StoryPhoto {
        [Key]
        public int Id { get; set;}

        public string FileName { get; set;} 
    }
}