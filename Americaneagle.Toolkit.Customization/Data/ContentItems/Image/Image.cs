using Microsoft.AspNetCore.Http;

namespace Accelerator;

public partial class Image
{
    public string AlternateText =>
        !string.IsNullOrWhiteSpace(ImageAlternateText)
            ? ImageAlternateText : !string.IsNullOrWhiteSpace(MediaDescription)
                ? MediaDescription : !string.IsNullOrWhiteSpace(MediaTitle)
                    ? MediaTitle : string.Empty;

    // public string RelativeUrl
    // {
    //     get
    //     {
    //         return UrlHelper.ContentUrl(this.ImageFile?.Url ?? string.Empty);
    //     }
    // }

    // public string CanonicalUrl(HttpContext context)
    //     => RelativeUrl.ToCanonicalUrl(context);
}
