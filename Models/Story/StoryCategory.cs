using System.ComponentModel.DataAnnotations.Schema;

namespace truyenchu.Models {
    [Table(nameof(StoryCategory))]
    public class StoryCategory
    {
        public int StoryId { get; set; }
        public int CategoryId { get; set; }

        [ForeignKey(nameof(StoryId))]
        public Story Story { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; }
    }
}