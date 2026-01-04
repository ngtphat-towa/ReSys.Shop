using Carter;
using ErrorOr;
using Microsoft.AspNetCore.StaticFiles;
using ReSys.Core.Common.Imaging;
using ReSys.Core.Common.Storage;

namespace ReSys.Api.Features.Files;

public class FilesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/files/image", async (
            IFormFile file, 
            IImageService imageService, 
            IFileService fileService, 
            CancellationToken ct) =>
        {
            if (file == null || file.Length == 0)
                return Results.BadRequest("No file uploaded.");

            using var stream = file.OpenReadStream();
            
            // Practical Use Case: Resize to standard max width for e-shop (e.g. 1200px)
            // and convert to efficient WebP format.
            var (processedStream, metadata) = await imageService.ProcessImageAsync(
                stream, 
                file.FileName, 
                maxWidth: 1200, 
                cancellationToken: ct);

            using (processedStream)
            {
                // We can use custom metadata for the file service
                var options = new FileUploadOptions(
                    Subdirectory: "products", // Organize images in products folder
                    AllowedExtensions: new[] { ".webp" },
                    GenerateHash: true,
                    OverwriteExisting: false
                );

                var saveResult = await fileService.SaveFileAsync(processedStream, metadata.FileName, options, ct);
                
                return saveResult.Match(
                    success => Results.Ok(new 
                    { 
                        metadata.Width,
                        metadata.Height,
                        metadata.Format,
                        metadata.SizeBytes,
                        SavedName = success.FileId,
                        Url = $"/api/files/{success.FileId}", // Use FileId (which might be the filename)
                        success.Hash
                    }),
                    errors => Results.BadRequest(errors.First().Description)
                );
            }
        })
        .WithName("UploadImage")
        .WithTags("Files")
        .DisableAntiforgery();

        app.MapGet("/api/files/{fileName}", async (string fileName, IFileService fileService, CancellationToken ct) =>
        {
            var result = await fileService.GetFileStreamAsync(fileName, ct);

            return result.Match(
                stream => 
                {
                    var provider = new FileExtensionContentTypeProvider();
                    // We might need to guess content type from stream or name if not provided
                    // Ideally we should use GetFileMetadataAsync to get the real content type
                    if (!provider.TryGetContentType(fileName, out var contentType))
                    {
                        contentType = "application/octet-stream";
                    }
                    return Results.File(stream, contentType);
                },
                errors => 
                {
                    if (errors.Any(e => e.Type == ErrorType.NotFound))
                        return Results.NotFound();
                    return Results.BadRequest(errors.First().Description);
                }
            );
        })
        .WithName("GetFile")
        .WithTags("Files");
    }
}