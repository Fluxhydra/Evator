using System;
using System.Collections.Generic;

namespace Evator.Models
{
    public partial class Events
    {
        public Events()
        {
            Attend = new HashSet<Attend>();
            Invite = new HashSet<Invite>();
        }

        public int EtId { get; set; }
        public int OwnerId { get; set; }
        public string EtName { get; set; }
        public string Speaker { get; set; }
        public string EtLocation { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public TimeSpan TimeStart { get; set; }
        public TimeSpan TimeEnd { get; set; }
        public string EtDescription { get; set; }
        public string QrInvite { get; set; }
        public string QrAttend { get; set; }
        public string Banner { get; set; }
        public byte EtStatus { get; set; }

        public virtual Accounts Owner { get; set; }
        public virtual ICollection<Attend> Attend { get; set; }
        public virtual ICollection<Invite> Invite { get; set; }
    }
}
