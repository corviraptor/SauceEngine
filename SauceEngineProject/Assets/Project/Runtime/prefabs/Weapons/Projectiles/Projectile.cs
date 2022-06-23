using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public int damage;
    public float speed;
    public float drop;
    public string id;

    protected Projectile(){}

    protected abstract void OnTriggerEnter(Collider collider);
    
}
