using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManagerScript : SingletonScript<AudioManagerScript>
{

    class ClipInfo
    {
       public AudioSource source { get; set; }
       public float defaultVolume { get; set; }
    }

    // Public Variables

    // Private Variables
    private List<ClipInfo> _activeAudioList;
    private AudioSource _activeMusic;
    private AudioSource _activeVoiceOver;
    private float _volumeMod;
    private float _volumeMin;
    private bool _VOfade; 
 
    void Awake()
    {
        try
        {
            transform.parent = GameObject.FindGameObjectWithTag("MainCamera").transform;
            transform.localPosition = new Vector3(0, 0, 0);
        }
        catch
        {
            Debug.Log("Unable to find main camera to put audiomanager");
        }
    }

    void Start() {
        this._activeAudioList = new List<ClipInfo>();
        this._volumeMod = 1;
        this._volumeMin = 0.2f;
        this._VOfade = false;
        this._activeVoiceOver = null;
        this._activeMusic = null;
    }

    void Update()
    {
        if (this._VOfade && this._volumeMod >= this._volumeMin)
        {
            this._volumeMod -= 0.1f;
        }
        else if (!this._VOfade && this._volumeMod < 1.0f)
        {
            this._volumeMod += 0.1f;
        }
        this.UpdateActiveAudio();
    }

    private void SetSource(ref AudioSource source, AudioClip clip, float volume)
    {
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.dopplerLevel = 0.5f;
        source.minDistance = 2;
        source.maxDistance = 30;
        source.priority = 128;
        source.clip = clip;
        source.volume = volume;
    }

    public AudioSource PlayVoiceOver(AudioClip voiceOver, float volume)
    {
        AudioSource source = Play(voiceOver, transform, volume);

        this._VOfade = true;
        this._activeVoiceOver = source;
        this._volumeMod = 0.2f;

        return source;
    }

    public AudioSource PlayMusic(AudioClip music, float volume)
    {
        this._activeMusic = PlayLoop(music, transform, volume);
        this._activeMusic.priority = 0;
        
        return this._activeMusic;
    }

    public AudioSource Play(AudioClip clip, Vector3 soundOrigin, float volume)
    {
        if (clip == null) return null;

        GameObject soundLoc = new GameObject("Audio: " + clip.name);
        AudioSource source = soundLoc.AddComponent<AudioSource>();

        soundLoc.transform.position = soundOrigin;
        this.SetSource(ref source, clip, volume);
        this._activeAudioList.Add(new ClipInfo { source = source, defaultVolume = volume });

        source.Play();
        Destroy(soundLoc, clip.length);
        return source;
    }

    public AudioSource Play(AudioClip clip, Transform emitter, float volume)
    {
        AudioSource source = Play(clip, emitter.position, volume);

        source.transform.parent = emitter;
        return source;
    }

    public AudioSource PlayLoop(AudioClip loop, Transform emitter, float volume)
    {
        if (loop == null) return null;

        GameObject movingSoundLoc = new GameObject("Audio: " + loop.name);
        AudioSource source = movingSoundLoc.AddComponent<AudioSource>();

        movingSoundLoc.transform.position = emitter.position;
        movingSoundLoc.transform.parent = emitter;

        this.SetSource(ref source, loop, volume);
        source.loop = true;
        source.Play();

        this._activeAudioList.Add(new ClipInfo { source = source, defaultVolume = volume });
        return source;
    }

    public void StopSound(AudioSource toStop)
    {
        if (toStop == null) return;

        try
        {
            Destroy(this._activeAudioList.Find(s => s.source == toStop).source.gameObject);
        }
        catch
        {
            Debug.Log("Error trying to stop audio source " + toStop);
        }
    }

    public bool IsPlaying(AudioClip clip)
    {
        if (clip == null) return false;

        try
        {
            return this._activeAudioList.Find(s => s.source.clip == clip) == null ? false : true;
        }
        catch
        {
            return false;
        }
    }

    private void UpdateActiveAudio()
    {
        List<ClipInfo> toRemove = new List<ClipInfo>();
        try
        {
            if (!this._activeVoiceOver)
            {
                this._VOfade = false;
            }
            foreach (ClipInfo audioClip in this._activeAudioList)
            {
                if (!audioClip.source)
                {
                    toRemove.Add(audioClip);
                }
                else if (audioClip.source != this._activeVoiceOver)
                {
                    audioClip.source.volume = audioClip.defaultVolume * this._volumeMod;
                }
            }
        }
        catch
        {
            Debug.Log("Error updating active audio clips");
            return;
        }

        foreach (ClipInfo audioClip in toRemove)
        {
            this._activeAudioList.Remove(audioClip);
        }
    }

    public void PauseFX()
    {
        foreach (ClipInfo audioClip in this._activeAudioList)
        {
            try
            {
                if (audioClip.source != this._activeMusic)
                {
                    audioClip.source.Pause();
                }
            }
            catch
            {
                continue;
            }
        }
    }

    public void UnpauseFX()
    {
        foreach (ClipInfo audioClip in this._activeAudioList)
        {
            try
            {
                if (!audioClip.source.isPlaying)
                {
                    audioClip.source.Play();
                }
            }
            catch
            {
                continue;
            }
        }
    }
}
