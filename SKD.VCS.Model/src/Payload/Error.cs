using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace SKD.VCS.Model {
    public class Error {

        public Error() {}
        public Error(string path, string message) {
          Path = new string[] { path };
          Message = message;
        }

        public IEnumerable<string> Path { get; set; } = new List<string>();
        public string Message { get; set; }
    }
}