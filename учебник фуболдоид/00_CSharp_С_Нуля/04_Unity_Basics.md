# ШАГИ 46-60: UNITY — БАЗОВЫЕ ТЕХНОЛОГИИ

---

## 🎮 УРОВЕНЬ 3: ОСНОВЫ UNITY

### Шаг 46: MonoBehaviour — основа всего

```csharp
using UnityEngine;

public class Ball : MonoBehaviour
{
    // Методы, вызываемые Unity
    private void Awake()
    {
        Debug.Log("Awake: Инициализация");
    }
    
    private void Start()
    {
        Debug.Log("Start: Старт");
    }
    
    private void Update()
    {
        Debug.Log("Update: Каждый кадр");
    }
    
    private void FixedUpdate()
    {
        Debug.Log("FixedUpdate: Физика");
    }
}
```

**Порядок вызова методов:**

| Метод | Когда вызывается |
|-------|-----------------|
| `Awake()` | При загрузке сцены |
| `Start()` | Перед первым кадром |
| `FixedUpdate()` | Каждый физический кадр |
| `Update()` | Каждый кадр |
| `LateUpdate()` | После Update |
| `OnDestroy()` | При удалении объекта |

**Почему `MonoBehaviour`?**
- Базовый класс для всех Unity-скриптов
- Позволяет использовать Unity API
- Добавляется на GameObject

**Задание:** Создай скрипт `Ball.cs` и добавь на GameObject.

---

### Шаг 47: Transform — позиция, вращение, масштаб

```csharp
using UnityEngine;

public class Ball : MonoBehaviour
{
    private void Update()
    {
        // Позиция
        transform.position = new Vector3(0, 0, 0);
        
        // Вращение
        transform.rotation = Quaternion.identity;
        
        // Масштаб
        transform.localScale = Vector3.one;
        
        // Движение
        transform.Translate(Vector3.forward * Time.deltaTime);
    }
}
```

**Почему `transform`?**
- `transform` — компонент трансформации
- Содержит позицию, вращение, масштаб
- Изменяет GameObject

**Задание:** Сделай так, чтобы мяч вращался вокруг оси Y.

---

### Шаг 48: Rigidbody — физика

```csharp
using UnityEngine;

public class BallPhysics : MonoBehaviour
{
    private Rigidbody _rb;
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    
    private void FixedUpdate()
    {
        // Применение силы
        _rb.AddForce(transform.forward * 10f);
        
        // Ограничение скорости
        if (_rb.velocity.magnitude > 20f)
        {
            _rb.velocity = _rb.velocity.normalized * 20f;
        }
    }
}
```

**Почему `Rigidbody`?**
- Позволяет применять физические силы
- Обрабатывает столкновения
- Работает с `FixedUpdate`

**Задание:** Добавь `Rigidbody` на GameObject и настрой mass.

---

### Шаг 49: Collider — столкновения

```csharp
using UnityEngine;

public class BallCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Столкновение: {collision.gameObject.name}");
        
        // Получаем информацию о столкновении
        ContactPoint contact = collision.contacts[0];
        Debug.Log($"Точка: {contact.point}");
        Debug.Log($"Нормаль: {contact.normal}");
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Вход в триггер: {other.name}");
    }
}
```

**Разница между `OnCollisionEnter` и `OnTriggerEnter`:**
- `OnCollisionEnter` — для коллайдеров с `isTrigger = false`
- `OnTriggerEnter` — для коллайдеров с `isTrigger = true`
- Триггеры не вызывают физическое столкновение

**Задание:** Создай коллайдер и триггер для мяча.

---

### Шаг 50: Input — клавиатура

```csharp
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private void Update()
    {
        // Клавиатура
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Пробел нажат");
        }
        
        if (Input.GetKey(KeyCode.W))
        {
            Debug.Log("W зажата");
        }
        
        if (Input.GetKeyUp(KeyCode.A))
        {
            Debug.Log("A отпущена");
        }
    }
}
```

**Почему `GetKeyDown`, `GetKey`, `GetKeyUp`?**
- `GetKeyDown` — вызывается в кадр нажатия
- `GetKey` — вызывается пока зажата
- `GetKeyUp` — вызывается в кадр отпускания

**Задание:** Напиши управление стрелками.

---

### Шаг 51: Input — мышь

```csharp
using UnityEngine;

public class MouseInput : MonoBehaviour
{
    private void Update()
    {
        // Позиция мыши
        Vector3 mousePos = Input.mousePosition;
        Debug.Log($"Мышь: {mousePos}");
        
        // Преобразование в мировой координаты
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        Debug.Log($"Мир: {worldPos}");
        
        // Клик
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("ЛКМ нажата");
        }
        
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("ПКМ нажата");
        }
        
        // Колёсико
        float scroll = Input.mouseScrollDelta.y;
        Debug.Log($"Скролл: {scroll}");
    }
}
```

**Почему `ScreenToWorldPoint`?**
- `Input.mousePosition` — в пикселях экрана
- `ScreenToWorldPoint` — в мировых координатах
- Нужно для работы с GameObject

**Задание:** Сделай так, чтобы мяч двигался к курсору.

---

### Шаг 52: Input — джойстик

```csharp
using UnityEngine;

public class JoystickInput : MonoBehaviour
{
    private void Update()
    {
        // Горизонталь
        float horizontal = Input.GetAxis("Horizontal");
        
        // Вертикаль
        float vertical = Input.GetAxis("Vertical");
        
        // Вектор движения
        Vector2 move = new Vector2(horizontal, vertical);
        
        if (move.magnitude > 0.1f)
        {
            Debug.Log($"Движение: {move}");
        }
    }
}
```

