using System;
using Entities;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerSpaceship player;
    [SerializeField] private Invaders invaders;
    [SerializeField] private TextMeshProUGUI pauseText;
    [SerializeField] private GameObject bunkerPrefab;
    private Camera _camera;
    private Bounds _gameBounds;
    public static State GameState;
    public static event Action OnGameOver;

    private void Awake()
    {
        _camera = Camera.main;
        _gameBounds = _camera.OrthographicBounds();
    }

    private void Start()
    {
        SpawnPlayer();
        SpawnBunkers(4);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
        switch (GameState)
        {
            case State.Paused:
                if (Input.GetKeyDown(KeyCode.P))
                {
                    pauseText.gameObject.SetActive(false);
                    GameState = State.Playing;
                    Time.timeScale = 1;
                }

                break;
            case State.Playing:
                if (Input.GetKeyDown(KeyCode.P))
                {
                    pauseText.text = "PAUSED";
                    pauseText.gameObject.SetActive(true);
                    GameState = State.Paused;
                    Time.timeScale = 0;
                }

                break;
            case State.GameOver:
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    GameState = State.Playing;
                    SceneManager.LoadScene("GameScene");
                }

                break;
            default:
                break;
        }
    }

    private void SpawnPlayer()
    {
        var spawnPos = new Vector2(_gameBounds.size.x / 2, _gameBounds.size.y / 15);
        player = Instantiate(player, spawnPos, Quaternion.identity);
    }

    private void SpawnBunkers(int amount)
    {
        var space = _gameBounds.size.x / amount;
        var baseX = space / 2;

        for (var i = 0; i < amount; i++)
        {
            var spawnPos = new Vector2(baseX + (space * i), _gameBounds.size.y / 6);
            Instantiate(bunkerPrefab, spawnPos, Quaternion.identity);
        }
    }

    public static void GameOver()
    {
        GameState = State.GameOver;
        OnGameOver?.Invoke();
    }

    public enum State
    {
        Playing,
        Paused,
        GameOver
    }
}