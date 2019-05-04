using System;

namespace Nancy.Serilog
{
    public class ResponseCookie
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool HttpOnly { get; set; }
        public bool Secure { get; set; }
        public DateTime? Expires { get; set; }
    }
}
