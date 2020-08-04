using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MT.Infra.Tool.Models
{
    public class Employee
    {
        public string MID { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }
    
    }
}
