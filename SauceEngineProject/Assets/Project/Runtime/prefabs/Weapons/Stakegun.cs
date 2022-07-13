using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stakegun : MonoBehaviour, IShootable
{   
    public int loadedRounds { get; set; }
    public int magSize { get; set; }
    public bool chambered { get; set; }

    Dictionary<string, int> clocks = new Dictionary<string, int>();

    PlayerWeapons pw;

    Animator viewmodel;  

    int chamberTime;
    int reloadStage;

    bool loadQueued = false;
    bool loading;


    float coolantDrain = 30;
    int startTime = 11;
    int magOutTime = 12;
    int magInTime = 17;
    int boltPullTime = 63;
    bool coolanting = false;
    float coolant = 100;

    public void InjectDependency(PlayerWeapons playerWeapons){
        pw = playerWeapons;
        viewmodel = pw.viewmodels["Stakegun"];

        chamberTime = 68;

        chambered = false;
        magSize = 5;
        loadedRounds = magSize;
    }

    void Update(){
        if (loadQueued || loadedRounds <= 0){ Reload(); }
    }
    
    public void Draw(int drawTime){
        loading = false;
        if (!loadQueued){
            chambered = false;
            StartCoroutine(DoDraw(drawTime));
        }
    }

    IEnumerator DoDraw(int drawTime){
        yield return new WaitForSeconds(drawTime / 60F);
        chambered = true;
    }

    public void PrimaryFire(PlayerArgs pargs){
        loadQueued = false;
        if (chambered && !loading && loadedRounds > 0){
            chambered = false;
            loadedRounds--;
            viewmodel.SetTrigger("Attack1");
            StartCoroutine(Chamber());
        }
    }

    public void SecondaryFire(PlayerArgs pargs){}

    IEnumerator Chamber(){
        yield return new WaitForSeconds(chamberTime / 60F);
        chambered = true;
    }

    public void PrimaryRelease(){}

    public void SecondaryRelease(){}
    
    public void Reload(){
        loadQueued = true;
        if (loadedRounds < magSize && chambered && !loading){
            loading = true;
            StartCoroutine(DoReload());
        }
    }

    IEnumerator DoReload(){
        viewmodel.SetTrigger("LoadStart");
        yield return new WaitForSeconds(startTime / 60F);
        if (reloadStage == 0){
            viewmodel.SetTrigger("MagOut");
            yield return new WaitForSeconds(magOutTime / 60F);
            reloadStage = 1;
            Debug.Log("1");
        }
        if (reloadStage == 1){
            // rn ive combined these because the draw animation is being mean, figure that out later future me ok?
            viewmodel.SetTrigger("MagIn");
            yield return new WaitForSeconds(magInTime / 60F);
            reloadStage = 2;
            Debug.Log("2");
        }
        if (reloadStage == 2){
            viewmodel.SetTrigger("BoltPull");
            yield return new WaitForSeconds(boltPullTime / 60F);
            reloadStage = 0;
            Debug.Log("0");

            loadedRounds = magSize;
            loadQueued = false;
            loading = false;
            chambered = true;
        }

    }
}
