using System.Collections.Generic;

namespace tas.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }   
        public string Role { get; set; }        

        public ICollection<AppTask> Tasks { get; set; }
    }
}