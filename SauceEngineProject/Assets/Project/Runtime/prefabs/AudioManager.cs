using UnityEngine.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Freya;

public class AudioManager : MonoBehaviour
{
    public Sound[] playerMovementSFX;
    public Sound[] playerSuitSFX;
    List<Sound> sounds = new List<Sound>();

     void Awake(){
        foreach (Sound s in playerMovementSFX){
            sounds.Add(s);
        }
        foreach (Sound s in playerSuitSFX){
            sounds.Add(s);
        }
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    private void Start(){
        GameEvents.current.OnSoundCommand += SoundHandler;
    }

    private void OnDestroy() {
        GameEvents.current.OnSoundCommand -= SoundHandler;
    }
    
    public void SoundHandler(string name, string command, float value){
        switch (command){
            case "PitchShift":
            PitchShift(name, value);
            break;

            case "Play":
            Play(name);
            break;

            case "PlayFade":
            PlayFade(name, value);
            break;

            case "Stop":
            Stop(name);
            break;

            case "StopFade":
            StopFade(name, value);
            break;

            default:
            Debug.Log("The desired audio command is not implemented in AudioManager!");
            break;

        }
    }

    public void PitchShift(string name, float pitch){
        Sound s = null;
        foreach (Sound sound in sounds){
            if (sound.name == name){ s = sound; }
        }

        if (s != null){
            float oldPitch = s.source.pitch;
            s.source.pitch = Mathfs.Clamp(Mathfs.Lerp(oldPitch, pitch, 0.5F), 0.8F, 2.0F);
        }
        else { Debug.Log("The desired sound was null!"); }
    }

    public void Play(string name){
        Sound s = null;
        foreach (Sound sound in sounds){
            if (sound.name == name){ s = sound; }
        }

        if (s != null){
            s.source.Play();
        }
        else { Debug.Log("The desired sound was null!"); }
    }

    public void PlayFade(string name, float duration){
        Sound s = null;
        foreach (Sound sound in sounds){
            if (sound.name == name){ s = sound; }
        }

        if (s != null){
            StartCoroutine(FadeIn(s, duration));
            s.source.Play();
        }
        else { Debug.Log("The desired sound was null!"); }
    }

    public void Stop(string name){
        Sound s = null;
        foreach (Sound sound in sounds){
            if (sound.name == name){ s = sound; }
        }

        if (s != null){
            s.source.Stop();
        }
        else { Debug.Log("The desired sound was null!"); }
    }

    public void StopFade(string name, float duration){
        Sound s = null;
        foreach (Sound sound in sounds){
            if (sound.name == name){ s = sound; }
        }

        if (s != null){
            StartCoroutine(FadeOut(s, duration));
        }
        else { Debug.Log("The desired sound was null!"); }
    }

    IEnumerator FadeIn(Sound s, float duration){
        int d = (int)Mathfs.Round(duration); 
        int t = 0;
        while (t <= d){
            s.source.volume = Mathf.Lerp(0, 1, t/d);
            t++;
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        s.source.volume = 1;
    }

    IEnumerator FadeOut(Sound s, float duration){
        int d = (int)Mathfs.Round(duration);
        int t = d;
        while (t >= d){
            s.source.volume = Mathf.Lerp(0, 1, t/d);
            t--;
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        s.source.Stop();
    }
}
