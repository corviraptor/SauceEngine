using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents current;

    private void Awake(){
        current = this;
    }

    public void playerUpdate(object sender, Vector3[] v){
        Vector3 playerVelocity = v[0];
        Vector3 playerAccelXZ = v[1];;
    }

    //PLAYER ACTIONS
    public void playerSurfEnter(object sender){
        Debug.Log("Surf!");
        playSound("Surfenter");
        playSound("Surfloop");
        stopSound("Slip");
    }

    public void playerSlipEnter(object sender){
        Debug.Log("Slip!");
        playSound("Slip");
        stopSound("Surfloop");
    }

    public void playerUnslope(object sender){
        Debug.Log("Unslope!");
        stopSound("Surfloop");
        stopSound("Slip");
    }

    public void playerUnsurf(object sender){
        Debug.Log("Unsurf!");
        playSound("Surfrelease");
    }

    public void playerJump(object sender){
        Debug.Log("Jump!");
        playSound("Jump");
    }

    // SOUNDS
    public event Action<string> onPlaySound;
    public void playSound(string name){
        if (onPlaySound != null){
            onPlaySound(name);
        }
    }

    public event Action<string> onStopSound;
    public void stopSound(string name){
        if (onPlaySound != null){
            onStopSound(name);
        }
    }
}
