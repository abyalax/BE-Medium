namespace Medium.Api.Infrastructure.Compression;

public class CompressedMessageWrapper
{
  public bool IsCompressed { get; set; }
  public byte[] Data { get; set; } = Array.Empty<byte>();
  public string? OriginalType { get; set; }
}