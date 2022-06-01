using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents current;
    public EventSettings eventSettings;

    private void Awake(){
        current = this;
    }

    public void inputUpdate(object sender, float[] a, bool[] b){
    }

    bool windPlayed = false;
    public void playerUpdate(object sender, Vector3 playerVelocity, float accelX, float accelZ, Transform pTransform, float height, Vector3 center, bool isOnGround){
        // sends velocity and accel vectors to the UI system to give some visual feedback
        //Debug.Log(new Vector3(playerVelocity.x, 0, playerVelocity.z).magnitude);
        if (new Vector3(playerVelocity.x, 0, playerVelocity.z).magnitude >= eventSettings.windThreshold && !windPlayed && !isOnGround){
            windPlayed = true;
            playFadeSound("Wind", 2F);
        }
        else if (playerVelocity.magnitude >= eventSettings.windThreshold){
            float pitch = (playerVelocity.magnitude - eventSettings.windThreshold + 1) * eventSettings.windPitchShift;
            pitchShift("Wind", pitch);
        }
        else if (playerVelocity.magnitude <= eventSettings.windThreshold * 1.2F && windPlayed){
            windPlayed = false;
            stopFadeSound("Wind", 0.4F);
        }
        playerPositionUpdate(sender, pTransform, height, center);
        onPlayerHudUpdate(sender, pTransform, playerVelocity, accelX, accelZ);
    }
    
    public event Action<object, string> onHeatPlayer;
    public void heatPlayer(object sender, string name){
        if (onHeatPlayer != null){
            onHeatPlayer(sender, name);
        }
        else{
            Debug.Log("HELP!!!!!");
        }
    }

    public event Action<object, float> onHeatUpdate;
    public void heatUpdate(object sender, float heat){
        if (onHeatUpdate != null){
            onHeatUpdate(sender, heat);
        }
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

    public void playerSlide(object sender){
        playSound("Slide");
    }
    public void playerSlideInterrupted(object sender){
        stopSound("Slide");
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

    public event Action<string, float> onPitchShift;
    public void pitchShift(string name, float pitch){
        if (onPitchShift != null){
            onPitchShift(name, pitch);
        }
    }
}
