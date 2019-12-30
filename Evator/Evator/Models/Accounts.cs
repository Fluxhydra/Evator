using System;
using System.Collections.Generic;

namespace Evator.Models
{
    public partial class Accounts
    {
        public Accounts()
        {
            AtToken = new HashSet<AtToken>();
            Attend = new HashSet<Attend>();
            Events = new HashSet<Events>();
            Invite = new HashSet<Invite>();
        }

        public int AtId { get; set; }
        public string AtName { get; set; }
        public int EmployeeId { get; set; }
        public string Department { get; set; }
        public string Email { get; set; }
        public string AtPassword { get; set; }
        public string AtProfile { get; set; }
        public byte RoleType { get; set; }

        public virtual ICollection<AtToken> AtToken { get; set; }
        public virtual ICollection<Attend> Attend { get; set; }
        public virtual ICollection<Events> Events { get; set; }
        public virtual ICollection<Invite> Invite { get; set; }
    }
}
