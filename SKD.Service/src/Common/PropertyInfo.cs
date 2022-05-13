#nullable enable

using System.Reflection;

namespace SKD.Common;

public static class ServiceExnsins {

    public static MemberInfo GetAccessedMemberInfo<T>(this Expression<T> expression) {
        MemberExpression? memberExpression = null;

        if (expression.Body.NodeType == ExpressionType.Convert) {
            memberExpression = ((UnaryExpression)expression.Body).Operand as MemberExpression;
        } else if (expression.Body.NodeType == ExpressionType.MemberAccess) {
            memberExpression = expression.Body as MemberExpression;
        }

        if (memberExpression == null) {
            throw new ArgumentException("Not a member access", nameof(expression));
        }

        return memberExpression.Member;
    }
}
