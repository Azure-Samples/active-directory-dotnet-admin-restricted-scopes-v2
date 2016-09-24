using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GroupManager.Models
{
    public class User
    {
        public string displayName { get; set; }
        public string jobTitle { get; set; }
        public string mail { get; set; }
        public string id { get; set; }
    }

    public class UserResponse
    {
        public List<User> value { get; set; }
    }
}