using System;
using System.ComponentModel.DataAnnotations;
namespace MT.Infra.BusinessLayer
{
  public class Viewusermodel
    {
        [RegularExpression(@"^[A-Za-z]*$", ErrorMessage = "String Only")]
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

       
        [Required(ErrorMessage = "Employee Id is required.")]
        public string EmployeeID { get; set; }

        [DataType(DataType.PhoneNumber, ErrorMessage = "Invalid Contact Number")]
        [Required(ErrorMessage = "Contact Number is required.")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "User Location is required.")]
        public string UserLocation { get; set; }

        [DataType(DataType.EmailAddress, ErrorMessage = "Invalid emaild address")]
        [RegularExpression(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", ErrorMessage = "Please enter valid email id.")]
        [Required(ErrorMessage = "Emailld is required.")]
        public string EmailId { get; set; }

         
        public string UserRole { get; set; }

        public int ID { get; set; }

     
        public int Role_ID { get; set; }
    }
}
