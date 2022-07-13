using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunBead : Projectile
{
    public Rigidbody rb;

    void FixedUpdate(){
        rb.AddForce(-Vector3.up * drop, ForceMode.Acceleration);
    }

    protected override void OnTriggerEnter(Collider collider){
        if(collider.tag == "Player" || collider.tag == "Projectile"){
            return;
        }
        gameObject.SetActive(false);
    }

    IEnumerator Cleanup(){
        int i = 0;
        while (i < 1){
            i++;
            yield return new WaitForSeconds(100);
        }
        gameObject.SetActive(false);
        Debug.Log("Cleaned up shotgun bead!");
    }

    void OnDisable(){
        rb.velocity = Vector3.zero;
    }
}
