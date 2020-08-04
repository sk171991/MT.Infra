using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MT.Infra.Tool.Models
{
    public class TokenData
    {
        public string GUID { get; set; }
        public string ServiceRequest_ID { get; set; }
        public DateTime MailSentTimeStamp { get; set; }
        public DateTime MailResponseTimeStamp { get; set; }
        public int Status_ID { get; set; }
        public int MetaActive { get; set; }


    }
}