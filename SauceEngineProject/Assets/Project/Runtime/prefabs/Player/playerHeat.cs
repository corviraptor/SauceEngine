using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Freya;

public class PlayerHeat : MonoBehaviour, IHeatable
{
    public PlayerHandler playerHandler;
    public CharacterController cc;
    public PlayerSettings player;
    float temperature = 30;
    float environmentalTemperature = 30; //will change based on coolant clouds, fire, etc
    PlayerArgs playerArgs;

    void Start(){
        GameEvents.current.OnHeatPlayer += HeatPlayer;
        playerArgs = playerHandler.playerArgs;
    }

    public void AddHeat(object sender, float heat){
        temperature += heat;
    }

    public void AdjustEnvironmentalTemperature(object sender, float temp){
        environmentalTemperature = temp;
    }

    void HeatPlayer(object sender, float amount){
        AddHeat(this, amount);
    }

    bool heatLimited = false;
    bool overheated = false;
    private void FixedUpdate() {
        float desiredTemperature = environmentalTemperature;
        float coolingRate = 1; // rate at which player loses heat, decreased due to wind, being wet, etc

        if (playerArgs.velocity.sqrMagnitude  > player.walkSpeed * player.walkSpeed){
            coolingRate += Mathfs.Clamp((playerArgs.velocity.magnitude  - player.walkSpeed) / 150, 0, player.heatDecay * 4);
        }
        if (playerArgs.velocity.sqrMagnitude  > player.walkSpeed * player.walkSpeed){
            //will approach a cooler temperature when moving faster to communicate to the player that you lose more heat when moving faster
            desiredTemperature = environmentalTemperature - Mathfs.Clamp(playerArgs.velocity.magnitude / 10, 0, 10);
        }

        if (temperature < desiredTemperature){
            temperature += player.heatDecay;
        }
        else {
            temperature -= player.heatDecay * coolingRate;
        }

        playerHandler.HeatUpdate(this, temperature);

        if (temperature > player.heatLimit && !heatLimited){
            playerHandler.SoundCommand("HeatLimit", "Play", 0);
            heatLimited = true;
        }
        if (temperature < player.heatLimit && heatLimited){
            playerHandler.SoundCommand("HeatRecovery", "Play", 0);
            heatLimited = false;
        }

        if (temperature > player.overheat && !overheated){
            playerHandler.SoundCommand("Overheat", "Play", 0);
            overheated = true;
        }
        if (temperature <= player.overheat && overheated){
            Debug.Log("Unoverheated!");
            overheated = false;
        }
    }

    void onDestroy(){
        GameEvents.current.OnHeatPlayer -= HeatPlayer;
    }
}
