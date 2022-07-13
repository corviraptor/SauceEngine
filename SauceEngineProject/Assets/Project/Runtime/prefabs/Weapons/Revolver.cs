using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Revolver : MonoBehaviour, IShootable
{
    public int loadedRounds { get; set; }
    public int magSize { get; set; }
    public bool chambered { get; set; }

    public void InjectDependency(PlayerWeapons playerWeapons){}

    void Update(){}

    public void Draw(int drawTime){}

    public void PrimaryFire(PlayerArgs pargs){}

    public void SecondaryFire(PlayerArgs pargs){}

    public void PrimaryRelease(){}

    public void SecondaryRelease(){}

    public void Reload(){}
}
