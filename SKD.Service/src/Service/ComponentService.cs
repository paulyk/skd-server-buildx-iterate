#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SKD.Common;
using SKD.Model;

namespace SKD.Service {

    public class ComponentService {
        private readonly SkdContext context;

        public ComponentService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<MutationPayload<Component>> SaveComponent(ComponentInput input) {
            var payload = new MutationPayload<Component>(null);
            payload.Errors = await ValidateCreateComponent<ComponentInput>(input);
            if (payload.Errors.Any()) {
                return payload;
            }


            var component = await context.Components.FirstOrDefaultAsync(t => t.Id == input.Id);

            if (component == null) {
                component = new Component();
                context.Components.Add(component);
            }

            component.Code = input.Code;
            component.Name = input.Name;
            component.DcwsSerialCaptureRule = input.DcwsSerialCaptureRule;

            Trim.TrimStringProperties<Component>(component);
            // save
            await context.SaveChangesAsync();
            payload.Payload = component;
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

        public async Task<List<Error>> ValidateCreateComponent<T>(ComponentInput input) where T : ComponentInput {
            var errors = new List<Error>();

            Component? existingComponent = null;
            if (input.Id.HasValue) {
                existingComponent = await context.Components.FirstOrDefaultAsync(t => t.Id == input.Id.Value);
                if (existingComponent == null) {
                    errors.Add(ErrorHelper.Create<T>(t => t.Id, $"component not found: {input.Id}"));
                    return errors;
                }
            }

            // code
            if (input.Code.Trim().Length == 0) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, "code requred"));
            } else if (input.Code.Length > EntityFieldLen.Component_Code) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, $"exceeded code max length of {EntityFieldLen.Component_Code} characters "));
            }

            // name
            if (input.Name.Trim().Length == 0) {
                errors.Add(ErrorHelper.Create<T>(t => t.Name, "name required"));
            } else if (input.Code.Length > EntityFieldLen.Component_Name) {
                errors.Add(ErrorHelper.Create<T>(t => t.Code, $"exceeded name max length of {EntityFieldLen.Component_Name} characters "));
            }

            // duplicate code
            if (existingComponent != null) {
                if (await context.Components.AnyAsync(t => t.Id != input.Id && t.Code == input.Code)) {
                    errors.Add(ErrorHelper.Create<T>(t => t.Code, "duplicate code"));
                }
            } else {
                // adding a new component, so look for duplicate
                if (await context.Components.AnyAsync(t => t.Code == input.Code)) {
                    errors.Add(ErrorHelper.Create<T>(t => t.Code, "duplicate code"));
                }
            }

            // duplicate name
            if (existingComponent != null) {
                if (await context.Components.AnyAsync(t => t.Id != input.Id && t.Name == input.Name)) {
                    errors.Add(ErrorHelper.Create<T>(t => t.Name, "duplicate name"));
                }
            } else {
                // adding a new component, so look for duplicate
                if (await context.Components.AnyAsync(t => t.Name == input.Name)) {
                    errors.Add(ErrorHelper.Create<T>(t => t.Code, "duplicate name"));
                }
            }

            return errors;
        }
    }
}
