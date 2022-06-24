using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHeatable
{
    void AddHeat(object sender, float heat);
    void AdjustEnvironmentalTemperature(object sender, float temp);
}
