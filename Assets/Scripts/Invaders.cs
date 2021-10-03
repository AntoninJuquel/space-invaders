using System;
using System.Collections;
using System.Linq;
using Entities;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Invaders : MonoBehaviour
{
    [SerializeField] private GameObject endScreen;
    [SerializeField] private TextMeshProUGUI waveText, scoreText, endScoreText, endRecap;
    [SerializeField] private Spaceship invader;
    [SerializeField] private Wave[] waves;
    private Spaceship[,] _invaders;

    private Transform _transform;

    private int _waveIndex, _waveNumber, _amountKilled, _step = 1, _totalKill, _score;
    private float _speed = 1, _lastShot;
    private bool _stunned;

    private Wave CurrentWave => waves[_waveIndex];
    private int AmountAlive => TotalAmount - _amountKilled;
    private int TotalAmount => CurrentWave.rows.Length * CurrentWave.amountPerRow;
    private float PercentKilled => _amountKilled / (float) TotalAmount;

    private void Awake()
    {
        _transform = transform;
    }

    private void Start()
    {
        GameManager.OnGameOver += OnGameOver;
        StartCoroutine(NewWave());
    }

    private bool CheckBounds(Vector2 direction) => _invaders.Cast<Spaceship>().Where(i => i.gameObject.activeSelf).Any(i =>
    {
        Vector3 position;
        return (position = i.transform.position).x + direction.x <= 0 || position.x + direction.x >= 30;
    });

    private IEnumerator NewWave()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
        _stunned = false;
        _waveNumber++;
        _waveIndex = Random.Range(0, waves.Length);
        _amountKilled = 0;
        _speed = CurrentWave.speedCurve.Evaluate(PercentKilled);
        _step = 1;
        waveText.text = string.Concat("WAVE:", _waveNumber);
        waveText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        waveText.gameObject.SetActive(false);
        yield return SpawnInvaders();
        StartCoroutine(UpdateInvadersRoutine());
    }

    private IEnumerator SpawnInvaders()
    {
        _invaders = new Spaceship[CurrentWave.amountPerRow, CurrentWave.rows.Length];
        for (var y = 0; y < CurrentWave.rows.Length; y++)
        {
            for (var x = 0; x < CurrentWave.amountPerRow; x++)
            {
                var spawnPos = new Vector3(x * CurrentWave.space.x - CurrentWave.amountPerRow + 1, y * CurrentWave.space.y - 2 * CurrentWave.rows.Length);
                var newInvader = Instantiate(this.invader, spawnPos + _transform.position, Quaternion.identity, _transform);
                var row = CurrentWave.rows[CurrentWave.rows.Length - 1 - y];
                newInvader.Setup(row.lives, row.sprites, Color.white, row.points);
                newInvader.OnDieEvent += OnInvaderDeath;
                _invaders[x, y] = newInvader;
                yield return new WaitForSeconds(.05f);
            }
        }
    }

    private IEnumerator UpdateInvadersRoutine()
    {
        while (AmountAlive > 0)
        {
            if (_stunned)
            {
                yield return new WaitForSeconds(CurrentWave.stunTime);
                _stunned = false;
            }

            var direction = Vector3.right * (_step * CurrentWave.step.x);
            if (CheckBounds(direction))
            {
                _step *= -1;
                yield return StartCoroutine(UpdateInvadersPosition(Vector3.down * Mathf.Abs(CurrentWave.step.y)));
                direction = Vector3.right * (_step * CurrentWave.step.x);
            }

            StartCoroutine(UpdateInvadersPosition(direction));
            yield return new WaitForSeconds(1f / _speed);
        }
    }

    private IEnumerator UpdateInvadersPosition(Vector3 direction)
    {
        var shot = false;
        for (var y = 0; y < CurrentWave.rows.Length; y++)
        {
            for (var x = 0; x < CurrentWave.amountPerRow; x++)
            {
                var invader = _invaders[x, y];
                if (!invader.gameObject.activeSelf) continue;
                invader.transform.position += direction;
                invader.SwitchSprite();
                if (Random.value > 1f / AmountAlive || shot || Time.time - _lastShot < 1f / CurrentWave.shotRate) continue;
                shot = true;
                _lastShot = Time.time;
                invader.Shoot(Vector2.down, "Enemy");
            }

            yield return new WaitForSeconds(.05f);
        }
    }

    private void ToggleWaveText() => waveText.gameObject.SetActive(!waveText.gameObject.activeSelf);

    private void OnGameOver()
    {
        StopAllCoroutines();
        foreach (Transform child in transform)
            Destroy(child.gameObject);
        endScreen.SetActive(true);
        waveText.text = "PRESS START";
        endScoreText.text = scoreText.text;
        endRecap.text = string.Concat("\nTIME....", ((Time.time / 60) % 60).ToString("00"), ":", (Time.time % 60).ToString("00"), "\n\nWAVE....", _waveNumber, "\n\nKILLS..", _totalKill, "\n\n");
        InvokeRepeating(nameof(ToggleWaveText), 1f, 1f);
    }

    private void OnInvaderDeath(Entity invader, int points)
    {
        _amountKilled++;
        _totalKill++;
        _score += points;
        scoreText.text = _score.ToString("0000");
        if (AmountAlive > 0)
        {
            _stunned = true;
            _speed = CurrentWave.speedCurve.Evaluate(PercentKilled);
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(NewWave());
        }
    }

    private void OnDisable()
    {
        GameManager.OnGameOver -= OnGameOver;
    }

    [System.Serializable]
    private struct Wave
    {
        public Row[] rows;
        public int amountPerRow;
        public Vector2 step;
        public Vector2Int space;
        public AnimationCurve speedCurve;
        public float shotRate, stunTime;
    }

    [System.Serializable]
    private struct Row
    {
        public Sprite[] sprites;
        public int lives, points;
    }
}