using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Storage;
using ReSys.Core.Common.Imaging;
using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Domain.Catalog.Products.Images;
using ReSys.Core.Features.Admin.Catalog.Products.Images.Common;

namespace ReSys.Core.Features.Admin.Catalog.Products.Images.UploadProductImage;

public static class UploadProductImage
{
    public class Request(
        Guid productId, 
        Stream imageStream, 
        string imageName, 
        Guid? variantId = null, 
        ProductImage.ProductImageType role = ProductImage.ProductImageType.Gallery,
        string? alt = null)
    {
        public Guid ProductId { get; set; } = productId;
        public Stream ImageStream { get; set; } = imageStream;
        public string ImageName { get; set; } = imageName;
        public Guid? VariantId { get; set; } = variantId;
        public ProductImage.ProductImageType Role { get; set; } = role;
        public string? Alt { get; set; } = alt;
    }

    public record Command(Request Request) : IRequest<ErrorOr<ProductImageListItem>>;

    public class Handler(
        IApplicationDbContext context,
        IFileService fileService,
        IImageService imageService) : IRequestHandler<Command, ErrorOr<ProductImageListItem>>
    {
        public async Task<ErrorOr<ProductImageListItem>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var product = await context.Set<Product>()
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product is null)
                return ProductErrors.NotFound(request.ProductId);

            // 1. Process Image
            var processResult = await imageService.ProcessAsync(
                request.ImageStream,
                request.ImageName,
                maxWidth: 1920,
                generateThumbnail: true,
                ct: cancellationToken);

            if (processResult.IsError) return processResult.Errors;

            // 2. Save File
            var processed = processResult.Value;
            var saveResult = await fileService.SaveFileAsync(
                processed.Main.Stream,
                processed.Main.FileName,
                new FileUploadOptions("products"),
                cancellationToken);

            if (saveResult.IsError) return saveResult.Errors;

            // 3. Update Domain
            var fileName = saveResult.Value.FileId;
            var subdir = saveResult.Value.Subdirectory;
            var imageUrl = $"/api/files/{subdir}/{fileName}";

            var addImageResult = product.AddImage(
                imageUrl, 
                request.Alt, 
                request.VariantId, 
                request.Role);

            if (addImageResult.IsError) return addImageResult.Errors;

            var newImage = addImageResult.Value;
            context.Set<ProductImage>().Add(newImage);
            context.Set<Product>().Update(product);

            await context.SaveChangesAsync(cancellationToken);

            return newImage.Adapt<ProductImageListItem>();
        }
    }
}
