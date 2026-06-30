using System.Collections.Generic;

namespace tas.Models
{
    public class Status
    {
        public int Id { get; set; }
        public string Name { get; set; }       

        public ICollection<AppTask> Tasks { get; set; }
    }
}