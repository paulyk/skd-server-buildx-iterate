
namespace VT.Model {
    public class Error {
        public string Path { get; set; }
        public string Message { get; set; }

        public static Error NewError(string path, string msg) {
            return new Error() { Path = path, Message = msg};
        }
    }

}