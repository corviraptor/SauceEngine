using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : MonoBehaviour, IShootable
{
    public int loadedRounds { get; set; }
    public int magSize { get; set; }
    public bool chambered { get; set; }

    Dictionary<string, int> clocks = new Dictionary<string, int>();

    PlayerWeapons pw;

    Animator viewmodel;  

    bool loadQueued = false;
    bool loading;

    int chamberTime;
    int startTime = 10;
    int baseLoadTime = 42;
    int lastLoadTime = 28;
    int finish1Time = 30;

    public void InjectDependency(PlayerWeapons playerWeapons){
        pw = playerWeapons;
        viewmodel = pw.viewmodels["Shotgun"];

        chambered = false;
        magSize = 6;
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
            chamberTime = 80;
            StartCoroutine(Chamber());
        }
    }
    
    public void SecondaryFire(PlayerArgs pargs){
        loadQueued = false;
        if (chambered && !loading && loadedRounds > 0){
            chambered = false;
            loadedRounds--;
            viewmodel.SetTrigger("Attack2");
            chamberTime = 40;
            StartCoroutine(Chamber());

            Debug.Log("Fired Rocket!");
            GameObject rocket = ObjectPooler.SharedInstance.GetPooledObject(0);
            rocket.SetActive(true);
            rocket.transform.position = pargs.cameraTransform.position + pargs.cameraTransform.forward;
            rocket.transform.rotation = pargs.cameraTransform.rotation;
        }
    }

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

        while (loadQueued && loadedRounds + 2 <= magSize){
            // reload loop
            viewmodel.SetTrigger("LoadLoop");
            // i do it this way so that the rounds will show up in the hud at the actual time that theyre loaded in the animation
            yield return new WaitForSeconds((baseLoadTime * 0.3F) / 60F);
            loadedRounds++;
            yield return new WaitForSeconds((baseLoadTime * 0.2F) / 60F);
            loadedRounds++;
            yield return new WaitForSeconds((baseLoadTime * 0.5F) / 60F);

        }
        if (loadQueued && loadedRounds + 1 == magSize){
            // load last round
            viewmodel.SetTrigger("LoadLast");
            yield return new WaitForSeconds((lastLoadTime * 0.5F) / 60F);
            loadedRounds++;
            yield return new WaitForSeconds((lastLoadTime * 0.5F) / 60F);
        }
        if (loadedRounds >= magSize || !loadQueued){
            // finish reloading
            viewmodel.SetTrigger("LoadFinish1");
            yield return new WaitForSeconds(finish1Time / 60F);
        }

        loading = false;
        chambered = true;
    }
}