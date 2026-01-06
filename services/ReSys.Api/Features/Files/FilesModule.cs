using Carter;
using ErrorOr;
using Microsoft.AspNetCore.StaticFiles;
using ReSys.Core.Common.Imaging;
using ReSys.Core.Common.Storage;
using ReSys.Core.Common.Models;

namespace ReSys.Api.Features.Files;

public record FileUploadResponse(
    int Width,
    int Height,
    string Format,
    long SizeBytes,
    string SavedName,
    string Url,
    string Hash);

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
            var result = await imageService.ProcessAsync(
                stream, 
                file.FileName, 
                maxWidth: 1200, 
                ct: ct);

            if (result.IsError)
                return Results.BadRequest(result.FirstError.Description);

            var processed = result.Value;
            using (processed.Main.Stream)
            {
                // We can use custom metadata for the file service
                var options = new FileUploadOptions("products")
                {
                    AllowedExtensions = new[] { ".webp" },
                    GenerateHash = true,
                    OverwriteExisting = false
                };

                var saveResult = await fileService.SaveFileAsync(processed.Main.Stream, processed.Main.FileName, options, ct);
                
                return saveResult.Match(
                    success => Results.Ok(ApiResponse.Success(new FileUploadResponse(
                        processed.Main.Width,
                        processed.Main.Height,
                        "webp",
                        processed.Main.Size,
                        success.FileId,
                        $"/api/files/{success.Subdirectory}/{success.FileId}",
                        success.Hash
                    ))),
                    errors => Results.BadRequest(errors.First().Description)
                );
            }
        })
        .WithName("UploadImage")
        .WithTags("Files")
        .DisableAntiforgery();

        app.MapGet("/api/files/{**path}", async (string path, IFileService fileService, CancellationToken ct) =>
        {
            var result = await fileService.GetFileStreamAsync(path, ct);

            return result.Match(
                stream => 
                {
                    var fileName = Path.GetFileName(path);
                    var provider = new FileExtensionContentTypeProvider();
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

        app.MapGet("/api/files/meta/{**path}", async (string path, IFileService fileService, CancellationToken ct) =>
        {
            var result = await fileService.GetFileMetadataAsync(path, ct);

            return result.Match(
                metadata => Results.Ok(ApiResponse.Success(metadata)),
                errors => 
                {
                    if (errors.Any(e => e.Type == ErrorType.NotFound))
                        return Results.NotFound();
                    return Results.BadRequest(errors.First().Description);
                }
            );
        })
        .WithName("GetFileMetadata")
        .WithTags("Files");
    }
}
