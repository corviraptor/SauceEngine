using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EventSettings", menuName = "ScriptableObjects/EventSettings", order = 0)]
public class EventSettings : ScriptableObject{
    public float windPitchShift = 0.5F;
    public float windThreshold = 25F;
}
