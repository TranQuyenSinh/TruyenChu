using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace truyenchu.Models
{
    [Table(nameof(Story))]
    public class Story
    {
        public Story() {}
        public Story(Story story)
        {
            StoryId = story.StoryId;
            StoryName = story.StoryName;
            AuthorId = story.AuthorId;
            Author = story.Author;
            Description = story.Description;
            StorySlug = story.StorySlug;
            StorySource = story.StorySource;
            ViewCount = story.ViewCount;
            StoryState = story.StoryState;
            PhotoId = story.PhotoId;
            Photo = story.Photo;
            DateCreated = story.DateCreated;
            DateUpdated = story.DateUpdated;
            LatestChapterOrder = story.LatestChapterOrder;
        }

        [Key]
        public int StoryId { get; set; }


        /* ================ Tên truyện ================ */
        [Column(TypeName = "nvarchar")]
        [DisplayName("Tên truyện")]
        [Required(ErrorMessage = "Phải nhập {0}")]
        [StringLength(255, ErrorMessage = "{0} tối đa {1} kí tự.")]
        public string StoryName { get; set; }

        /* ================ Tác giả ================ */
        [DisplayName("Tác giả")]
        public int AuthorId { get; set; }

        [DisplayName("Tác giả")]
        [ForeignKey(nameof(AuthorId))]
        public Author? Author { get; set; }


        /* ================ Mô tả ================ */
        [DisplayName("Mô tả ngắn")]
        [Required(ErrorMessage = "Phải nhập {0}")]
        public string Description { get; set; }


        /* ================ Slug ================ */
        [StringLength(255)]
        public string? StorySlug { get; set; }


        /* ================ Nguồn ================ */
        [DisplayName("Nguồn truyện")]
        public string? StorySource { get; set; }

        /* ================ Lượt xem ================ */
        [DisplayName("Lượt xem")]
        public int ViewCount { get; set; }


        /* ================ Trạng thái: Full/Đang ra ================ */
        [DisplayName("Trạng thái")]
        public bool StoryState { get; set; }


        /* ================ Thumbnail image ================ */
        public int? PhotoId { get; set; }

        [ForeignKey(nameof(PhotoId))]
        public StoryPhoto? Photo { get; set; }


        /* ================ Ngày đăng, cập nhật ================ */
        [DisplayName("Ngày đăng")]
        public DateTime DateCreated { get; set; }

        [DisplayName("Ngày cập nhật")]
        public DateTime DateUpdated { get; set; }

        /* ================ Xuất bản ================ */
        [DisplayName("Xuất bản")]
        public bool Published { get; set; }

        /* ================ Chương mới nhất ================ */
        [DisplayName("Chương mới nhất")]
        public int LatestChapterOrder { get; set; }


        public ICollection<StoryCategory>? StoryCategory { get; set; }
        public ICollection<Chapter>? Chapters { get; set; }


    }
}