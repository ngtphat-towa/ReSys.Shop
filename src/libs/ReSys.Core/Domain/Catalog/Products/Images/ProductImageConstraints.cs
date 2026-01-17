namespace ReSys.Core.Domain.Catalog.Products.Images;

public static class ProductImageConstraints
{
    public const int UrlMaxLength = 2048;
    public const int AltMaxLength = 500;
    public const int ModelNameMaxLength = 100;
    public const int ModelVersionMaxLength = 50;
    public const int ContentTypeMaxLength = 100;

    /// <summary>
    /// Predefined AI Models for Semantic Search and Visual Similarity.
    /// Choosing models with high "One-Shot" capacity for zero-label discovery.
    /// </summary>
    public static class AIModels
    {
        /// <summary>
        /// Fashion-CLIP (512 dims)
        /// USE CASE: Text-to-Image / Semantic Search.
        /// PROS: Understands "Style" keywords (e.g., 'boho', 'minimalist') specifically for apparel.
        /// CONS: Higher latency; requires specialized fashion fine-tuning.
        /// </summary>
        public const string FashionClip = "fashion-clip-v1";
        public const int FashionClipDimensions = 512;

        /// <summary>
        /// DINOv2 (1024 dims)
        /// USE CASE: "Find Similar" / Visual Identity.
        /// PROS: SOTA for geometry and texture; doesn't need text labels to understand object similarity.
        /// CONS: Large vector size (1024+); no native text-to-image bridge.
        /// </summary>
        public const string DinoV2 = "dinov2-vit-large";
        public const int DinoV2Dimensions = 1024;

        /// <summary>
        /// ConvNeXt-V2 (1024 dims)
        /// USE CASE: High-speed categorization and attribute extraction.
        /// PROS: Extremely stable for "One-Shot" classification; faster than Transformers on CPU.
        /// CONS: Less semantic depth than CLIP models.
        /// </summary>
        public const string ConvNext = "convnext-v2-base";
        public const int ConvNextDimensions = 1024;

        /// <summary>
        /// EfficientNet-B4 (1792 dims)
        /// USE CASE: Real-time mobile visual search.
        /// PROS: Incredible throughput-to-accuracy ratio.
        /// CONS: Older architecture; larger vector size relative to modern ViTs.
        /// </summary>
        public const string EfficientNet = "efficientnet-b4";
        public const int EfficientNetDimensions = 1792;
    }
}
