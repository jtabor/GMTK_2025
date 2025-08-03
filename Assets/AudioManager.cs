using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public AudioSource source;
    
    [Range(0f, 1f)]
    public float fxVolume;
    [Range(0f, 1f)]
    public float musicVolume = 0.25f;
    [Range(0f, 1f)]
    public float environmentVolume;
   
    public enum AudioSourceType
    {
        EFFECT,
        MUSIC,
        ENVIRONMENT
    }
    
    private float[] volumes;
    
    public List<AudioClip> backgroundMusicClips;
    private int currentMusicIndex = 0;
    private AudioSource musicSource;

    public AudioClip engineNoise;
    public AudioClip tractorNoise;
    public AudioClip blinkNoise;
    
    private AudioSource engineNoiseSource;
    private AudioSource tractorNoiseSource;
    private AudioSource blinkNoiseSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        volumes = new float[System.Enum.GetNames(typeof(AudioSourceType)).Length];
        volumes[(int) AudioSourceType.EFFECT] = fxVolume; 
        volumes[(int) AudioSourceType.MUSIC] = musicVolume; 
        volumes[(int) AudioSourceType.ENVIRONMENT] = environmentVolume; 
        
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = false;
        musicSource.volume = musicVolume;
        
        engineNoiseSource = gameObject.AddComponent<AudioSource>();
        engineNoiseSource.loop = true;
        engineNoiseSource.volume = environmentVolume;
        
        tractorNoiseSource = gameObject.AddComponent<AudioSource>();
        tractorNoiseSource.loop = true;
        tractorNoiseSource.volume = environmentVolume;
        
        blinkNoiseSource = gameObject.AddComponent<AudioSource>();
        blinkNoiseSource.loop = true;
        blinkNoiseSource.volume = fxVolume;
        
        if (backgroundMusicClips != null && backgroundMusicClips.Count > 0)
        {
            PlayNextBackgroundMusic();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (backgroundMusicClips != null && backgroundMusicClips.Count > 0 && musicSource != null && !musicSource.isPlaying)
        {
            PlayNextBackgroundMusic();
        }
    }
    
    private void PlayNextBackgroundMusic()
    {
        if (backgroundMusicClips == null || backgroundMusicClips.Count == 0)
            return;
            
        musicSource.clip = backgroundMusicClips[currentMusicIndex];
        musicSource.volume = musicVolume;
        musicSource.Play();
        
        currentMusicIndex = (currentMusicIndex + 1) % backgroundMusicClips.Count;
    }

    public void PlayEffectClip(AudioClip clip, AudioSourceType type, float volume = -1f)
    {
        float volumeToPlay = volume >= 0 ? volume : volumes[(int) type];
        source.PlayOneShot(clip, volumeToPlay);
    }
    
    public void PlayEffectClip(AudioClip clip, AudioSourceType type, Vector3 location, float volume = -1f)
    {
        float volumeToPlay = volume >= 0 ? volume : volumes[(int) type];
        AudioSource.PlayClipAtPoint(clip, location, volumeToPlay);
    }
    
    public void StartEngineNoise()
    {
        if (engineNoise != null && engineNoiseSource != null && !engineNoiseSource.isPlaying)
        {
            engineNoiseSource.clip = engineNoise;
            engineNoiseSource.Play();
        }
    }
    
    public void StopEngineNoise()
    {
        if (engineNoiseSource != null)
        {
            engineNoiseSource.Stop();
        }
    }
    
    public void StartTractorNoise()
    {
        if (tractorNoise != null && tractorNoiseSource != null && !tractorNoiseSource.isPlaying)
        {
            tractorNoiseSource.clip = tractorNoise;
            tractorNoiseSource.Play();
        }
    }
    
    public void StopTractorNoise()
    {
        if (tractorNoiseSource != null)
        {
            tractorNoiseSource.Stop();
        }
    }
    
    public void StartBlinkNoise()
    {
        if (blinkNoise != null && blinkNoiseSource != null && !blinkNoiseSource.isPlaying)
        {
            blinkNoiseSource.clip = blinkNoise;
            blinkNoiseSource.Play();
        }
    }
    
    public void StopBlinkNoise()
    {
        if (blinkNoiseSource != null)
        {
            blinkNoiseSource.Stop();
        }
    }
}
