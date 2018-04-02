using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MemberLite_EF6.Models
{
    public class SignInVM
    {
        [Required(ErrorMessage = "Enter your email or phone number")]
        [MaxLength(50)]
        [DataType(DataType.EmailAddress)]
        public string Login { get; set; }
        [Required(ErrorMessage = "Enter your password")]
        [MaxLength(20)]
        [DataType(DataType.Password)]

        //Named passkey to avoid conflict on same page with signup password
        public string Passkey { get; set; }
        public bool RememberMe { get; set; }
        public string ReturnUrl { get; set; }
    }
}