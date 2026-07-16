# C# С НУЛЯ — ПУТЬ ОТ НОЛЯ ДО ФУТБОЛОИДА

---

## 🎯 ДЛЯ КОГО ЭТО РУКОВОДСТВО

Если ты:
- Никогда не писал на C#
- Знаешь основы программирования (переменные, циклы)
- Хочешь писать код уровня Futboloid

Ты по дороге! Это руководство проведёт тебя от нуля до уровня, когда ты сможешь читать и писать код Futboloid.

---

## 📚 ПЛАН ОБУЧЕНИЯ

### Уровень 0: Основы (1-2 недели)

| Тема | Что изучить | Практика |
|------|-------------|----------|
| Переменные | `int`, `float`, `bool`, `string` | Создать переменные, выводить в консоль |
| Условия | `if`, `else`, `switch` | Написать калькулятор |
| Циклы | `for`, `while` | Вывести числа от 1 до 100 |
| Массивы | `int[]`, `List<T>` | Создать список чисел |
| Методы | `void`, `return`, `parameters` | Написать 5 методов |

### Уровень 1: ООП (2-3 недели)

| Тема | Что изучить | Практика |
|------|-------------|----------|
| Классы | `class`, `public`, `private` | Создать класс `Person` |
| Конструкторы | `public MyClass()`, параметры | Добавить конструктор |
| Свойства | `get`, `set`, `private set` | Создать свойства |
| Наследование | `: BaseClass` | Создать `ChildClass : ParentClass` |
| Интерфейсы | `interface`, `implements` | Создать `IMyInterface` |

### Уровень 2: Продвинутый C# (2-3 недели)

| Тема | Что изучить | Практика |
|------|-------------|----------|
| Generics | `<T>`, `where T : class` | Создать `MyList<T>` |
| Events | `event`, `EventHandler` | Создать событие |
| LINQ | `Where`, `Select`, `First` | Отфильтровать список |
| Async/Await | `async`, `await`, `Task` | Написать асинхронный метод |
| Structs | `struct`, `readonly struct` | Создать событие-структуру |

### Уровень 3: Unity (2-3 недели)

| Тема | Что изучить | Практика |
|------|-------------|----------|
| MonoBehaviour | `Start()`, `Update()`, `Awake()` | Создать скрипт для GameObject |
| Transform | `position`, `rotation`, `scale` | Двигать объект |
| Physics | `Rigidbody`, `Collider`, `OnCollisionEnter` | Создать физический объект |
| Input | `Input.GetAxis`, `Input.GetKeyDown` | Управление персонажем |
| UI | `Text`, `Button`, `Slider` | Создать интерфейс |

### Уровень 4: Футболроид (2-3 недели)

| Тема | Что изучить | Практика |
|------|-------------|----------|
| DI (VContainer) | `LifetimeScope`, `Register` | Зарегистрировать сервис |
| Event Bus | `Publish`, `Subscribe` | Создать шину событий |
| ScriptableObject | `CreateAssetMenu`, `[SerializeField]` | Создать настройки |
| AudioMixer | `SetFloat`, `AudioMixerGroup` | Управление звуком |
| UniTask | `UniTask`, `async/await` | Асинхронные операции |

---

## 📖 ПОШАГОВЫЙ ПУТЬ

### Шаг 1: Первая программа

```csharp
using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("Привет, Футболроид!");
    }
}
```

**Что здесь происходит:**
- `using System;` — подключаем библиотеку System
- `class Program` — определяем класс
- `static void Main()` — точка входа в программу
- `Console.WriteLine()` — выводим текст

**Задание:** Выведи своё имя и возраст.

---

### Шаг 2: Переменные

```csharp
int age = 25;              // целое число
float score = 95.5f;       // дробное число
bool isPlayer = true;      // логическое значение
string playerName = "Алекс"; // строка

Console.WriteLine($"Игрок: {playerName}, Возраст: {age}, Счёт: {score}");
```

**Почему `f` после `95.5`?**
- `float` — 4 байта, меньше точность
- `double` — 8 байт, больше точность
- В Unity чаще используют `float`

