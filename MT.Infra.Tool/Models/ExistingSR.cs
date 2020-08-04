using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MT.Infra.Tool.Models
{
    public class ExistingSR
    {
        public int ID { get; set; }

        public string SR_Number { get; set; }

        public DateTime SR_Ticket_From { get; set; }

        public DateTime SR_Ticket_To { get; set; }

        public int Status { get; set; }
    }
}

