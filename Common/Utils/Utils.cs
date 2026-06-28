
namespace Medium.Api.Common.Utils;

public static class Utils
{
  public static string GenerateSlug(string title)
  {
    var slug = title.ToLowerInvariant();
    slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
    slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
    slug = slug.Trim('-');
    return slug;
  }
}