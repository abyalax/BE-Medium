namespace Medium.Api.Infrastructure.Compression;

public interface ICompressionService
{
  byte[] Compress(byte[] data);
  byte[] Decompress(byte[] compressedData);
  Task<byte[]> CompressAsync(byte[] data, CancellationToken cancellationToken = default);
  Task<byte[]> DecompressAsync(byte[] compressedData, CancellationToken cancellationToken = default);
}