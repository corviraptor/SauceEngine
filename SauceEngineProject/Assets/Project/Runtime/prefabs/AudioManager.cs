using UnityEngine.Audio;
using System;
using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

     void Awake(){
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
        GameEvents.current.onPlaySound += Play;
        GameEvents.current.onStopSound += Stop;
        GameEvents.current.onPlayFadeSound += PlayFade;
        GameEvents.current.onStopFadeSound += StopFade;
        GameEvents.current.onPitchShift += pitchShift;
    }

    public void pitchShift(string name, float pitch){
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.pitch = Mathf.Clamp(pitch, 0.8F, 2.3F);
    }

    public void Play(string name){
        Sound s = Array.Find(sounds, sound => sound.name == name);
        //Debug.Log(name);
        s.source.Play();
    }

    public void PlayFade(string name, float speed){
        Sound s = Array.Find(sounds, sound => sound.name == name);
        //Debug.Log(name);
        StartCoroutine(FadeIn(s, speed));
        s.source.Play();
    }

    public void Stop(string name){
        Sound s = Array.Find(sounds, sound => sound.name == name);
        //Debug.Log(name);
        s.source.Stop();
    }

    public void StopFade(string name, float speed){
        Sound s = Array.Find(sounds, sound => sound.name == name);
        //Debug.Log(name);
        StartCoroutine(FadeOut(s, speed));
    }

    IEnumerator FadeIn(Sound s, float speed){
        int fadeLerp = 0;
        while (fadeLerp <= 50){
            s.source.volume = Mathf.Lerp(0, 1, fadeLerp/50f);
            yield return new WaitForSeconds(speed/100f);
            fadeLerp++;
        }
        s.source.volume = 1;
    }

    IEnumerator FadeOut(Sound s, float speed){
        int fadeLerp = 50;
        while (fadeLerp >= 0){
            s.source.volume = Mathf.Lerp(0, 1, fadeLerp/50f);
            yield return new WaitForSeconds(speed/100f);
            fadeLerp--;
        }
        s.source.Stop();
    }

    private void OnDestroy() {
        GameEvents.current.onPlaySound -= Play;
        GameEvents.current.onStopSound -= Stop;
        GameEvents.current.onPlayFadeSound -= PlayFade;
        GameEvents.current.onStopFadeSound -= StopFade;
    }
}
