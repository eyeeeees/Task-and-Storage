using System;

namespace tas.DTOs
{
    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime CreatedAt { get; set; }
        public string StatusName { get; set; }
        public string ExecutorLogin { get; set; }
        public int? UserId { get; set; }
    }
}