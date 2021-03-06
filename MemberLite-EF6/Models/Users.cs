//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MemberLite_EF6.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Users
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Users()
        {
            this.LoginHistory = new HashSet<LoginHistory>();
        }
    
        public string UserID { get; set; }
        public int SignInType { get; set; }
        public string FirstName { get; set; }
        public string OtherNames { get; set; }
        public string Gender { get; set; }
        public Nullable<System.DateTime> DOB { get; set; }
        public string Email { get; set; }
        public Nullable<long> Phone { get; set; }
        public string Password { get; set; }
        public int Status { get; set; }
        public bool EmailConfirmed { get; set; }
        public System.DateTime DateStamp { get; set; }
        public Nullable<System.DateTime> LockoutReleaseDate { get; set; }
        public string VerificationCode { get; set; }
        public Nullable<System.DateTime> LastUpdated { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LoginHistory> LoginHistory { get; set; }
    }
}
