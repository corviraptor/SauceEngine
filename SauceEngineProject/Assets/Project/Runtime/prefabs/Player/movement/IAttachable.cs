using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttachable
{
    void InjectDependency(PlayerMovement playerMovement);
}
