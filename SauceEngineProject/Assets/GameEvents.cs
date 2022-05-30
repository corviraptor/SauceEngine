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

    bool windPlayed = false;
    public void playerUpdate(object sender, Vector3 playerVelocity, float accelX, float accelZ, Transform pTransform, float height, Vector3 center){
        // sends velocity and accel vectors to the UI system to give some visual feedback
        //Debug.Log(new Vector3(playerVelocity.x, 0, playerVelocity.z).magnitude);
        if (playerVelocity.magnitude >= 30 && !windPlayed){
            windPlayed = true;
            playFadeSound("Wind", 1F);
        }
        else if (playerVelocity.magnitude <= 28 && windPlayed){
            windPlayed = false;
            stopFadeSound("Wind", 0.1F);
        }
        playerPositionUpdate(sender, pTransform, height, center);
        onPlayerHudUpdate(sender, pTransform, playerVelocity, accelX, accelZ);
    }

    public event Action<object, Transform, Vector3, float, float> onPlayerHudUpdate;
    public void playerHudUpdate(object sender, Transform pTransform, Vector3 velocity, float accelX, float accelZ){
        if (onPlayerHudUpdate != null){
            onPlayerHudUpdate(sender, pTransform, velocity, accelX, accelZ);
        }
    }

    public event Action<object, Transform, float, Vector3> onPlayerPositionUpdate;
    public void playerPositionUpdate(object sender, Transform pTransform, float height, Vector3 center){
        if (onPlayerPositionUpdate != null){
            onPlayerPositionUpdate(sender, pTransform, height, center);
        }
    }

    // PLAYER ACTIONS
    public void playerSurfEnter(object sender){
        playSound("Surfenter");
        playFadeSound("Surfloop", 0F);
        stopFadeSound("Slip", 0.05F);
    }

    public void playerSlipEnter(object sender){
        playFadeSound("Slip", 0.05F);
        stopFadeSound("Surfloop", 0.05F);
    }

    public void playerUnslope(object sender){
        stopFadeSound("Surfloop", 0F);
        stopFadeSound("Slip", 0.05F);
    }

    public void playerUnsurf(object sender){
        playSound("Surfrelease");
    }

    public void playerJump(object sender){
        playSound("Jump");
    }

    // SOUNDS
    public event Action<string> onPlaySound;
    public void playSound(string name){
        if (onPlaySound != null){
            onPlaySound(name);
        }
    }

    public event Action<string, float> onPlayFadeSound;
    public void playFadeSound(string name, float speed){
        if (onPlayFadeSound != null){
            onPlayFadeSound(name, speed);
        }
    }

    public event Action<string> onStopSound;
    public void stopSound(string name){
        if (onPlaySound != null){
            onStopSound(name);
        }
    }

    public event Action<string, float> onStopFadeSound;
    public void stopFadeSound(string name, float speed){
        if (onStopFadeSound != null){
            onStopFadeSound(name, speed);
        }
    }
}
