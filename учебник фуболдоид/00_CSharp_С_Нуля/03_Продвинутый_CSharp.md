# ШАГИ 31-45: ПРОДВИНУТЫЙ C#

---

## 🚀 УРОВЕНЬ 2: ПРОДВИНУТЫЕ ТЕХНОЛОГИИ

### Шаг 31: Generics — базовое понятие

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

MyList<string> stringList = new MyList<string>(10);
stringList.Add("Hello");
```

**Почему `T`?**
- `T` — placeholder для типа
- Можно использовать любой тип
- Компилятор проверяет типы

**Задание:** Создай `MyDictionary<TKey, TValue>`.

---

### Шаг 32: Generics — ограничения

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

**Почему `where`?**
- Ограничивает типы, которые можно использовать
- Делает код безопаснее
- Позволяет использовать специфические методы

**Задание:** Создай `MyCache<T>` где `T : class`.

---

### Шаг 33: Generics в Unity

```csharp
// В Unity часто используют generics
public class EventBus<T> where T : struct
{
    private List<Action<T>> _handlers = new List<Action<T>>();
    
    public void Subscribe(Action<T> handler)
    {
        _handlers.Add(handler);
    }
    
    public void Publish(T message)
    {
        foreach (var handler in _handlers)
        {
            handler(message);
        }
    }
}

// Использование
EventBus<BallContactEvent> bus = new EventBus<BallContactEvent>();
bus.Subscribe(OnBallContact);
bus.Publish(new BallContactEvent());
```

**Почему generics в Unity?**
- Позволяют создавать гибкие системы
- EventBus, Cache, Pool — всё можно сделать generic
- Экономит код

**Задание:** Создай `EventBus<GoalScoredEvent>`.

---

### Шаг 34: Events — создание события

```csharp
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

// Использование
Ball ball = new Ball();
ball.OnHit += OnBallHit;
ball.Hit("wall");

void OnBallHit(string surface)
{
    Console.WriteLine($"Мяч ударился о {surface}");
}
```

**Почему `?.Invoke()`?**
- `?.` — если `OnHit == null`, не вызывает
- Защищает от NullReferenceException

**Задание:** Создай событие `OnGoalScored` в классе `Match`.

---

### Шаг 35: Events — подписка и отписка

```csharp
class AudioService
{
    private Ball _ball;
    
    public AudioService(Ball ball)
    {
        // Подписываемся на событие
        _ball.OnHit += OnBallHit;
    }
    
    private void OnBallHit(string surface)
    {
        Console.WriteLine($"Мяч ударился о {surface}");
    }
    
    // Метод для отписки
    public void Unsubscribe()
    {
        _ball.OnHit -= OnBallHit;
    }
}

// Использование
AudioService audio = new AudioService(ball);
audio.Unsubscribe(); // Отписываемся
```

**Почему нужно отписываться?**
- Чтобы не было утечек памяти
- Чтобы событие не вызывалось после уничтожения объекта
- `+=` — подписаться, `-=` — отписаться

**Задание:** Добавь `Unsubscribe()` в класс `AudioService`.

---

### Шаг 36: LINQ — Where (фильтрация)

```csharp
using System.Linq;

List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

// Where — фильтрация
var evenNumbers = numbers.Where(n => n % 2 == 0);
// Результат: 2, 4, 6, 8, 10

List<string> defenders = new List<string> { "Защитник1", "Вратарь", "Защитник2" };
var onlyDefenders = defenders.Where(d => d.Contains("Защитник"));
// Результат: Защитник1, Защитник2
```

**Почему `=>`?**
- `=>` — лямбда-выражение
- `n => n % 2 == 0` — "для каждого n, если n чётное"
- Анонимный метод

**Задание:** Отфильтруй защитников с здоровьем > 50.

---

### Шаг 37: LINQ — Select (преобразование)

```csharp
using System.Linq;

List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };

// Select — преобразование
var doubled = numbers.Select(n => n * 2);
// Результат: 2, 4, 6, 8, 10

List<string> defenders = new List<string> { "Защитник1", "Защитник2" };
var names = defenders.Select(d => d.ToUpper());
// Результат: ЗАЩИТНИК1, ЗАЩИТНИК2
```

**Почему `Select`?**
- Преобразует каждый элемент
- Возвращает новый список
- Не меняет исходный список

**Задание:** Преобразуй список мячей в список скоростей.

---

### Шаг 38: LINQ — First/FirstOrDefault

```csharp
using System.Linq;

