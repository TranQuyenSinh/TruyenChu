using System.ComponentModel.DataAnnotations;
using truyenchu.Models;

namespace truyenchu.Areas.Admin.Model
{
    public class CreateViewModel : Story
    {
        public CreateViewModel() { }
        public CreateViewModel(Story story, int[]? categoryIds = null) : base(story)
        {
            CategoryIds = categoryIds;
        }
        [Display(Name = "Các thể loại")]
        [Required(ErrorMessage = "Phải chọn ít nhất một thể loại")]
        public int[]? CategoryIds { get; set; }
    }
}