**Задание:** Создай переменные для мяча: `position`, `speed`, `isOnFire`.

---

### Шаг 3: Условия

```csharp
int score = 100;

if (score >= 100)
{
    Console.WriteLine("Победа!");
}
else if (score >= 50)
{
    Console.WriteLine("Хорошо!");
}
else
{
    Console.WriteLine("Попробуй ещё!");
}
```

**switch вместо if-else:**

```csharp
string ballContact = "wall";

switch (ballContact)
{
    case "wall":
        Console.WriteLine("Отскок от стены");
        break;
    case "defender":
        Console.WriteLine("Отскок от защитника");
        break;
    case "goal":
        Console.WriteLine("Гол!");
        break;
    default:
        Console.WriteLine("Неизвестное столкновение");
        break;
}
```

**Задание:** Напиши программу, которая определяет тип столкновения мяча.

---

### Шаг 4: Циклы

```csharp
// for — когда знаем количество итераций
for (int i = 0; i < 10; i++)
{
    Console.WriteLine($"Итерация {i}");
}

// while — когда не знаем количество итераций
int health = 100;
while (health > 0)
{
    health -= 10;
    Console.WriteLine($"Здоровье: {health}");
}

// foreach — перебор коллекции
string[] defenders = {"Защитник1", "Защитник2", "Защитник3"};
foreach (string defender in defenders)
{
    Console.WriteLine(defender);
}
```

**Задание:** Выведи числа от 1 до 100, кратные 5.

---

### Шаг 5: Массивы и List

```csharp
// Массив — фиксированный размер
int[] scores = { 100, 200, 300 };
scores[0] = 150; // Изменяем элемент

// List — динамический размер
List<string> defenders = new List<string>();
defenders.Add("Защитник1");
defenders.Add("Защитник2");
defenders.Remove("Защитник1");

// Проверка существования
if (defenders.Contains("Защитник1"))
{
    Console.WriteLine("Найден!");
}

// Перебор
foreach (string defender in defenders)
{
    Console.WriteLine(defender);
}
```

**Задание:** Создай список мячей и добавь/удали несколько.

---

### Шаг 6: Методы

```csharp
class Ball
{
    // Метод без возврата
    public void ShowInfo()
    {
        Console.WriteLine("Мяч в игре");
    }
    
    // Метод с возвратом
    public bool IsOnFire(float speed)
    {
        return speed > 10f;
    }
    
    // Метод с параметрами
    public float CalculateDamage(float speed, int multiplier)
    {
        return speed * multiplier;
    }
    
    // Статический метод (не требует экземпляра)
    public static string GetVersion()
    {
        return "1.0";
    }
}
```

**Задание:** Напиши методы для калькулятора: `Add`, `Subtract`, `Multiply`, `Divide`.

---

### Шаг 7: Классы и объекты

```csharp
class Defender
{
    // Поля
    private string _name;
    private int _health;
    private float _speed;
    
    // Свойства (public доступ к private полям)
    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }
    
    public int Health
    {
        get { return _health; }
        private set { _health = value; } // Только для чтения извне
    }
    
    // Конструктор
    public Defender(string name, int health, float speed)
    {
        _name = name;
        _health = health;
        _speed = speed;
    }
    
    // Методы
    public void TakeDamage(int damage)
    {
        _health -= damage;
        if (_health <= 0)
        {
            Console.WriteLine($"{_name} уничтожен!");
        }
    }
    
    public void Move(float deltaTime)
    {
        // Логика движения
    }
}

// Использование
Defender defender = new Defender("Защитник1", 100, 5f);
defender.TakeDamage(50);
Console.WriteLine(defender.Name);
```

**Задание:** Создай класс `Ball` с полями `position`, `speed`, `direction`.

---

### Шаг 8: Наследование

