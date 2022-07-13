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
        rb.velocity = (transform.forward * speed) - (transform.up * drop);
    }

    protected override void OnTriggerEnter(Collider collider){
        if(collider.tag == "Player"){
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, blastRadius);
        foreach (Collider c in hits){
            if (c.gameObject.GetComponent<IBlastible>() != null){
				Vector3 diff = (c.gameObject.transform.position - transform.position);
				float blastDistancePercent = (blastRadius - diff.magnitude) / blastRadius;
                Vector3 blastForceVector = diff.normalized * blastPower * blastDistancePercent;
                c.gameObject.GetComponent<IBlastible>().Blast(this, id, blastForceVector);
            }
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
        Debug.Log("Cleaned up rocket!");
    }
}
