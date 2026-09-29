using System.Collections.Generic;
using UnityEngine;

public class SceneAudio : MonoBehaviour
{
    public static SceneAudio Instance { get; private set; }
    [Header("Música de fondo")]
    public AudioSource music;
    [Range(0f, 1f)] public float musicVolume = 0.18f;

    [Header("Configuración del nivel")]
    public bool finalLevel;

    [Header("Sonidos de los encuentros")]
    public AudioClip warningSound;
    public AudioClip clearSound;

    [Header("Sonidos de resultados")]
    public AudioClip victorySound;
    public AudioClip campaignSound;
    public AudioClip defeatSound;
    private readonly HashSet<EncounterController> encounters = new HashSet<EncounterController>();
    private bool finished;

    private void Awake() => Instance = this;

    private void Start()
    {
        if (music != null && music.clip != null)
        {
            music.volume = musicVolume;
            music.Play();
        }
    }

    private void Update()
    {
        if (music == null) return;
        float target = !finished ? musicVolume : 0f;
        music.volume = Mathf.MoveTowards(music.volume, target, Time.unscaledDeltaTime * 0.3f);
    }

    public void BeginEncounter(EncounterController encounter)
    {
        if (finished || !encounters.Add(encounter)) return;
        if (encounter.StopsPlayerDuringEncounter)
            OneShotAudio.Play(warningSound, transform.position, 0.6f, false);
    }

    public void EndEncounter(EncounterController encounter, bool completed)
    {
        if (!encounters.Remove(encounter) || finished) return;
        if (completed && encounter.StopsPlayerDuringEncounter)
            OneShotAudio.Play(clearSound, transform.position, 0.45f, false);
    }

    public void Finish(bool victory)
    {
        if (finished) return;
        finished = true;
        encounters.Clear();
        if (music != null) music.Stop();
        OneShotAudio.Play(victory ? (finalLevel ? campaignSound : victorySound) : defeatSound, transform.position, 0.7f, spatial: false, gameplay: false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
