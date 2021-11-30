namespace SKD.Service;

public class ErrorHelper {
    public static Error Create<T>(Expression<Func<T, object>> expression, string msg) {

        var path = "";
        if (expression.Body is MemberExpression) {
            path = ((MemberExpression)expression.Body).Member.Name;
        } else {
            var op = ((UnaryExpression)expression.Body).Operand;
            path = ((MemberExpression)op).Member.Name;
        }

        return new Error() {
            Path = new List<string> { path },
            Message = msg
        };
    }
}
