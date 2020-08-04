using System;

namespace MT.Infra.BusinessLayer.Models
{
    public class SRExpiry_MailParametersForUser
    {
        public string SRID { get; set; }
        public string SRDescription { get; set; }
        public string userMailID { get; set; }
        public DateTime srCreatedDateDetails { get; set; }
        public DateTime srTillDate { get; set; }
        public DateTime mailDate { get; set; }
        public string usageType { get; set; }
    }
}
