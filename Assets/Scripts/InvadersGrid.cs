using System.Collections;
using Entities;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class InvadersGrid : MonoBehaviour
{
    [SerializeField] private GameObject endScreen;
    [SerializeField] private TextMeshProUGUI waveText, scoreText, endRecap;
    [SerializeField] private Wave[] waves;
    private Coroutine _updateRoutine;
    private Wave CurrentWave => waves[_waveIndex];
    private int _waveIndex, _waveNumber, _amountKilled, _step = 1, _totalKill;

    private Spaceship[,] _invaders;

    [SerializeField] private GameObject invaderPrefab;
    private float _speed = 1;
    private int AmountAlive => TotalAmount - _amountKilled;
    private int TotalAmount => CurrentWave.rows.Length * CurrentWave.amountPerRow;
    private float PercentKilled => _amountKilled / (float) TotalAmount;

    private Transform _leftInvader, _rightInvader, _topInvader, _bottomInvader, _transform;
    private bool _stunned;

    private void Awake()
    {
        _transform = transform;
    }

    private void Start()
    {
        StartCoroutine(NewWave());
    }

    private IEnumerator NewWave()
    {
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
        UpdateBounds();
        _updateRoutine = StartCoroutine(UpdateInvadersRoutine());
        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator SpawnInvaders()
    {
        _invaders = new Spaceship[CurrentWave.amountPerRow, CurrentWave.rows.Length];
        for (var y = 0; y < CurrentWave.rows.Length; y++)
        {
            for (var x = 0; x < CurrentWave.amountPerRow; x++)
            {
                var spawnPos = new Vector3(x * CurrentWave.space.x - CurrentWave.amountPerRow + 1, y * CurrentWave.space.y - 2 * CurrentWave.rows.Length);
                var invader = Instantiate(invaderPrefab, spawnPos + _transform.position, Quaternion.identity, _transform).GetComponent<Spaceship>();
                invader.Setup(CurrentWave.rows[CurrentWave.rows.Length - 1 - y].lives, CurrentWave.rows[CurrentWave.rows.Length - 1 - y].sprites, Color.white);
                invader.OnDieEvent += OnInvaderDeath;
                _invaders[x, y] = invader;
                yield return new WaitForSeconds(.1f);
            }
        }
    }

    private IEnumerator UpdateInvadersRoutine()
    {
        while (AmountAlive > 0)
        {
            var direction = Vector3.right * (_step * CurrentWave.step.x);
            if (_leftInvader.position.x + direction.x <= 0 || _rightInvader.position.x + direction.x >= 30)
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
        if (_stunned) yield break;
        var shot = false;
        for (var y = 0; y < CurrentWave.rows.Length; y++)
        {
            for (var x = 0; x < CurrentWave.amountPerRow; x++)
            {
                var invader = _invaders[x, y];
                if (!invader.gameObject.activeSelf) continue;
                invader.transform.position += direction;
                invader.SwitchSprite();
                if (Random.value > 1f / AmountAlive || shot) continue;
                shot = true;
                invader.Shoot(Vector2.down, "Enemy");
            }

            yield return new WaitForSeconds(.05f);
        }
    }

    private IEnumerator OnInvaderDeathRoutine(Entity invader, float deathTime)
    {
        _stunned = true;
        yield return new WaitForSeconds(CurrentWave.stunTime);
        _stunned = false;
        yield return new WaitForSeconds(deathTime);
        if (_leftInvader == invader.transform || _rightInvader == invader.transform || _topInvader == invader.transform || _bottomInvader == invader.transform)
            UpdateBounds();
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitUntil(() => _bottomInvader.position.y <= 2);
        StopAllCoroutines();
        foreach (Transform child in transform)
            Destroy(child.gameObject);
        GameManager.GameState = GameManager.State.GameOver;
        endScreen.SetActive(true);
        waveText.text = "PRESS START";
        endRecap.text = string.Concat("\nTIME....00:00\n\nWAVE....", _waveNumber, "\n\nKILLS..", _totalKill, "\n\n");
        while (gameObject.activeSelf)
        {
            yield return new WaitForSeconds(1f);
            waveText.gameObject.SetActive(!waveText.gameObject.activeSelf);
        }
    }

    private void OnInvaderDeath(Entity invader, float deathTime)
    {
        StartCoroutine(OnInvaderDeathRoutine(invader, deathTime));
        _amountKilled++;
        _totalKill++;
        _speed = CurrentWave.speedCurve.Evaluate(PercentKilled);
        if (AmountAlive != 0) return;
        StopAllCoroutines();
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        StartCoroutine(NewWave());
    }

    private void UpdateBounds()
    {
        _leftInvader = _rightInvader = _topInvader = _bottomInvader = _invaders[0, 0].transform;
        foreach (var invader in _invaders)
        {
            if (!invader.gameObject.activeSelf) continue;

            if (invader.transform != _leftInvader && invader.transform.position.x <= _leftInvader.transform.position.x || !_leftInvader.gameObject.activeSelf)
                _leftInvader = invader.transform;
            if (invader.transform != _rightInvader && invader.transform.position.x >= _rightInvader.transform.position.x || !_rightInvader.gameObject.activeSelf)
                _rightInvader = invader.transform;
            if (invader.transform != _topInvader && invader.transform.position.y >= _topInvader.transform.position.y || !_topInvader.gameObject.activeSelf)
                _topInvader = invader.transform;
            if (invader.transform != _bottomInvader && invader.transform.position.y <= _bottomInvader.transform.position.y || !_bottomInvader.gameObject.activeSelf)
                _bottomInvader = invader.transform;
        }
    }

    [System.Serializable]
    private struct Wave
    {
        public Row[] rows;
        public int amountPerRow;
        public Vector2 step;
        public Vector2Int space;
        public AnimationCurve speedCurve;
        public float missileAttackRate, stunTime;
    }

    [System.Serializable]
    private struct Row
    {
        public Sprite[] sprites;
        public int lives;
    }
}