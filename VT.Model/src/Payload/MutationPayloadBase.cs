using System.Collections.Generic;

namespace VT.Model {
    public class MutationPayloadBase {
        public List<Error> Errors { get; set; } = new List<Error>();

        public void AddError(string path, string msg) {
            Errors.Add(new Error() { Path = path, Message = msg});
        }
    }
}