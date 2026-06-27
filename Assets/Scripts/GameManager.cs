using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public enum GameState { None, Countdown, Playing, GameOver }
    public static GameManager Instance { get; private set; }

    public GameState currentState => _state;

    [Header("Timer Settings")]
    [SerializeField] private int countdownTime = 3;
    [SerializeField] private int matchTime = 90;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("UI Scores")]
    [SerializeField] private TextMeshProUGUI scorePlayerText;
    [SerializeField] private TextMeshProUGUI scoreEnemyText;

    private int _scorePlayer;
    private int _scoreEnemy;
    private int _remainingTime;
    private GameState _state;
    private Ball _ball;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        _ball = FindObjectOfType<Ball>();
        StartCountdown();
    }

    public void StartMatch()
    {
        // Вызывается из MainMenu при нажатии Play
        SceneManager.LoadScene("SampleScene");
    }

    void StartCountdown()
    {
        _state = GameState.Countdown;
        _remainingTime = countdownTime;
        UpdateTimerDisplay();
    }

    private float _timerAccumulator = 0f;

    void Update()
    {
        if (_state == GameState.None || _state == GameState.GameOver) return;

        _timerAccumulator += Time.deltaTime;
        if (_timerAccumulator >= 1f)
        {
            _timerAccumulator -= 1f;
            _remainingTime--;
            UpdateTimerDisplay();

            if (_state == GameState.Countdown && _remainingTime <= 0)
            {
                StartMatchTimer();
            }
            else if (_state == GameState.Playing && _remainingTime <= 0)
            {
                EndMatch();
            }
        }
    }

    void StartMatchTimer()
    {
        _state = GameState.Playing;
        _remainingTime = matchTime;
    }

    public void EndMatch()
    {
        _state = GameState.GameOver;
        if (_ball != null)
        {
            _ball.enabled = false;
        }
        timerText.text = "Match over";
    }

    void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        if (_state == GameState.Countdown)
        {
            timerText.text = _remainingTime.ToString();
        }
        else if (_state == GameState.Playing)
        {
            timerText.text = _remainingTime.ToString();
        }
    }
    public void RecordGoal(bool isEnemyGoal)
    {
        if (isEnemyGoal) _scoreEnemy++;
        else _scorePlayer++;
        UpdateScoreDisplay();
    }

    void UpdateScoreDisplay()
    {
        if (scorePlayerText != null) scorePlayerText.text = _scorePlayer.ToString();
        if (scoreEnemyText != null) scoreEnemyText.text = _scoreEnemy.ToString();
    }
}
