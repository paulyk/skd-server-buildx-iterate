#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SKD.VCS.Model {

    public class ComponentService {
        private readonly SkdContext context;

        public ComponentService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<MutationPayload<Component>> SaveComponent(ComponentDTO dto) {
            var component = await context.Components.FirstOrDefaultAsync(t => t.Id == dto.Id);

            if (component != null) {
                component.Code = dto.Code;
                component.Name = dto.Name;
            } else {
                component = new Component { Code = dto.Code, Name = dto.Name };
                context.Components.Add(component);
            }
            Trim.TrimStringProperties<Component>(component);

            var payload = new MutationPayload<Component>(component);

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

        public async Task<MutationPayload<Component>> RemoveComponent(Guid componentId) {
            var component = await context.Components.FirstOrDefaultAsync(t => t.Id == componentId);
            var payload = new MutationPayload<Component>(component);

            payload.Errors = ValidateRemoveComponent<Component>(component);

            if (payload.Errors.Any()) {
                return payload;
            }

            component.RemovedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return payload;
        }

          public async Task<MutationPayload<Component>> RestoreComponent(Guid componentId) {
            var component = await context.Components.FirstOrDefaultAsync(t => t.Id == componentId);
            var payload = new MutationPayload<Component>(component);

            payload.Errors = ValidateRestoreComponent<Component>(component);

            if (payload.Errors.Any()) {
                return payload;
            }

            component.RemovedAt = null;
            await context.SaveChangesAsync();
            return payload;
        }

        public List<Error> ValidateRemoveComponent<T>(Component component) where T : Component {
            var errors = new List<Error>();

            if (component == null) {
                errors.Add(new Error("", "component not found"));
                return errors;
            }

            if (component.RemovedAt != null) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, "component already removed"));
            }
            return errors;
        }

         public List<Error> ValidateRestoreComponent<T>(Component component) where T : Component {
            var errors = new List<Error>();

              if (component == null) {
                errors.Add(new Error("", "component not found"));
                return errors;
            }

            if (component.RemovedAt == null) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, "component already active"));
            }
            return errors;
        }

        public async Task<List<Error>> ValidateCreateComponent<T>(Component component) where T : Component {
            var errors = new List<Error>();

            if (component.Code.Trim().Length == 0) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, "code requred"));
            } else if (component.Code.Length > EntityFieldLen.Component_Code) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, $"exceeded code max length of {EntityFieldLen.Component_Code} characters "));
            }
            if (component.Name.Trim().Length == 0) {
                errors.Add(ErrorHelper.Create<T>(t => t.Name, "name required"));
            } else if (component.Code.Length > EntityFieldLen.Component_Name) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, $"exceeded name max length of {EntityFieldLen.Component_Name} characters "));
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
