using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BallSounds : MonoBehaviour
{
    [Header("Звуки ударов (из папки ball punch)")]
    public AudioClip[] impactClips;   // перетащите сюда все файлы из папки

    [Header("Настройки")]
    [Tooltip("Минимальный интервал между звуками (сек.)")]
    public float cooldown = 0.5f;     // можно уменьшить, например 0.2f

    private AudioSource audioSource;
    private float lastPlayTime = -999f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 0f; // 2D-звук – громкость не зависит от расстояния
    }

    // 2D-столкновения (работает с Rigidbody2D и Collider2D)
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Если кулдаун ещё не прошёл – выходим
        if (Time.time - lastPlayTime < cooldown)
            return;

        // Если нужно играть только при контакте с определёнными объектами, добавьте условие:
        // if (!collision.gameObject.CompareTag("Player") && !collision.gameObject.CompareTag("Wall"))
        //     return;

        PlayRandomImpactSound();
    }

    void PlayRandomImpactSound()
    {
        if (impactClips == null || impactClips.Length == 0)
        {
            Debug.LogWarning("Массив impactClips пуст! Перетащите аудиоклипы в инспекторе.");
            return;
        }

        // Случайный клип
        AudioClip clip = impactClips[Random.Range(0, impactClips.Length)];

        // Лог в консоль
        Debug.Log("Playing impact sound: " + clip.name);

        // Небольшая вариация высоты тона
        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(clip);

        // Запоминаем время проигрывания
        lastPlayTime = Time.time;
    }
}