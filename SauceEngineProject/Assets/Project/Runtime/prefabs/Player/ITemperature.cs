using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITemperature
{
    void AddHeat(object sender, float heat);
    void AdjustEnvironmentalTemperature(object sender, float temp);
}
