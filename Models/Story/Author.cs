using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace truyenchu.Models {
    [Table("Author")]
    public class Author
    {
        [Key]
        public int AuthorId { get; set; }

        [DisplayName("Tên tác giả")]
        [Required(ErrorMessage = "Phải nhập {0}")]
        [Column(TypeName = "nvarchar")]
        [StringLength(255, ErrorMessage ="{0} tối đa {1} kí tự")]
        public string AuthorName { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(255)]
        public string AuthorSlug {get;set;}
    }
}