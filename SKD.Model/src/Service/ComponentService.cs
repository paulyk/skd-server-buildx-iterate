

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.Model {

    public class ComponentService {
        private readonly SkdContext context;

        public ComponentService(SkdContext ctx) {
            this.context = ctx;

        }

        public async Task<MutationPayload<Component>> CreateComponent(Component component) {
            var payload = new MutationPayload<Component>(component);
            context.Components.Add(component);

            // validate
            payload.Errors = await ValidateCreateComponent<Component>(component);
            if (payload.Errors.Any()) {
                return payload;
            }

            // save
            await context.SaveChangesAsync();

            payload.Entity = component;
            return payload;                        
        }

        public async Task<List<Error>> ValidateCreateComponent<T>(Component component) where T : Component {
            var errors = new List<Error>();

            if (component.Code.Trim().Length == 0) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, "code requred"));
            }
            if (component.Name.Trim().Length == 0) {
                errors.Add(ErrorHelper.Create<T>(t => t.Name, "name required"));
            }

            if (await context.Components.AnyAsync(t => t.Id != component.Id && t.Code == component.Code)) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, "duplicate code"));
            }
            if (await context.Components.AnyAsync(t => t.Id != component.Id && t.Name == component.Name)) {
                errors.Add(ErrorHelper.Create<T>(t => t.Name, "duplicate name"));
            }

            return errors;
        }



    }
}
