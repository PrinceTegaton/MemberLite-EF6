using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MemberLite_EF6.Models
{
    public partial class MemberLiteEntities
    {
        public MemberLiteEntities Init
        {
            get
            {
                var context = new MemberLiteEntities();
                context.Configuration.LazyLoadingEnabled = false;
                return context;
            }

            // This method saves the context configuration state
            // to be retained after model updates
        }
    }
}