using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT.Infra.BusinessLayer
{
   public class ViewAssetsModel
    {
        
        public int ID { get; set; }

        [Required(ErrorMessage = "System Name is required.")]
        public string SystemName { get; set; }

        [Required(ErrorMessage = "System IP is required.")]
        public string SystemIP { get; set; }

        public string SerialNumber { get; set; }
        public string MachineCode { get; set; }

        [Required(ErrorMessage = "Machine Type is required.")]
        public string MachineType { get; set; }

        [Required(ErrorMessage = "Registration Date is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime RegistrationDate { get; set; }

        [Required(ErrorMessage = "Expiration Date is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ExpirationDate { get; set; }

    }
}
