using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBlastible
{
    void Blast(object sender, string id, Vector3 blastForceVector);
}
