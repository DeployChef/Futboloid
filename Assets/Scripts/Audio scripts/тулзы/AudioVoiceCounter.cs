using UnityEngine;

public class AudioVoiceCounter : MonoBehaviour
{
    public bool isLogging = true;   // галочка в инспекторе
    public float logInterval = 1f;

    private float nextLogTime;

    private void Update()
    {
        if (isLogging && Time.time >= nextLogTime)
        {
            int activeVoices = CountPlayingAudioSources();
            int totalSources = CountTotalAudioSources();
            Debug.Log($"Audio Voices: {activeVoices} playing / {totalSources} total");
            nextLogTime = Time.time + logInterval;
        }
    }

    private int CountPlayingAudioSources()
    {
        int count = 0;
        AudioSource[] sources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource src in sources)
            if (src.isPlaying) count++;
        return count;
    }

    private int CountTotalAudioSources()
    {
        return FindObjectsOfType<AudioSource>().Length;
    }
}