

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.Model {

    public class ComponentService {
        private readonly AppDbContext context;

        public ComponentService(AppDbContext ctx) {
            this.context = ctx;

        }

        public async Task<MutationPayload<Component>> CreateComponent(Component component) {
            context.Components.Add(component);

            // validate
            var payload = await ValidateCreateComponent(component);
            if (payload.Errors.Any()) {
                return payload;
            }

            // save
            await context.SaveChangesAsync();

            payload.Entity = component;
            return payload;                        
        }

        public async Task<MutationPayload<Component>> ValidateCreateComponent(Component component) {
            var payload = new MutationPayload<Component>();
            payload.Entity = component;

            if (component.Code.Trim().Length == 0) {
                payload.AddError("code", $"component code required");
            }
            if (component.Name.Trim().Length == 0) {
                payload.AddError("name", $"component name required");
            }

            if (await context.Components.AnyAsync(t => t.Id != component.Id && t.Code == component.Code)) {
                payload.AddError("code", "Duplicate component code");
            }
            if (await context.Components.AnyAsync(t => t.Id != component.Id && t.Name == component.Name)) {
                payload.AddError("name", "Duplicate component name");
            }

            return payload;
        }



    }
}
