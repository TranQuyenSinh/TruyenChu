using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Identity.Client;
using truyenchu.Data;

namespace truyenchu.Models
{
    [Table("Category")]
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [DisplayName("Tên thể loại")]
        [Required(ErrorMessage = "Phải nhập {0}")]
        [Column(TypeName = "nvarchar")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "{0} phải từ {2} đến {1} kí tự")]
        public string CategoryName { get; set; }

        [DisplayName("Mô tả")]
        public string? Description { get; set; }

        [StringLength(255)]
        public string CategorySlug { get; set; }

        public IEnumerable<StoryCategory>? StoryCategory { get; set; }


    }
}