using UnityEngine;

public class OneShotAudio : MonoBehaviour
{
    private AudioSource source;
    private bool stopOnPause;

    public static void Play(AudioClip clip, Vector3 position, float volume = 0.7f, bool spatial = true, bool persist = false, bool gameplay = true)
    {
        if (clip == null || volume <= 0f) return;
        var sound = new GameObject("Sound_" + clip.name);
        sound.transform.position = position;
        var playback = sound.AddComponent<OneShotAudio>();
        playback.stopOnPause = gameplay;
        playback.source = sound.AddComponent<AudioSource>();
        playback.source.playOnAwake = false;
        playback.source.clip = clip;
        playback.source.volume = volume;
        playback.source.spatialBlend = spatial ? 1f : 0f;
        playback.source.rolloffMode = AudioRolloffMode.Linear;
        playback.source.minDistance = 1.5f;
        playback.source.maxDistance = 25f;
        playback.source.dopplerLevel = 0f;
        playback.source.priority = spatial ? 160 : 64;
        if (persist) DontDestroyOnLoad(sound);
        playback.source.Play();
    }

    private void Update()
    {
        if (source == null || !source.isPlaying || (stopOnPause && Time.timeScale == 0f))
            Destroy(gameObject);
    }
}
