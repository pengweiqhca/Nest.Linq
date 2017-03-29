using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Nest.Linq.Utility;

namespace Nest.Linq.Request.Visitors
{
    class GroupKeyExpressionVisitor : ExpressionVisitor
    {
        private readonly MethodInfo getKey;

        private readonly ParameterExpression bindingParameter;
        public GroupKeyExpressionVisitor(MethodInfo getKey, ParameterExpression bindingParameter)
        {
            this.getKey = getKey;
            this.bindingParameter = bindingParameter;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member.Name == "Key" && node.Member.DeclaringType.IsGenericOf(typeof(IGrouping<,>)))
                return Expression.Convert(Expression.Call(null, getKey, bindingParameter, Expression.Constant(node.Type)), node.Type);

            return base.VisitMember(node);
        }
    }
}
