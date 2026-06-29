
using System.Text.RegularExpressions;

namespace Medium.Api.Common.Utils;

public static class Utils
{
  public static string GenerateSlug(string title)
  {
    var slug = title.ToLowerInvariant();
    slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
    slug = Regex.Replace(slug, @"\s+", "-");
    slug = slug.Trim('-');
    return slug;
  }

  public static List<string> ExtractMentions(string content)
  {
    // Extract mentions like @userId or @username
    var mentions = new List<string>();
    var regex = new Regex(@"@([a-zA-Z0-9_-]+)");
    var matches = regex.Matches(content);

    foreach (Match match in matches)
    {
      mentions.Add(match.Groups[1].Value);
    }

    return mentions;
  }
}