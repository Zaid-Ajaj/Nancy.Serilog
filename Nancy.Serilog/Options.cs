namespace Nancy.Serilog
{
    public class Options
    {
        public FieldChoser<RequestLogData> IgnoredRequestLogFields { get; set; } = new FieldChoser<RequestLogData>();
        public FieldChoser<ErrorLogData>  IgnoreErrorLogFields { get; set; } = new FieldChoser<ErrorLogData>();
        public FieldChoser<ResponseLogData> IgnoredResponseLogFields { get; set; } = new FieldChoser<ResponseLogData>();
    }
}
