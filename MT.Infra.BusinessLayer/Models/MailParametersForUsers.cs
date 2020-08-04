using System;
using System.ComponentModel.DataAnnotations;

namespace MT.Infra.BusinessLayer.Models
{
    public class MailParametersForUsers
    {
        public string SRID { get; set; }

        public string Name { get; set; }

        public string SRDescription { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreatedTimeStamp { get; set; }

        public string EmailId { get; set; }
    }
}