```csharp
// Базовый класс
class Entity
{
    public string Name { get; set; }
    public int Health { get; set; }
    
    public virtual void TakeDamage(int damage)
    {
        Health -= damage;
    }
}

// Наследник
class Defender : Entity
{
    public float Speed { get; set; }
    
    // Переопределение метода
    public override void TakeDamage(int damage)
    {
        // Защитник получает меньше урона
        base.TakeDamage(damage / 2);
    }
}

class Goalkeeper : Entity
{
    public float Reflexes { get; set; }
    
    public override void TakeDamage(int damage)
    {
        // Вратарь получает ещё меньше урона
        base.TakeDamage(damage / 3);
    }
}

// Использование
Entity entity = new Defender();
entity.TakeDamage(100); // Получит 50 урона
```

**Задание:** Создай иерархию: `Vehicle` → `Car`, `Motorcycle`.

---

### Шаг 9: Интерфейсы

```csharp
// Интерфейс — контракт, который должен реализовать класс
interface IDamageable
{
    void TakeDamage(int damage);
    int Health { get; }
}

interface IMovable
{
    void Move(float deltaX, float deltaY);
    float Speed { get; set; }
}

// Класс реализует несколько интерфейсов
class Ball : IDamageable, IMovable
{
    public int Health { get; private set; }
    public float Speed { get; set; }
    
    public void TakeDamage(int damage)
    {
        Health -= damage;
    }
    
    public void Move(float deltaX, float deltaY)
    {
        // Логика движения
    }
}

class Defender : IDamageable, IMovable
{
    public int Health { get; private set; }
    public float Speed { get; set; }
    
    public void TakeDamage(int damage)
    {
        Health -= damage;
    }
    
    public void Move(float deltaX, float deltaY)
    {
        // Логика движения
    }
}
```

**Зачем интерфейсы?**
- Класс может реализовать НЕСКОЛЬКО интерфейсов
- Нельзя наследовать от нескольких классов
- Код становится гибким

**Задание:** Создай интерфейс `IScoreable` и реализуй его в `Match`.

---

### Шаг 10: Generics

```csharp
// Generics — работа с любыми типами
class MyList<T>
{
    private T[] _items;
    private int _count;
    
    public MyList(int capacity)
    {
        _items = new T[capacity];
        _count = 0;
    }
    
    public void Add(T item)
    {
        _items[_count] = item;
        _count++;
    }
    
    public T Get(int index)
    {
        return _items[index];
    }
    
    public int Count => _count;
}

// Использование
MyList<int> intList = new MyList<int>(10);
intList.Add(100);
intList.Add(200);

MyList<string> stringList = new MyList<string>(10);
stringList.Add("Hello");
stringList.Add("World");
```

**Ограничения generics:**

```csharp
// T должен быть классом
class MyRepository<T> where T : class
{
    public T Find(int id) { ... }
}

// T должен иметь конструктор без параметров
class MyFactory<T> where T : new()
{
    public T Create()
    {
        return new T();
    }
}

// T должен реализовывать интерфейс
class MyService<T> where T : IDamageable
{
    public void DamageAll(List<T> items)
    {
        foreach (var item in items)
        {
            item.TakeDamage(10);
        }
    }
}
```

**Задание:** Создай `MyDictionary<TKey, TValue>`.

---

### Шаг 11: Events

```csharp
// Событие — механизм уведомления
class Ball
{
    // Объявляем событие
    public event Action<string> OnHit;
    
    public void Hit(string surface)
    {
        // Вызываем событие
        OnHit?.Invoke(surface);
    }
}

class AudioService
{
    public AudioService(Ball ball)
    {
        // Подписываемся на событие
        ball.OnHit += OnBallHit;
    }
    
    private void OnBallHit(string surface)
    {
        Console.WriteLine($"Мяч ударился о {surface}");
    }
}

// Использование
Ball ball = new Ball();
AudioService audio = new AudioService(ball);

ball.Hit("wall"); // Выведет: Мяч ударился о wall
```

**Задание:** Создай событие `OnGoalScored` в классе `Match`.

---

### Шаг 12: LINQ

