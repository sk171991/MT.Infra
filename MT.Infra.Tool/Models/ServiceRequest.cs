using System;
using System.ComponentModel.DataAnnotations;

namespace MT.Infra.Tool.Models
{
    public class ServiceRequest
    {
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddThh:mm:ss}")]

        public DateTime SRDate { get; set; }

        [Required]
        public string SRDescription { get; set; }
         
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please select Purpose")]
        public int Purpose { get; set; } 

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter from date.")]
        public DateTime FromDate { get; set; }

        public DateTime TillDate { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please select Location.")]
        public string UserLocation { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter Contact Number.")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        public string ContactNumber { get; set; }

        public DateTime LastModifiedTimeStamp { get; set; }

        public int MetaActive { get; set; }

        public string Comments { get; set; }

        public string AttachmentName { get; set; }

        public string Attachment { get; set; }

        [Required(ErrorMessage = "Please select either Yes or No")]
        public string Selection { get; set; }

    }
}