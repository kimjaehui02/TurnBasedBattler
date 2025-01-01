using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace UDPServer
{
    class JsonCompressionManager
    {
        // JSON 문자열 압축
        public static byte[] CompressJson(string json)
        {
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
                {
                    gzipStream.Write(jsonBytes, 0, jsonBytes.Length);
                }

                return memoryStream.ToArray();
            }
        }

        // JSON 문자열 압축 해제 (압축된 바이트 배열을 JSON 문자열로 변환)
        public static string DecompressJson(byte[] compressedData)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(compressedData))
                using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                using (MemoryStream decompressedStream = new MemoryStream())
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;

                    while ((bytesRead = gzipStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        decompressedStream.Write(buffer, 0, bytesRead);
                    }

                    byte[] decompressedData = decompressedStream.ToArray();

                    if (decompressedData.Length == 0)
                    {
                        return string.Empty;
                    }

                    return Encoding.UTF8.GetString(decompressedData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during decompression: {ex.Message}");
                return string.Empty;
            }
        }

        // 압축된 JSON의 크기 (바이트 단위)
        public long GetCompressedSize(string json)
        {
            byte[] compressedData = CompressJson(json);
            return compressedData.Length;
        }

        // JSON 크기 (압축 전 크기)
        public long GetOriginalSize(string json)
        {
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            return jsonBytes.Length;
        }

        public void test()
        {
            // 테스트할 JSON 문자열
            string originalJson = @"
            [
                {
                    ""transform"": {
                        ""position"": { ""x"": 1.5, ""y"": 2.0, ""z"": -3.0 },
                        ""rotation"": { ""x"": 0.0, ""y"": 0.7071, ""z"": 0.0, ""w"": 0.7071 },
                        ""scale"": { ""x"": 1.0, ""y"": 1.0, ""z"": 1.0 }
                    }
                },
                {
                    ""transform"": {
                        ""position"": { ""x"": 4.5, ""y"": 5.0, ""z"": -6.0 },
                        ""rotation"": { ""x"": 0.0, ""y"": 0.5, ""z"": 0.5, ""w"": 0.5 },
                        ""scale"": { ""x"": 1.0, ""y"": 1.0, ""z"": 1.0 }
                    }
                }
            ]";

            // 압축 전 크기
            long originalSize = GetOriginalSize(originalJson);
            Console.WriteLine($"Original JSON Size: {originalSize} bytes");

            // 압축 후 크기
            long compressedSize = GetCompressedSize(originalJson);
            Console.WriteLine($"Compressed JSON Size: {compressedSize} bytes");

            // 압축 비율 계산
            double compressionRatio = (double)originalSize / compressedSize;
            Console.WriteLine($"Compression Ratio: {compressionRatio:F2}");

            // JSON 압축 해제
            byte[] compressedData = CompressJson(originalJson);
            string decompressedJson = DecompressJson(compressedData);
            Console.WriteLine($"Decompressed JSON: {decompressedJson}");
        }
    }
}
