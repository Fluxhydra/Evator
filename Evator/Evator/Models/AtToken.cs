using System;
using System.Collections.Generic;

namespace Evator.Models
{
    public partial class AtToken
    {
        public string Token { get; set; }
        public int AtId { get; set; }
        public byte TnStatus { get; set; }

        public virtual Accounts At { get; set; }
    }
}
