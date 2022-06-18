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
        if (new Vector3(playerVelocity.x, 0, playerVelocity.z).magnitude >= eventSettings.windThreshold && !windPlayed && !isOnGround){
            windPlayed = true;
            PlayFadeSound("Wind", 2F);
        }
        else if (playerVelocity.magnitude >= eventSettings.windThreshold){
            float pitch = (playerVelocity.magnitude - eventSettings.windThreshold + 1) * eventSettings.windPitchShift;
            PitchShift("Wind", pitch);
        }
        else if (playerVelocity.magnitude <= eventSettings.windThreshold * 1.2F && windPlayed){
            windPlayed = false;
            StopFadeSound("Wind", 0.4F);
        }
        PlayerPositionUpdate(sender, pTransform, height, center);
        OnPlayerHudUpdate(sender, pTransform, playerVelocity, accelX, accelZ);
    }
    
    public event Action<object, string> OnHeatPlayer;
    public void HeatPlayer(object sender, string name){
        if (OnHeatPlayer != null){
            OnHeatPlayer(sender, name);
        }
    }

    public event Action<object, float> OnHeatUpdate;
    public void HeatUpdate(object sender, float heat){
        if (OnHeatUpdate != null){
            OnHeatUpdate(sender, heat);
        }
    }
    
    public event Action<object, Transform, Vector3, float, float> OnPlayerHudUpdate;
    public void PlayerHudUpdate(object sender, Transform pTransform, Vector3 velocity, float accelX, float accelZ){
        if (OnPlayerHudUpdate != null){
            OnPlayerHudUpdate(sender, pTransform, velocity, accelX, accelZ);
        }
    }

    public event Action<object, Transform, float, Vector3> OnPlayerPositionUpdate;
    public void PlayerPositionUpdate(object sender, Transform pTransform, float height, Vector3 center){
        if (OnPlayerPositionUpdate != null){
            OnPlayerPositionUpdate(sender, pTransform, height, center);
        }
    }

    // PLAYER ACTIONS
    public void playerSurfEnter(object sender){
        PlaySound("Surfenter");
        PlayFadeSound("Surfloop", 0F);
        StopFadeSound("Slip", 0.05F);
    }

    public void playerSlipEnter(object sender){
        PlayFadeSound("Slip", 0.05F);
        StopFadeSound("Surfloop", 0.05F);
    }

    public void playerUnslope(object sender){
        StopFadeSound("Surfloop", 0F);
        StopFadeSound("Slip", 0.05F);
    }

    public void playerUnsurf(object sender){
        PlaySound("Surfrelease");
    }

    public void playerJump(object sender){
        PlaySound("Jump");
    }

    public void playerSlide(object sender){
        PlaySound("Slide");
    }
    public void playerSlideInterrupted(object sender){
        StopSound("Slide");
    }

    // SOUNDS
    public event Action<string> OnPlaySound;
    public void PlaySound(string name){
        if (OnPlaySound != null){
            OnPlaySound(name);
        }
    }

    public event Action<string, float> OnPlayFadeSound;
    public void PlayFadeSound(string name, float speed){
        if (OnPlayFadeSound != null){
            OnPlayFadeSound(name, speed);
        }
    }

    public event Action<string> OnStopSound;
    public void StopSound(string name){
        if (OnStopSound != null){
            OnStopSound(name);
        }
    }

    public event Action<string, float> OnStopFadeSound;
    public void StopFadeSound(string name, float speed){
        if (OnStopFadeSound != null){
            OnStopFadeSound(name, speed);
        }
    }

    public event Action<string, float> OnPitchShift;
    public void PitchShift(string name, float pitch){
        if (OnPitchShift != null){
            OnPitchShift(name, pitch);
        }
    }
}
