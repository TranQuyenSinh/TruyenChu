using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.Identity.Client;

namespace truyenchu.Models {
    [Table("Author")]
    public class Author
    {
        [Key]
        public int AuthorId { get; set; }

        [Display(Name = "Tên tác giả", Prompt = "Tên tác giả")]
        [Required(ErrorMessage = "Phải nhập {0}")]
        [Column(TypeName = "nvarchar")]
        [StringLength(255, ErrorMessage ="{0} tối đa {1} kí tự")]
        public string AuthorName { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(255)]
        public string AuthorSlug {get;set;}

        public DateTime DateCreated { get; set; }

        public DateTime DateUpdated { get; set;}
    }
}