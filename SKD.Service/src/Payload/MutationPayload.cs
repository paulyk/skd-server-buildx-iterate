#nullable enable

namespace SKD.Service;

public class MutationPayload<T> where T : class {
    public MutationPayload(T? payload = null) {
        Payload = payload;
    }
    public T? Payload { get; set; }
    public List<Error> Errors { get; set; } = new List<Error>();
}
