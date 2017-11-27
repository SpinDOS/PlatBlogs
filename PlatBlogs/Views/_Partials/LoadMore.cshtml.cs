using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PlatBlogs.Views._Partials
{
    public class LoadMoreModel
    {
        public string Action { get; set; }
        public int Offset { get; set; }

        private Dictionary<string, string> _additionalFields = null;

        public bool HasAdditionalFields => _additionalFields != null && _additionalFields.Count > 0;

        public Dictionary<string, string> AdditionalFields 
            => _additionalFields ?? (_additionalFields = new Dictionary<string, string>());
    }
}