using System;

namespace tas.Models
{
    public class AppTask
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? Deadline { get; set; }          
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
        public int StatusId { get; set; }
        public Status Status { get; set; }
        public int? UserId { get; set; }
        public User User { get; set; }
    }
}