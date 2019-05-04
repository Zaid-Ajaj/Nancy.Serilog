namespace Nancy.Serilog
{
    public class NancySerilogOptions
    {
        public FieldChooser<RequestLogData> IgnoredRequestLogFields { get; set; } = new FieldChooser<RequestLogData>();
        public FieldChooser<ErrorLogData>  IgnoreErrorLogFields { get; set; } = new FieldChooser<ErrorLogData>();
        public FieldChooser<ResponseLogData> IgnoredResponseLogFields { get; set; } = new FieldChooser<ResponseLogData>();
    }
}
