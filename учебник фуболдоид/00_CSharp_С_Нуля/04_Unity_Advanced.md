# ШАГИ 61-75: UNITY — ПРОДВИНУТЫЕ ТЕХНОЛОГИИ

---

## 🚀 УРОВЕНЬ 4: ПРОДВИНУТЫЕ ТЕХНОЛОГИИ UNITY

### Шаг 61: ScriptableObject — создание

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "BallSettings", menuName = "Futboloid/Ball Settings")]
public class BallSettings : ScriptableObject
{
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float fireSpeedThreshold = 10f;
    [SerializeField] private int fireExtraDamage = 2;
    
    public float BaseSpeed => baseSpeed;
    public float FireSpeedThreshold => fireSpeedThreshold;
    public int FireExtraDamage => fireExtraDamage;
}
```

**Почему `[CreateAssetMenu]`?**
- Добавляет пункт в меню Create
- Позволяет создавать Asset'ы через редактор
- `menuName` — имя в меню

**Задание:** Создай `DefenderSettings` с настройками защитника.

---

### Шаг 62: ScriptableObject — использование

```csharp
using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] private BallSettings settings;
    
    private void Start()
    {
        Debug.Log($"Базовая скорость: {settings.BaseSpeed}");
        Debug.Log($"Порог огня: {settings.FireSpeedThreshold}");
    }
}
```

**Почему `[SerializeField]` для ScriptableObject?**
- Поле становится видимым в Inspector
- Можно перетащить Asset из проекта
- Не создаёт копию, а ссылается на оригинал

**Задание:** Подключи `BallSettings` к GameObject.

---

### Шаг 63: AudioMixer — создание

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

**Почему `VolumeToDb`?**
- `AudioMixer` работает с dB (децибелами)
- `linear` — от 0 до 1
- `dB` — от -80 до 0
- Нужно конвертировать

**Задание:** Создай AudioMixer и настрой параметры.

---

### Шаг 64: AudioMixer — группы

```csharp
// Структура AudioMixer
// AudioMixer
// ├── Music (экспозируемый параметр: MusicVolume)
// │   └── Traks (музыка матча)
// ├── SFX (экспозируемый параметр: SfxVolume)
// │   ├── ball wall (удары о стену)
// │   ├── ball (удары о защитников)
// │   ├── Crown_Scored (гол забит)
// │   ├── Crown_Conceded (гол пропущен)
// │   ├── start (свисток, конец матча)
// │   ├── perc (перки, бонусы)
// │   ├── LevelUp (level up)
// │   └── Defender_destroy (защитник уничтожен)
```

**Почему группы в AudioMixer?**
- Группирует звуки по типу
- Позволяет управлять громкостью группы
- Экспозируемые параметры — доступны из кода

**Задание:** Создай группы в AudioMixer.

---

### Шаг 65: AudioMixer — экспозируемые параметры

```csharp
// В AudioMixer:
// 1. Создаём параметр MusicVolume
// 2. Делаем его экспозируемым (экспортируемым)
// 3. Теперь доступен из кода через mixer.SetFloat()

// В коде:
mixer.SetFloat("MusicVolume", VolumeToDb(0.5f));
// Устанавливает громкость музыки на 50%
```

**Почему экспозируемые параметры?**
- Доступны из кода через `SetFloat()`
- Можно менять в реальном времени
- Позволяют создавать UI для настроек

**Задание:** Добавь экспозируемый параметр SfxVolume.

---

### Шаг 66: Animation — анимация

```csharp
using UnityEngine;

public class BallAnimation : MonoBehaviour
{
    private Animator _animator;
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    
    private void Update()
    {
        // Управление анимацией
        _animator.SetFloat("Speed", Mathf.Abs(rb.velocity.magnitude));
        _animator.SetBool("IsOnFire", _speed > 10f);
        _animator.SetTrigger("Hit");
    }
}
```

**Почему `Animator`?**
- Управляет анимациями
- Позволяет создавать состояния
- Связывает анимацию с параметрами

**Задание:** Создай Animator для мяча.

---

### Шаг 67: Animation — Animator Controller

```csharp
// Animator Controller — файл состояния
// Animator Controller
// ├── Idle (покой)
// ├── Move (движение)
// ├── Hit (удар)
// └── Fire (горит)

// Параметры:
// - Speed (float) — скорость
// - IsOnFire (bool) — горит ли
// - Hit (trigger) — триггер удара
```

**Почему Animator Controller?**
- Определяет состояния анимации
- Позволяет переключаться между состояниями
- Визуально настраивается в редакторе

**Задание:** Создай Animator Controller для мяча.

---

### Шаг 68: Coroutines — основы

```csharp
using UnityEngine;
using System.Collections;

public class BallCoroutine : MonoBehaviour
{
    private void Start()
    {
        // Запуск корутины
        StartCoroutine(DelayedAction());
    }
    
