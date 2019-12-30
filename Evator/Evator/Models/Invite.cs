using System;
using System.Collections.Generic;

namespace Evator.Models
{
    public partial class Invite
    {
        public int IeId { get; set; }
        public int EtId { get; set; }
        public int AtId { get; set; }

        public virtual Accounts At { get; set; }
        public virtual Events Et { get; set; }
    }
}
