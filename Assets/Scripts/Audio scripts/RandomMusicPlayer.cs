using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomMusicPlayer : MonoBehaviour
{
    [Header("Музыкальные треки (перетащить сюда файлы из папки)")]
    public AudioClip[] musicClips;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Настройки для 2D-музыки (громкость не зависит от позиции)
        audioSource.spatialBlend = 0f;
        audioSource.loop = false;       // будем сами переключать треки

        if (musicClips.Length > 0)
            PlayRandomMusic();
        else
            Debug.LogWarning("Массив musicClips пуст! Добавьте треки в инспекторе.");
    }

    void Update()
    {
        // Если текущий трек закончился и массив не пуст — запускаем следующий случайный
        if (!audioSource.isPlaying && musicClips.Length > 0 && audioSource.clip != null)
        {
            PlayRandomMusic();
        }
    }

    void PlayRandomMusic()
    {
        if (musicClips.Length == 0) return;

        AudioClip clip = musicClips[Random.Range(0, musicClips.Length)];
        audioSource.clip = clip;
        audioSource.Play();

        Debug.Log("Now playing music: " + clip.name);
    }
}