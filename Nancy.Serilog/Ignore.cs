using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Nancy.Serilog
{
    public static class Ignore
    {
        public static FieldChoser<ResponseLogData> FromResponse() => new FieldChoser<ResponseLogData>();
        public static FieldChoser<RequestLogData> FromRequest() => new FieldChoser<RequestLogData>();
        public static FieldChoser<ErrorLogData> FromError() => new FieldChoser<ErrorLogData>();
    }

    public class FieldChoser<T>
    {
        private List<string> ignoredFields = new List<string>();

        public FieldChoser<T> Field<TProp>(Expression<Func<T, TProp>> action)
        {
            var expression = (MemberExpression)action.Body;
            string name = expression.Member.Name;
            ignoredFields.Add(name);
            return this;
        }

        public string[] ToArray() => ignoredFields.ToArray();
    }
}
