using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerHeat : MonoBehaviour
{
    public CharacterController cc;
    public PlayerSettings player;
    float heat;

    void Start(){
        GameEvents.current.OnHeatPlayer += HeatPlayer;
    }

    void HeatPlayer(object sender, string name){
        switch (name)
        {
            case "Slide":
                if (heat < 1){
                    heat += player.slideHeat;
                }
                break;
            default:
                Debug.Log("UNREGISTERED HEAT EVENT");
                break;
        }
        
    }

    private void Update() {
        if (heat > 0){
            heat -= player.heatDecay * Time.deltaTime;
            GameEvents.current.HeatUpdate(this, heat); //multiply or add something for stuff that cools the player
        }
    }

    void onDestroy(){
        GameEvents.current.OnHeatPlayer -= HeatPlayer;
    }
}
