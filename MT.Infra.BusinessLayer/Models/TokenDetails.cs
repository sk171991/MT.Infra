using System;

namespace MT.Infra.BusinessLayer.Models
{
    public class TokenDetails
    {
        public int ServiceRequest_ID { get; set; }
        public string guid { get; set; }
        public string status { get; set; }
        public int User_ID { get; set; }
        public DateTime LastModifiedTimeStamp { get; set; }
        public int MetaActive { get; set; }
        public int Approver_UserID { get; set; }
        public int User_UserID { get; set; }
        public string SRCreatedBy { get; set; }
        public string userMailID { get; set; }
        public string Approver2 { get; set; }
    }
}