    private IEnumerator DelayedAction()
    {
        Debug.Log("Начало");
        yield return new WaitForSeconds(2f); // Пауза 2 секунды
        Debug.Log("Конец");
    }
}
```

**Почему `IEnumerator`?**
- Позволяет делать паузы в коде
- `yield return` — пауза до следующего кадра
- Не блокирует поток

**Задание:** Создай корутину для повторного удара мяча.

---

### Шаг 69: Coroutines — использование

```csharp
using UnityEngine;
using System.Collections;

public class BallCoroutine : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(SpawnBalls());
    }
    
    private IEnumerator SpawnBalls()
    {
        for (int i = 0; i < 5; i++)
        {
            Instantiate(ballPrefab, transform.position, Quaternion.identity);
            yield return new WaitForSeconds(0.5f); // Пауза 0.5 секунды
        }
    }
}
```

**Почему `for` с `WaitForSeconds`?**
- Создаёт задержку между спавнами
- Не блокирует поток
- Позволяет делать последовательные действия

**Задание:** Создай корутину для последовательного появления защитников.

---

### Шаг 70: Coroutines — отмена

```csharp
using UnityEngine;
using System.Collections;

public class BallCoroutine : MonoBehaviour
{
    private Coroutine _currentCoroutine;
    
    private void Start()
    {
        _currentCoroutine = StartCoroutine(SpawnBalls());
    }
    
    private void StopButton()
    {
        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
            _currentCoroutine = null;
        }
    }
    
    private IEnumerator SpawnBalls()
    {
        for (int i = 0; i < 5; i++)
        {
            Instantiate(ballPrefab, transform.position, Quaternion.identity);
            yield return new WaitForSeconds(0.5f);
        }
    }
}
```

**Почему нужно отменять корутины?**
- Предотвращает утечки памяти
- Останавливает выполнение после уничтожения объекта
- Позволяет управлять длительностью

**Задание:** Добавь кнопку для отмены корутины.

---

### Шаг 71: Prefabs — создание

```csharp
// Создание prefab'а:
// 1. Создаём GameObject с компонентами
// 2. Перетаскиваем в папку Prefabs
// 3. Удаляем из сцены
// 4. Перетаскиваем обратно для создания экземпляра
```

**Почему prefab'ы?**
- Позволяют переиспользовать GameObject'ы
- Изменения в prefab'е применяются ко всем экземплярам
- Экономит время

**Задание:** Создай prefab для мяча.

---

### Шаг 72: Prefabs — Instantiate

```csharp
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    [SerializeField] private GameObject ballPrefab;
    
    private void SpawnBall()
    {
        // Создание экземпляра prefab'а
        GameObject ball = Instantiate(ballPrefab, transform.position, Quaternion.identity);
        
        // Настройка экземпляра
        Ball ballScript = ball.GetComponent<Ball>();
        ballScript.SetSpeed(10f);
    }
}
```

**Почему `Instantiate`?**
- Создаёт новый GameObject из prefab'а
- Позволяет настраивать экземпляр
- Возвращает ссылку на созданный объект

**Задание:** Создай спавнер мячей.

---

### Шаг 73: Prefabs — Destroy

```csharp
using UnityEngine;

public class BallDestroyer : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Уничтожение GameObject
        Destroy(collision.gameObject);
        
        // Уничтожение с задержкой
        Destroy(gameObject, 2f);
    }
}
```

**Почему `Destroy`?**
- Удаляет GameObject из сцены
- Можно добавить задержку
- Освобождает память

**Задание:** Добавь уничтожение мяча при столкновении.

---

### Шаг 74: GameObject — поиск

```csharp
using UnityEngine;

public class BallFinder : MonoBehaviour
{
    private void Start()
    {
        // Поиск по имени
        GameObject ball = GameObject.Find("Ball");
        
        // Поиск по тегу
        GameObject player = GameObject.FindWithTag("Player");
        
        // Поиск по компоненту
        Ball ballScript = GetComponent<Ball>();
        
        // Поиск по всем компонентам
        Ball[] allBalls = FindObjectsOfType<Ball>();
    }
}
```

**Почему `FindWithTag`?**
- Быстрее, чем `Find`
- Не ищет по имени
- Требует установки тега

**Задание:** Найди всех защитников по тегу.

---

### Шаг 75: GameObject — SetActive

```csharp
using UnityEngine;

public class BallVisibility : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Включение/выключение GameObject
        gameObject.SetActive(true);   // Включить
        gameObject.SetActive(false);  // Выключить
        
        // Включение/выключение компонента
        GetComponent<Renderer>().enabled = false;
    }
}
```

**Почему `SetActive`?**
- Включает/выключает GameObject
- Не удаляет объект, а скрывает
- Позволяет переиспользовать

**Задание:** Скрывай мяч при столкновении.

---

*← [[00_CSharp_С_Нуля/04_Unity_Basics]] | [[06_Футболоид|Шаги 76-90 →]]*
