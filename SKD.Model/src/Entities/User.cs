using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class User : EntityBase {
        public string Email { get; set; }

        public User() : base() {

        }
    }
}