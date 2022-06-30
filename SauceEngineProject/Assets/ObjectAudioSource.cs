using UnityEngine.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Freya;

public class ObjectAudioSource : MonoBehaviour
{
    public WorldSound[] loadedSounds;
    List<WorldSound> sounds = new List<WorldSound>();

    void Awake(){
        foreach (WorldSound s in loadedSounds){
            sounds.Add(s);
        }
        foreach (WorldSound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.minDistance = s.minDistance;
            s.source.maxDistance = s.maxDistance;
            s.source.spatialBlend = s.spatialBlend;
            s.source.dopplerLevel = s.dopplerLevel;
        }
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
        WorldSound s = null;
        foreach (WorldSound sound in sounds){
            if (sound.name == name){ s = sound; }
        }

        if (s != null){
            float oldPitch = s.source.pitch;
            s.source.pitch = Mathfs.Clamp(Mathfs.Lerp(oldPitch, pitch, 0.5F), 0.8F, 2.0F);
        }
        else { Debug.Log("The desired sound was null!"); }
    }

    public void Play(string name){
        WorldSound s = null;
        foreach (WorldSound sound in sounds){
            if (sound.name == name){ s = sound; }
        }

        if (s != null){
            s.source.Play();
        }
        else { Debug.Log("The desired sound was null!"); }
    }

    public void PlayFade(string name, float duration){
        WorldSound s = null;
        foreach (WorldSound sound in sounds){
            if (sound.name == name){ s = sound; }
        }

        if (s != null){
            StartCoroutine(FadeIn(s, duration));
            StopCoroutine(FadeOut(s, duration));
            s.source.Play();
        }
        else { Debug.Log("The desired sound was null!"); }
    }

    public void Stop(string name){
        WorldSound s = null;
        foreach (WorldSound sound in sounds){
            if (sound.name == name){ s = sound; }
        }

        if (s != null){
            s.source.Stop();
        }
        else { Debug.Log("The desired sound was null!"); }
    }

    public void StopFade(string name, float duration){
        WorldSound s = null;
        foreach (WorldSound sound in sounds){
            if (sound.name == name){ s = sound; }
        }

        if (s != null){
            StartCoroutine(FadeOut(s, duration));
            StopCoroutine(FadeIn(s, duration));
        }
        else { Debug.Log("The desired sound was null!"); }
    }

    IEnumerator FadeIn(WorldSound s, float duration){
        int d = (int)Mathfs.Round(duration); 
        int t = 0;
        while (t <= d){
            s.source.volume = Mathf.Lerp(0, 1, t/d);
            t++;
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        s.source.volume = 1;
    }

    IEnumerator FadeOut(WorldSound s, float duration){
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


