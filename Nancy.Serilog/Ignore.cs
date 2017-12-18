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
        public static FieldChooser<ResponseLogData> FromResponse() => new FieldChooser<ResponseLogData>();
        public static FieldChooser<RequestLogData> FromRequest() => new FieldChooser<RequestLogData>();
        public static FieldChooser<ErrorLogData> FromError() => new FieldChooser<ErrorLogData>();
    }

    public class FieldChooser<T>
    {
        private List<string> ignoredFields = new List<string>();

        public FieldChooser<T> Field<TProp>(Expression<Func<T, TProp>> action)
        {
            var expression = (MemberExpression)action.Body;
            string name = expression.Member.Name;
            ignoredFields.Add(name);
            return this;
        }

        public string[] ToArray() => ignoredFields.ToArray();
    }
}
