using System;
using System.Collections.Generic;

namespace Evator.Models
{
    public partial class Attend
    {
        public int AdId { get; set; }
        public int EtId { get; set; }
        public int AtId { get; set; }
        public DateTime AdRecord { get; set; }

        public virtual Accounts At { get; set; }
        public virtual Events Et { get; set; }
    }
}
