using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace truyenchu.Models {
    [Table(nameof(Chapter))]
    public class Chapter {
        [Key]
        public int ChapterId { get; set; }

        [DisplayName("Chương")]
        public int Order {get;set;}

        [DisplayName("Tiêu đề chương")]
        [Required(ErrorMessage = "Phải nhập {0}")] 
        [StringLength(255, ErrorMessage = "{0} tối đa {2} kí tự")]
        [Column(TypeName = "nvarchar")]
        public string Title { get; set; }

        [DisplayName("Nội dung")]
        public string? Content { get; set; }

        [DisplayName("Ngày đăng")]
        public DateTime? DateCreated { get; set; }
        
        [DisplayName("Truyện")]
        public int? StoryId { get; set; }

        [DisplayName("Truyện")]
        [ForeignKey(nameof(StoryId))]
        public Story? Story { get; set; }
    }
}