
namespace Grand.Core.Domain.Seo
{
    /// <summary>
    /// Represents an entity which supports slug (SEO friendly one-word URLs)
    /// </summary>
    public interface ISlugSupported
    {
        string SeName { get; set; }
    }
}
