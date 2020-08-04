using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MT.Infra.Tool.Models
{
    public class ApproverSRDetails
    {
        public string Comments { get; set; }
        public NextStatus Statuses { get; set; }
        public string selectStatus { get; set; }
    }
    public enum NextStatus
    {
        InProgress,
        Closed
    }
}