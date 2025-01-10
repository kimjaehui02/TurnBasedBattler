using System.IO.Compression;
using System.IO;
using System.Text;
using UnityEngine;
using System;

// [ExtensionOfNativeClass] 이 어트리뷰트를 추가해줍니다.

public class CompressionManager
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
                return decompressedData.Length == 0 ? string.Empty : Encoding.UTF8.GetString(decompressedData);
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"Error during decompression: {ex.Message}");
            return string.Empty;
        }
    }

    // 압축된 JSON의 크기 (바이트 단위)
    public static long GetCompressedSize(string json)
    {
        byte[] compressedData = CompressJson(json);
        return compressedData.Length;
    }

    // JSON 크기 (압축 전 크기)
    public static long GetOriginalSize(string json)
    {
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        return jsonBytes.Length;
    }
}