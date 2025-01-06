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

    // JSON 문자열 압축
    public static byte[] CompressJson(string json)
    {
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

        using (MemoryStream memoryStream = new MemoryStream())
        {
            // GZipStream의 생성 시 GzipHeader 설정을 명시적으로 하지 않지만, 문제 발생 시 추가할 수 있음
            using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
            {
                gzipStream.Write(jsonBytes, 0, jsonBytes.Length);
            } // GZipStream은 자동으로 Close 호출

            // 압축된 데이터를 메모리 스트림에서 반환
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
            Debug.LogError($"Error during decompression: {ex.Message}");
            return string.Empty;
        }
    }


    // 압축된 JSON의 크기 (바이트 단위)
    public static long GetCompressedSize(string json)
    {
        byte[] compressedData = CompressJson(json);
        Debug.Log($"Compressed Data: {BitConverter.ToString(compressedData)}");
        return compressedData.Length;
    }

    // JSON 크기 (압축 전 크기)
    public static long GetOriginalSize(string json)
    {
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        return jsonBytes.Length;
    }


}

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
            Console.WriteLine($"Error during decompression: {ex.Message}");
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