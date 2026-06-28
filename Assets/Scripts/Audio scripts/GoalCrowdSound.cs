using UnityEngine;

public class GoalZone : MonoBehaviour
{
    [Header("Звуки толпы")]
    public AudioClip[] crowdSounds;

    [Header("Настройки")]
    public float cooldown = 3f;

    private AudioSource audioSource;
    private float lastPlayTime = -999f;

    void Start()
    {
        // Если на этом же объекте висит AudioSource – используем его,
        // иначе создаём новый (можно и просто заранее добавить компонент)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ball") && Time.time - lastPlayTime >= cooldown)
        {
            PlayRandomSound();
        }
    }

    void PlayRandomSound()
    {
        if (crowdSounds == null || crowdSounds.Length == 0) return;

        AudioClip clip = crowdSounds[Random.Range(0, crowdSounds.Length)];
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(clip);

        lastPlayTime = Time.time;
    }
}