using System.Collections.Generic;

namespace SKD.Model {

    public class MutationPayload<T> where T : class {
        public MutationPayload(T entity) {
            Entity = entity;
        }
        public T Entity { get; set; }
        public IEnumerable<Error> Errors { get; set; } = new List<Error>();
    }
}