using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT.Infra.BusinessLayer.Models
{
    public class AdminDetails
    {
        public string AdminEmailID { get; set; }
        public int Role_ID { get; set; }
        public int Delegate_ID { get; set; }
        public int MetaActive { get; set; }
    }
}
