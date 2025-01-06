using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Text;
using UnityEngine;
using System;

public class JsonCompressionManager : MonoBehaviour
{
    public TransformManager transferFunction;

    // JSON ���ڿ� ����
    public static byte[] CompressJson(string json)
    {
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

        using (MemoryStream memoryStream = new MemoryStream())
        {
            // GZipStream�� ���� �� GzipHeader ������ ��������� ���� ������, ���� �߻� �� �߰��� �� ����
            using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
            {
                gzipStream.Write(jsonBytes, 0, jsonBytes.Length);
            } // GZipStream�� �ڵ����� Close ȣ��

            // ����� �����͸� �޸� ��Ʈ������ ��ȯ
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

                if (decompressedData.Length == 0)
                {
                    return string.Empty;
                }

                return Encoding.UTF8.GetString(decompressedData);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error during decompression: {ex.Message}");
            return string.Empty;
        }
    }


    // ����� JSON�� ũ�� (����Ʈ ����)
    public static long GetCompressedSize(string json)
    {
        byte[] compressedData = CompressJson(json);
        Debug.Log($"Compressed Data: {BitConverter.ToString(compressedData)}");
        return compressedData.Length;
    }

    // JSON ũ�� (���� �� ũ��)
    public static long GetOriginalSize(string json)
    {
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        return jsonBytes.Length;
    }


}

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
            Console.WriteLine($"Error during decompression: {ex.Message}");
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