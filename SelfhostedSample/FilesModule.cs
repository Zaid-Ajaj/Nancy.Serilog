using Nancy;
using System.IO;
using System;
using System.Linq;
using Nancy.Responses;

namespace SelfhostedSample
{
    public class FilesModule : NancyModule
    {
        public FilesModule() : base("/files")
        {
            Before += ctx => 
                !ctx.Request.Files.Any() 
                    ? new TextResponse(HttpStatusCode.BadRequest, "Not Found") 
                    : null;

            Get["/"] = args =>
            {
                return "Cannot reach me :( ";
            };
            
            Post["/sync"] = x =>
            {
                var file = Request.Files.FirstOrDefault();

                if (file == null) return Response.AsText("No file provided");
                
                using (var reader = new StreamReader(file.Value))
                {
                    var fileContent = reader.ReadToEnd();
                    return Response.AsText($"File Contents: \n{fileContent}");
                }
            };
        }
    }
}