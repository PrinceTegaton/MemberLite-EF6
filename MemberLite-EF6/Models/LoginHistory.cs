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
    
    public partial class LoginHistory
    {
        public int ID { get; set; }
        public string UserID { get; set; }
        public System.DateTime DateStamp { get; set; }
        public string IP { get; set; }
        public string UserAgent { get; set; }
        public string DeviceType { get; set; }
    
        public virtual Users Users { get; set; }
    }
}
