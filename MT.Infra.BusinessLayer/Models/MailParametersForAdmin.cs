using System;

namespace MT.Infra.BusinessLayer.Models
{
    public class MailParametersForAdmin
    {
        public string SRID { get; set; }
        public string Name { get; set; }
        public string SRDescription { get; set; }
        public DateTime CreatedTimeStamp { get; set; }
        public string AdminMailID { get; set; }
    }
}
