#nullable enable
using System.Collections.Generic;

namespace SKD.Model {

    public class QueryPayload<T> where T : class {
        public QueryPayload(T? entity) {
            Result = entity;
        }
        public T? Result { get; set; }
        public List<Error> Errors { get; set; } = new List<Error>();
    }
}