using System.IO;
using UnityEngine;

public static class Recorder
{
    public static string FileName = "data1.txt";
    private static StreamWriter _stream;
    public static bool IsRecording { get; private set; }

    /// <summary>
    /// Open the stream and wait for the data to be read
    /// </summary>
    public static void Start()
    {
        Directory.CreateDirectory(Application.streamingAssetsPath);
        _stream = new StreamWriter(Path.Combine(Application.streamingAssetsPath, FileName), true);
        IsRecording = _stream != null;
    }

    public static void Stop()
    {
        if (IsRecording)
        {
            _stream.Close();
            IsRecording = false;
        }
    }

    public static void Write(string text)
    {
        _stream.Write(text);
    }

    public static void WriteLine(string text)
    {
        _stream.WriteLine(text);
    }
}