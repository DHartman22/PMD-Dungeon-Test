using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Credit to Mac Soulsby for creating the original code
[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 0.7f;

    [Range(0.5f, 1.5f)]
    public float pitch = 1f;

    [Range(0f, 0.5f)]
    public float pitchRand = 0.1f;

    private AudioSource source;

    public void SetSource(AudioSource _source)
    {
        source = _source;
        source.clip = clip;
    }

    public void Play()
    {
        source.volume = volume;
        source.pitch = pitch * (1 + Random.Range(-pitchRand / 2, pitchRand / 2));
        source.PlayOneShot(clip);
    }

    public void Pause()
    {
        source.Pause();
    }

    public void Play(float newPitch)
    {
        source.volume = volume;
        source.pitch = newPitch;
        source.PlayOneShot(clip);
    }

    public void Play(Vector3 location)
    {
        source.volume = volume;
        source.pitch = pitch * (1 + Random.Range(-pitchRand / 2, pitchRand / 2)); ;
        //source.PlayOneShot(clip);

        AudioSource.PlayClipAtPoint(clip, location, source.volume);
    }
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField]
    Sound[] sounds;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Extra Audio Manager in scene");
        }

        instance = this;
    }
    private void Start()
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            GameObject _go = new GameObject("Sound_" + i + "_" + sounds[i].name);
            sounds[i].SetSource(_go.AddComponent<AudioSource>());
        }

        GameEvents.instance.onSuccessfulPogo += () => PlaySound("PogoSuccess");

    }

    public void PlaySound(string _name)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == _name)
            {
                sounds[i].Play();
                return;
            }
        }
        Debug.LogWarning("SoundManager: Sound not found in sounds list:" + _name);
    }


    public void PlaySoundWithPitch(string _name, float newPitch)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == _name)
            {
                sounds[i].Play(newPitch);
                return;
            }
        }
        Debug.LogWarning("SoundManager: Sound not found in sounds list:" + _name);
    }

    public void PlaySoundAtLocation(string _name, Vector3 location)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == _name)
            {
                sounds[i].Play(location);
                return;
            }
        }
        Debug.LogWarning("SoundManager: Sound not found in sounds list:" + _name);
    }

    public void PauseSound(string _name)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == _name)
            {
                sounds[i].Pause();
                return;
            }
        }
        Debug.LogWarning("SoundManager: Sound not found in sounds list:" + _name);
    }


}