**Почему `GetAxis`?**
- `GetAxis` — возвращает значение от -1 до 1
- Учитывает настройку пользователя
- Работает с клавиатурой и джойстиком

**Задание:** Сделай движение мяча с джойстика.

---

### Шаг 53: UI — Text

```csharp
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private Text timerText;
    
    private void Update()
    {
        UpdateScore(100);
        UpdateTimer(60f);
    }
    
    private void UpdateScore(int score)
    {
        scoreText.text = $"Счёт: {score}";
    }
    
    private void UpdateTimer(float timer)
    {
        timerText.text = $"Время: {timer:F1}";
    }
}
```

**Почему `[SerializeField]`?**
- Поле становится видимым в Inspector
- Можно перетащить компонент из сцены
- `private` — не видно в редакторе без `[SerializeField]`

**Задание:** Создай Text для отображения здоровья.

---

### Шаг 54: UI — Button

```csharp
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    
    private void Start()
    {
        // Подписка на кнопку
        startButton.onClick.AddListener(OnStartClicked);
        restartButton.onClick.AddListener(OnRestartClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }
    
    private void OnStartClicked()
    {
        Debug.Log("Старт нажат!");
    }
    
    private void OnRestartClicked()
    {
        Debug.Log("Рестарт нажат!");
    }
    
    private void OnQuitClicked()
    {
        Application.Quit();
    }
}
```

**Почему `onClick.AddListener`?**
- `onClick` — событие кнопки
- `AddListener` — добавляет обработчик
- Можно добавить несколько обработчиков

**Задание:** Создай кнопку "Пауза".

---

### Шаг 55: UI — Slider

```csharp
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    
    private void Start()
    {
        // Подписка на изменение
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxChanged);
    }
    
    private void OnMusicChanged(float value)
    {
        Debug.Log($"Музыка: {value}");
    }
    
    private void OnSfxChanged(float value)
    {
        Debug.Log($"SFX: {value}");
    }
}
```

**Почему `onValueChanged`?**
- Вызывается при изменении значения слайдера
- Позволяет реагировать на изменения
- `value` — текущее значение (0-1)

**Задание:** Сделай слайдер громкости.

---

### Шаг 56: Camera — основной ракурс

```csharp
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 10, -10);
    
    private void LateUpdate()
    {
        // Позиция камеры
        transform.position = target.position + offset;
        
        // Камера смотрит на цель
        transform.LookAt(target);
    }
}
```

**Почему `LateUpdate`?**
- Вызывается после `Update`
- Камера должна следовать за объектом после его движения
- Предотвращает джиттер

**Задание:** Сделай камеру, которая следует за мячом.

---

### Шаг 57: Scene Management — загрузка сцен

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    
    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
    
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
```

**Почему `SceneManager`?**
- Загружает новые сцены
- Перезагружает текущую сцену
- Переключается между сценами

**Задание:** Сделай кнопку "Загрузить игру".

---

### Шаг 58: Resources — загрузка ресурсов

```csharp
using UnityEngine;

public class ResourceLoader : MonoBehaviour
{
    private void Start()
    {
        // Загрузка prefab'а
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Ball");
        Instantiate(prefab);
        
        // Загрузка AudioClip
        AudioClip clip = Resources.Load<AudioClip>("Audio/BallHit");
        
        // Загрузка текстуры
        Texture2D texture = Resources.Load<Texture2D>("Textures/Logo");
    }
}
```

**Почему `Resources`?**
- Загружает файлы из папки `Resources`
- Не нужно знать путь к файлу
- Можно загружать по имени

**Задание:** Создай папку `Resources/Prefabs` и загрузи prefab.

---

### Шаг 59: Debug — отладка

```csharp
using UnityEngine;

public class DebugExample : MonoBehaviour
{
    private void Update()
    {
        // Вывод в консоль
        Debug.Log("Обычное сообщение");
        Debug.LogWarning("Предупреждение");
        Debug.LogError("Ошибка");
        
        // Вывод в играбельную сцену
        Debug.DrawLine(Vector3.zero, Vector3.forward, Color.red);
        Debug.DrawRay(transform.position, Vector3.up, Color.green);
        
        // Вывод в инспектор
        Debug.Log($"Позиция: {transform.position}");
        Debug.Log($"Скорость: {_speed}");
    }
}
```

**Почему `Debug`?**
- `Log` — обычное сообщение
- `LogWarning` — предупреждение (жёлтое)
- `LogError` — ошибка (красное)
- `DrawLine` — рисует линию в сцене

**Задание:** Добавь `Debug.Log` в методы BallMotion.

---

### Шаг 60: Time — управление временем

```csharp
using UnityEngine;

public class TimeExample : MonoBehaviour
{
    private void Update()
    {
        // Время между кадрами
        float delta = Time.deltaTime;
        Debug.Log($"Delta: {delta}");
        
        // Время без паузы
        float unscaledDelta = Time.unscaledDeltaTime;
        Debug.Log($"Unscaled: {unscaledDelta}");
        
        // Время игры
        float elapsed = Time.time;
        Debug.Log($"Elapsed: {elapsed}");
        
        // Скорость времени
        Time.timeScale = 0.5f; // Замедление
        Time.timeScale = 1f;   // Нормальная скорость
        Time.timeScale = 0f;   // Пауза
    }
}
```

**Почему `Time.timeScale`?**
- `timeScale = 1` — нормальная скорость
- `timeScale = 0` — пауза
- `timeScale = 0.5` — замедление в 2 раза

**Задание:** Сделай паузу по клавише Escape.

---

*← [[00_CSharp_С_Нуля/03_Продвинутый_CSharp]] | [[00_CSharp_С_Нуля/04_Unity_Advanced|Шаги 61-75 →]]*
