

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace VT.Model {

    public class ComponentService {
        private readonly AppDbContext context;

        public ComponentService(AppDbContext ctx) {
            this.context = ctx;
        }

        
       
    }
}
