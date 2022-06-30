using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class WorldSound
{
    public string name;
    public AudioClip clip;

    [Range(0f,1f)]
    public float volume = 1;
    [Range(0.1f,3f)]
    public float pitch = 1;
    [Range(0f,1f)]
    public float spatialBlend = 1;
    

    public float dopplerLevel = 1;
    public float minDistance = 5;
    public float maxDistance = 50;

    [HideInInspector]
    public AudioSource source;
    public bool loop;
}
