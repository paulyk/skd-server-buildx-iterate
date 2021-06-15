using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace SKD.Service {
    public class Error {

        public Error() {}
        public Error(string path, string message) {
          Path = new List<string>() { path };
          Message = message;
        }

        public List<string> Path { get; set; } = new List<string>();
        public string Message { get; set; }
    }
}