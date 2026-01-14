using Carter;

using ErrorOr;

using Microsoft.AspNetCore.StaticFiles;

using ReSys.Core.Common.Imaging;
using ReSys.Core.Common.Storage;
using ReSys.Shared.Models;

namespace ReSys.Api.Features.Files;

/// <summary>
/// Response model for file upload operations.
/// </summary>
/// <param name="Width">Image width in pixels.</param>
/// <param name="Height">Image height in pixels.</param>
/// <param name="Format">Image format (e.g., webp).</param>
/// <param name="SizeBytes">File size in bytes.</param>
/// <param name="SavedName">Unique identifier of the saved file.</param>
/// <param name="Url">Relative URL to access the file.</param>
/// <param name="Hash">SHA256 hash of the file content.</param>
public record FileUploadResponse(
    int Width,
    int Height,
    string Format,
    long SizeBytes,
    string SavedName,
    string Url,
    string Hash);

/// <summary>
/// Module for handling file uploads and serving files.
/// </summary>
public class FilesModule : ICarterModule
{
    /// <summary>
    /// Registers endpoints for file management.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // BUS: Process image uploads by resizing and converting to WebP for optimization
        // API: Upload an image file for processing and storage
        app.MapPost("/api/files/image", async (
            IFormFile file,
            IImageService imageService,
            IFileService fileService,
            CancellationToken ct) =>
        {
            if (file == null || file.Length == 0)
                return Results.BadRequest("No file uploaded.");

            using var stream = file.OpenReadStream();

            // NOTE: Resize to standard max width for e-shop (e.g. 1200px) and convert to efficient WebP format.
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
                // NOTE: Using 'products' subdirectory and enforcing .webp extension
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

        // API: Retrieve a file stream by its path
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

        // API: Retrieve metadata for a specific file
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
