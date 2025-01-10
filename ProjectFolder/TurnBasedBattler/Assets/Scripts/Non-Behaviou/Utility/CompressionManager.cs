using System.IO.Compression;
using System.IO;
using System.Text;
using UnityEngine;
using System;

// [ExtensionOfNativeClass] �� ��Ʈ����Ʈ�� �߰����ݴϴ�.

public class CompressionManager
{
    // JSON ���ڿ� ����
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

    // JSON ���ڿ� ���� ���� (����� ����Ʈ �迭�� JSON ���ڿ��� ��ȯ)
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

    // ����� JSON�� ũ�� (����Ʈ ����)
    public static long GetCompressedSize(string json)
    {
        byte[] compressedData = CompressJson(json);
        return compressedData.Length;
    }

    // JSON ũ�� (���� �� ũ��)
    public static long GetOriginalSize(string json)
    {
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        return jsonBytes.Length;
    }
}