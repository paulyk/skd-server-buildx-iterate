#nullable enable

namespace SKD.Service;

public class MutationResult<T> where T : class {
    public MutationResult(T? payload = null) {
        Payload = payload;
    }
    public T? Payload { get; set; }
    public List<Error> Errors { get; set; } = new List<Error>();
}
