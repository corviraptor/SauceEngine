using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IShootable
{
    int loadedRounds { get; set; }
    int magSize { get; set; }
    bool chambered { get; set; }

    void InjectDependency(PlayerWeapons playerWeapons);

    void Draw(int drawTime);

    void PrimaryFire(PlayerArgs pargs);
    
    void SecondaryFire(PlayerArgs pargs);

    void PrimaryRelease();

    void SecondaryRelease();

    void Reload();
}
