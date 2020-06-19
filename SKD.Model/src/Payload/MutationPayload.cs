using System.Collections.Generic;

namespace SKD.Model {
    public class MutationPayload<T> where T : class {

        public T Entity { get; set; }
        public List<Error> Errors { get; set; } = new List<Error>();

        public void AddError(string path, string msg) {
            Errors.Add(new Error() { Path = path, Message = msg});
        }
    }
}