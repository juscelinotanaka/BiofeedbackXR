using TMPro;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance;
    [SerializeField] private int _score = 0;
    [SerializeField] private TextMeshPro _scoreText;

    private void Awake()
    {
        Instance = this;
    }

    public static void Score()
    {
        Instance._score++;
        Instance._scoreText.text = $"Score: {Instance._score}";
    }
}