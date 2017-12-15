using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nancy.Serilog
{
    public class Options
    {
        public string[] IgnoredRequestLogFields { get; set; } = new string[] { };
        public string[] IgnoreErrorLogFields { get; set; } = new string[] { };
        public string[] IgnoredResponseLogFields { get; set; } = new string[] { };
    }
}
