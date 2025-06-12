using System.ComponentModel.DataAnnotations;

namespace aspnet.Data
{
    public class Courses
    {
        [Key]
        public int courseId { get; set; }
        public string? courseName { get; set; }

    }
}