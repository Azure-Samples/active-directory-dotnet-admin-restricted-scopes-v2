using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace GroupManager.Models
{
    public class Group
    {
        public string id { get; set; }
        public string description { get; set; }
        public string displayName { get; set; }
        public string mailNickname { get; set; }
    }

    public class GroupResponse
    {
        public List<Group> value { get; set; }
    }
}