```csharp
using System.Linq;

List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

// Where — фильтрация
var evenNumbers = numbers.Where(n => n % 2 == 0);
// Результат: 2, 4, 6, 8, 10

// Select — преобразование
var doubled = numbers.Select(n => n * 2);
// Результат: 2, 4, 6, 8, 10, 12, 14, 16, 18, 20

// First — первый элемент
var first = numbers.First(n => n > 5);
// Результат: 6

// Count — количество
var count = numbers.Count(n => n > 5);
// Результат: 5

// Sort — сортировка
var sorted = numbers.OrderByDescending(n => n);
// Результат: 10, 9, 8, 7, 6, 5, 4, 3, 2, 1

// Join — объединение
List<string> names = new List<string> { "Alice", "Bob" };
List<int> ages = new List<int> { 25, 30 };
var result = names.Join(ages, n => names.IndexOf(n), a => ages.IndexOf(a), (n, a) => $"{n}: {a}");
```

**Задание:** Отфильтруй список защитников по здоровью > 50.

---

### Шаг 13: Async/Await

```csharp
using System.Threading.Tasks;

class GameLoader
{
    // Асинхронный метод
    public async Task LoadSceneAsync(string sceneName)
    {
        Console.WriteLine($"Загрузка {sceneName}...");
        
        // Имитация загрузки
        await Task.Delay(2000);
        
        Console.WriteLine($"{sceneName} загружена!");
    }
    
    // Асинхронный метод с возвратом
    public async Task<int> CalculateDamageAsync()
    {
        await Task.Delay(1000);
        return 100;
    }
}

// Использование
class Program
{
    static async Task Main()
    {
        GameLoader loader = new GameLoader();
        
        // Ожидаем завершения
        await loader.LoadSceneAsync("Game");
        
        // Асинхронный вызов
        int damage = await loader.CalculateDamageAsync();
        Console.WriteLine($"Урон: {damage}");
    }
}
```

**В Unity:**

```csharp
using UnityEngine;
using UniTask; // Плагин для Unity

class BallMotion : MonoBehaviour
{
    // UniTask — оптимизированный для Unity
    private async UniTaskVoid StartBallAsync()
    {
        Debug.Log("Запуск мяча...");
        await UniTask.Delay(1000); // Пауза 1 секунда
        Debug.Log("Мяч запущен!");
    }
    
    private void Start()
    {
        StartBallAsync().Forget(); // .Forget() — не ждём
    }
}
```

**Задание:** Напиши асинхронную загрузку ресурсов.

---

### Шаг 14: Structs (readonly)

```csharp
// Структура — лёгкий тип данных
public struct Vector2
{
    public float X { get; }
    public float Y { get; }
    
    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }
}

// readonly struct — неизменяемая структура
public readonly struct BallContactEvent
{
    public string ContactType { get; }
    public Vector2 Position { get; }
    
    public BallContactEvent(string contactType, Vector2 position)
    {
        ContactType = contactType;
        Position = position;
    }
}

// Использование
BallContactEvent event = new BallContactEvent("wall", new Vector2(1f, 2f));
// event.ContactType = "defender"; // ОШИБКА! readonly нельзя менять
```

**Почему `readonly struct`?**
- **Производительность** — в стеке, без аллокаций
- **Неизменяемость** — нельзя изменить после создания
- **Безопасность** — никто не подменит данные

**Задание:** Создай `readonly struct GoalScoredEvent`.

---

### Шаг 15: Unity — MonoBehaviour

```csharp
using UnityEngine;

public class Ball : MonoBehaviour
{
    // Поля, видимые в Inspector
    [SerializeField] private float speed = 5f;
    [SerializeField] private float radius = 0.5f;
    
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
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Вход в триггер: {other.name}");
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Столкновение: {collision.gameObject.name}");
    }
}
```

**Порядок вызова методов:**

```
Awake() → Start() → FixedUpdate() → Update() → LateUpdate() → OnGUI() → OnDestroy()
```

**Задание:** Создай скрипт, который двигает объект вперёд.

---

### Шаг 16: Unity — Physics

