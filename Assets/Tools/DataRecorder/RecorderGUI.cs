using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class RecorderGUI : MonoBehaviour
{
    private readonly WaitForSeconds _fiveMs = new WaitForSeconds(0.05f);

    public enum Finger
    {
        Thumb,
        Index,
        Middle,
        Ring,
        Pinky
    }

    public enum State
    {
        Idle,
        Recording
    }

    [SerializeField] private Finger currentFinger;
    [SerializeField] private State state;
    [SerializeField] private string fileName = "data.txt";
    [SerializeField] private string userId = "1";

    private void OnGUI()
    {
        float width = Screen.width / 3f;
        GUILayout.BeginArea(new Rect(Screen.width / 3f, 0, width, 1000));
        GUILayout.Label($"Is Recording: {Recorder.IsRecording}");
        GUILayout.Label($"Finger: {currentFinger}");
        GUILayout.Label($"State: {state}");
        GUILayout.Space(20);

        userId = GUILayout.TextField(userId);
        GUILayout.EndArea();
    }

    private void Start()
    {
        StartCoroutine(ReaderLoop());
    }

    private IEnumerator ReaderLoop()
    {
        while (true)
        {
            if (Recorder.IsRecording && Input.GetKey(KeyCode.Space))
            {
                state = State.Recording;
                float x = Random.Range(-1f, 1f);
                float y = Random.Range(-100f, 100f);
                float z = Random.Range(-1000f, 10000f);
                float w = Random.Range(-10000f, 100000f);

                Recorder.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6}",
                    userId,
                    currentFinger,
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    x,
                    y,
                    z,
                    w));
            }
            else
            {
                state = State.Idle;
            }

            yield return _fiveMs;
        }
    }

    private void OnApplicationQuit()
    {
        Recorder.Stop();
    }

    private void Update()
    {
        if (!Recorder.IsRecording && Input.GetKeyDown(KeyCode.Return))
        {
            Recorder.FileName = fileName;
            Recorder.Start();
        }

        if (Recorder.IsRecording && Input.GetKeyDown(KeyCode.Escape))
        {
            Recorder.Stop();
        }

        HandleFingers();

        if (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.Period))
        {
            var id = Convert.ToInt32(userId);
            userId = (++id).ToString();
        }
    }

    private void HandleFingers()
    {
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.H)) currentFinger = Finger.Thumb;
        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.U)) currentFinger = Finger.Index;
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.I)) currentFinger = Finger.Middle;
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.O)) currentFinger = Finger.Ring;
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.P)) currentFinger = Finger.Pinky;
    }
}