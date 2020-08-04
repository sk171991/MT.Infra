using System;

namespace MT.Infra.BusinessLayer.Models
{
    public class InsertToken
    {
        public string guid { get; set; }
        public int ServiceRequest_ID { get; set; }
        public int Status_ID { get; set; }
        public string User_ID { get; set; }
        public DateTime CreatedTimeStamp { get; set; }
        public DateTime LastModifiedTimeStamp { get; set; }
        public int MetaActive { get; set; }
    }
}