```csharp
using UnityEngine;

public class BallPhysics : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float kickForce = 10f;
    
    private void FixedUpdate()
    {
        // Применение силы
        rb.AddForce(transform.forward * kickForce);
        
        // Проверка скорости
        if (rb.velocity.magnitude > 20f)
        {
            rb.velocity = rb.velocity.normalized * 20f;
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Отскок
        Vector3 reflection = Vector3.Reflect(rb.velocity, collision.contacts[0].normal);
        rb.velocity = reflection;
    }
}
```

**Задание:** Создай физический мяч с отскоками.

---

### Шаг 17: Unity — Input

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
        
        // Мышь
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log($"Мышь: {mousePos}");
        }
        
        // Джойстик
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        if (horizontal != 0 || vertical != 0)
        {
            Debug.Log($"Джойстик: ({horizontal}, {vertical})");
        }
    }
}
```

**Задание:** Напиши управление персонажем.

---

### Шаг 18: Unity — UI

```csharp
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private Text timerText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Button restartButton;
    
    private void Start()
    {
        // Подписка на кнопку
        restartButton.onClick.AddListener(OnRestartClicked);
        
        // Инициализация
        UpdateScore(0);
        UpdateTimer(60f);
        UpdateHealth(100f);
    }
    
    private void UpdateScore(int score)
    {
        scoreText.text = $"Счёт: {score}";
    }
    
    private void UpdateTimer(float timer)
    {
        timerText.text = $"Время: {timer:F1}";
    }
    
    private void UpdateHealth(float health)
    {
        healthSlider.value = health / 100f;
    }
    
    private void OnRestartClicked()
    {
        Debug.Log("Рестарт!");
    }
}
```

**Задание:** Создай HUD с счётом и таймером.

---

### Шаг 19: Unity — ScriptableObject

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "BallSettings", menuName = "Futboloid/Ball Settings")]
public class BallSettings : ScriptableObject
{
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float fireSpeedThreshold = 10f;
    [SerializeField] private int fireExtraDamage = 2;
    [SerializeField] private float deceleration = 1f;
    
    public float BaseSpeed => baseSpeed;
    public float FireSpeedThreshold => fireSpeedThreshold;
    public int FireExtraDamage => fireExtraDamage;
    public float Deceleration => deceleration;
}

// Использование
public class Ball : MonoBehaviour
{
    [SerializeField] private BallSettings settings;
    
    private void Start()
    {
        Debug.Log($"Базовая скорость: {settings.BaseSpeed}");
    }
}
```

**Задание:** Создай ScriptableObject для настроек защитника.

---

### Шаг 20: Unity — AudioMixer

```csharp
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private string musicParam = "MusicVolume";
    [SerializeField] private string sfxParam = "SfxVolume";
    
    public void SetMusicVolume(float volume)
    {
        // Конвертация linear → dB
        float db = VolumeToDb(volume);
        mixer.SetFloat(musicParam, db);
    }
    
    public void SetSfxVolume(float volume)
    {
        float db = VolumeToDb(volume);
        mixer.SetFloat(sfxParam, db);
    }
    
    private static float VolumeToDb(float linear)
    {
        return linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;
    }
}
```

**Задание:** Создай управление громкостью.

---

## 📊 ИТОГОВЫЙ ПЛАН

| Уровень | Недели | Итого |
|---------|--------|-------|
| Основы | 1-2 | 2 недели |
| ООП | 2-3 | 2 недели |
| Продвинутый C# | 2-3 | 2 недели |
| Unity | 2-3 | 2 недели |
| Футболроид | 2-3 | 2 недели |
| **ВСЕГО** | | **10 недель** |

---

## 🔗 ССЫЛКИ

- [Официальная документация C#](https://docs.microsoft.com/en-us/dotnet/csharp/)
- [Unity Documentation](https://docs.unity3d.com/Manual/index.html)
- [VContainer Documentation](https://vcontainer.hadashikick.jp/)
- [UniTask Documentation](https://github.com/Cysharp/UniTask)

---

*Версия: 1.0 | Обновлено: 16.07.2026 | Проект: Futboloid*
