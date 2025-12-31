using Carter;
using Microsoft.AspNetCore.StaticFiles;
using ReSys.Core.Interfaces;

namespace ReSys.Api.Features.Files;

public class FilesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/files/{fileName}", (string fileName, IFileService fileService) =>
        {
            try
            {
                var stream = fileService.GetFileStream(fileName);
                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(fileName, out var contentType))
                {
                    contentType = "application/octet-stream";
                }
                return Results.File(stream, contentType);
            }
            catch (FileNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("GetFile")
        .WithTags("Files");
    }
}
