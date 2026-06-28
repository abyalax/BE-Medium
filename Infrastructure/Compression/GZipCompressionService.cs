using System.IO.Compression;

namespace Medium.Api.Infrastructure.Compression;

public class GZipCompressionService : ICompressionService
{
  public byte[] Compress(byte[] data)
  {
    using var output = new MemoryStream();
    using (var gzip = new GZipStream(output, CompressionLevel.Optimal))
    {
      gzip.Write(data, 0, data.Length);
    }
    return output.ToArray();
  }

  public byte[] Decompress(byte[] compressedData)
  {
    using var input = new MemoryStream(compressedData);
    using var gzip = new GZipStream(input, CompressionMode.Decompress);
    using var output = new MemoryStream();
    gzip.CopyTo(output);
    return output.ToArray();
  }

  public async Task<byte[]> CompressAsync(byte[] data, CancellationToken cancellationToken = default)
  {
    using var output = new MemoryStream();
    using (var gzip = new GZipStream(output, CompressionLevel.Optimal))
    {
      await gzip.WriteAsync(data, cancellationToken);
    }
    return output.ToArray();
  }

  public async Task<byte[]> DecompressAsync(byte[] compressedData, CancellationToken cancellationToken = default)
  {
    using var input = new MemoryStream(compressedData);
    using var gzip = new GZipStream(input, CompressionMode.Decompress);
    using var output = new MemoryStream();
    await gzip.CopyToAsync(output, cancellationToken);
    return output.ToArray();
  }
}