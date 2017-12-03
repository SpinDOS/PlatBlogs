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
        public LoadMoreModel(string action) { Action = action; }

        public string Action { get; set; }
        public string Text { get; set; } = "Load more";

        public int Offset { get; set; } = 0;

        private Dictionary<string, string> _additionalFields = null;
        public Dictionary<string, string> AdditionalFields
        {
            get => _additionalFields ?? (_additionalFields = new Dictionary<string, string>());
            set => _additionalFields = value;
        }
        public bool HasAdditionalFields => _additionalFields != null && _additionalFields.Count > 0;

        private string _method = "get";
        public string Method
        {
            get => _method;
            set => _method = value.ToLower();
        }

    }
}