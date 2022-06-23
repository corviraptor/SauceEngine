using System.Linq;   
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : Projectile
{
    public Rigidbody rb;
    public float blastRadius = 3;
    public float blastPower = 50;

    void FixedUpdate(){
        id = "Rocket";
        rb.AddForce((transform.forward * speed) - (transform.up * drop), ForceMode.VelocityChange);
    }

    protected override void OnTriggerEnter(Collider collider){
        if(collider.tag == "Player"){
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, blastRadius);
        foreach (Collider c in hits){
            if (c.gameObject.GetComponent<IBlastible>() != null){
				var diff = (c.gameObject.transform.position - transform.position);
				var blastDistancePercent = (blastRadius - diff.magnitude) / blastRadius;
                Vector3 blastForceVector = diff.normalized * blastPower * blastDistancePercent;
                c.gameObject.GetComponent<IBlastible>().Blast(this, id, blastForceVector);
            }
        }

        Debug.Log("booom!!!!! waah!@!! yowcch!!!" + collider.gameObject.name);
        gameObject.SetActive(false);
    }

    IEnumerator Cleanup(){
        int i = 0;
        while (i < 1){
            i++;
            yield return new WaitForSeconds(100);
        }
        gameObject.SetActive(false);
    }
}
