
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SKD.VCS.Model {
    public class ErrorHelper {
        public static Error Create<T>(Expression<Func<T, object>> expression, string msg) {

            var path = "";
            if (expression.Body is MemberExpression) {
              
                path = ((MemberExpression)expression.Body).Member.Name;
            } else {
                var op = ((UnaryExpression)expression.Body).Operand;
                path =  ((MemberExpression)op).Member.Name;
            }

            // var expr = (MemberExpression)expression.Body;
            // var prop = (PropertyInfo)expr.Member;

            return new Error() {
                Path = new string[] { path },
                Message = msg
            };
        }
    }
}