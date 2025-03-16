using UnityEngine;
using System.Collections;

public enum SoundType { 
    DOOR_OPEN, 
    SUPERMARKET_ANNOUCEMENT, 
    SUPERMARKET_MUSIC, 
    SUPERMARKET_SCANNER,
    BUTTON_CLICK 
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour {
    [SerializeField] private AudioClip[] soundList;
    
    private static SoundManager instance;
    private AudioSource sfxAudioSource; // For one-shot sound effects
    private AudioSource bgmAudioSource; // For background music
    
    [Range(0.1f, 5.0f)]
    [SerializeField] private float fadeInDuration = 2.0f;
    
    [Range(0.1f, 5.0f)]
    [SerializeField] private float fadeOutDuration = 2.0f;
    
    private Coroutine fadeCoroutine;
    
    public static SoundManager Instance {
        get { return instance; }
    }
    
    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Setup audio sources
            sfxAudioSource = GetComponent<AudioSource>();
            
            // Create a separate audio source for background music
            bgmAudioSource = gameObject.AddComponent<AudioSource>();
            bgmAudioSource.loop = true;
            bgmAudioSource.playOnAwake = false;
            bgmAudioSource.volume = 0f; // Start with volume at 0 for fade-in
        } else {
            Destroy(gameObject);
        }
    }
    
    public void PlaySound(SoundType soundType, float volume = 1f) {
        sfxAudioSource.PlayOneShot(soundList[(int)soundType], volume);
    }
    
    public void PlayBackgroundMusic(SoundType soundType, bool fadeIn = true) {
        AudioClip musicClip = soundList[(int)soundType];
        
        // If we're already playing this clip, don't restart
        if (bgmAudioSource.clip == musicClip && bgmAudioSource.isPlaying)
            return;
            
        // Stop any current fade
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
            
        // Set the clip
        bgmAudioSource.clip = musicClip;
        
        if (fadeIn) {
            bgmAudioSource.volume = 0f;
            bgmAudioSource.Play();
            fadeCoroutine = StartCoroutine(FadeAudioSource(bgmAudioSource, fadeInDuration, 1f));
        } else {
            bgmAudioSource.volume = 1f;
            bgmAudioSource.Play();
        }
    }
    
    public void StopBackgroundMusic(bool fadeOut = true) {
        if (!bgmAudioSource.isPlaying)
            return;
            
        // Stop any current fade
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
            
        if (fadeOut) {
            fadeCoroutine = StartCoroutine(FadeAudioSource(bgmAudioSource, fadeOutDuration, 0f, true));
        } else {
            bgmAudioSource.Stop();
        }
    }
    
    private IEnumerator FadeAudioSource(AudioSource audioSource, float duration, float targetVolume, bool stopAfterFade = false) {
        float startVolume = audioSource.volume;
        float time = 0;
        
        while (time < duration) {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            yield return null;
        }
        
        audioSource.volume = targetVolume;
        
        if (stopAfterFade && targetVolume <= 0) {
            audioSource.Stop();
        }
        
        fadeCoroutine = null;
    }

    public AudioClip GetSoundClip(SoundType soundType) {
    if ((int)soundType < soundList.Length) {
        return soundList[(int)soundType];
    }
    return null;
}
}