namespace Nancy.Serilog
{
    public class Options
    {
        public FieldChoser<RequestLogData> IgnoredRequestLogFields { get; set; } = new FieldChoser<RequestLogData>();
        public FieldChoser<ResponseLogData> IgnoreErrorLogFields { get; set; } = new FieldChoser<ResponseLogData>();
        public FieldChoser<ErrorLogData> IgnoredResponseLogFields { get; set; } = new FieldChoser<ErrorLogData>();
    }
}
