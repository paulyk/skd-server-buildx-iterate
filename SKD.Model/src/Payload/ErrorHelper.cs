
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SKD.Model {
  public class ErrorHelper {
     public static Error Create<T>(Expression<Func<T, object>> expression, string msg) {
            var expr = (MemberExpression)expression.Body;
            var prop = (PropertyInfo)expr.Member;

            return new Error() {
              Path = new string[] { prop.Name },
              Message = msg
            };            
        }
  }
}