List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };

// First — первый элемент
var first = numbers.First(n => n > 3);
// Результат: 4

// FirstOrDefault — первый или default
var firstOrNone = numbers.FirstOrDefault(n => n > 100);
// Результат: 0 (default для int)

List<string> defenders = new List<string> { "Защитник1", "Защитник2" };
var firstDefender = defenders.First(d => d.Contains("Защитник1"));
// Результат: Защитник1
```

**Почему `OrDefault`?**
- `First()` — выбрасывает исключение если не нашёл
- `FirstOrDefault()` — возвращает `default` если не нашёл
- Безопаснее использовать `OrDefault`

**Задание:** Найди первого защитника с именем "Защитник1".

---

### Шаг 39: LINQ — Count

```csharp
using System.Linq;

List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

// Count — количество
var count = numbers.Count(n => n > 5);
// Результат: 5

List<string> defenders = new List<string> { "Защитник1", "Защитник2", "Вратарь" };
var defenderCount = defenders.Count(d => d.Contains("Защитник"));
// Результат: 2
```

**Почему `Count`?**
- Считает элементы по условию
- Быстрее, чем `Where().Count()`
- Оптимизирован в LINQ

**Задание:** Посчитай количество мячей со скоростью > 10.

---

### Шаг 40: LINQ — Sort

```csharp
using System.Linq;

List<int> numbers = new List<int> { 5, 2, 8, 1, 9 };

// OrderBy — сортировка по возрастанию
var sorted = numbers.OrderBy(n => n);
// Результат: 1, 2, 5, 8, 9

// OrderByDescending — сортировка по убыванию
var sortedDesc = numbers.OrderByDescending(n => n);
// Результат: 9, 8, 5, 2, 1

List<string> defenders = new List<string> { "Защитник1", "Вратарь", "Защитник2" };
var sortedByName = defenders.OrderBy(d => d);
```

**Почему `OrderBy`?**
- Сортирует элементы
- Возвращает новый список
- Не меняет исходный список

**Задание:** Отсортиуй защитников по имени.

---

### Шаг 41: Async/Await — базовое понятие

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
}

// Использование
class Program
{
    static async Task Main()
    {
        GameLoader loader = new GameLoader();
        await loader.LoadSceneAsync("Game");
    }
}
```

**Почему `async/await`?**
- Не блокирует поток
- Позволяет делать тяжёлые операции без зависаний
- `await` — ждёт завершения асинхронной операции

**Задание:** Напиши асинхронную загрузку ресурсов.

---

### Шаг 42: Async/Await в Unity

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

**Почему `UniTask`, а не `Task`?**
- `Task` — создаёт аллокации в куче
- `UniTask` — оптимизирован для Unity (нет аллокаций)
- `UniTask` работает с Unity API

**Задание:** Напиши асинхронную загрузку сцены.

---

### Шаг 43: Async/Await — CancellationToken

```csharp
using System.Threading;

class GameLoader
{
    private CancellationTokenSource _cts;
    
    public async Task LoadSceneAsync(string sceneName, CancellationToken ct)
    {
        try
        {
            Console.WriteLine($"Загрузка {sceneName}...");
            await Task.Delay(2000, ct);
            Console.WriteLine($"{sceneName} загружена!");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Загрузка отменена!");
        }
    }
    
    public void CancelLoad()
    {
        _cts?.Cancel();
    }
}
```

**Почему `CancellationToken`?**
- Позволяет отменить асинхронную операцию
- Безопасно завершает загрузку
- Предотвращает утечки памяти

**Задание:** Добавь `CancellationToken` в `LoadSceneAsync`.

---

### Шаг 44: Structs — базовое понятие

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

// Использование
Vector2 position = new Vector2(1f, 2f);
float x = position.X;
float y = position.Y;
```

**Почему `struct`?**
- `struct` — в стеке, без аллокаций
- `class` — в куче, с аллокациями
- `struct` быстрее для маленьких объектов

**Задание:** Создай `struct Point2D`.

---

### Шаг 45: Structs — readonly

```csharp
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

*← [[00_CSharp_С_Нуля/02_ООП]] | [[00_CSharp_С_Нуля/04_Unity_Basics|Шаги 46-60 →]]*
