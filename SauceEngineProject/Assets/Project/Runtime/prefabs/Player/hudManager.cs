using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    [SerializeField] private GameObject crosshair;
    [SerializeField] private GameObject velocityDisplay;
    [SerializeField] private GameObject accelDisplay;
    [SerializeField] private GameObject speedometerDisplay;
    [SerializeField] private Text speedText;
    [SerializeField] private Text heatText;
    [SerializeField] private Slider heatBar;
    bool hudUpdateInProgress = false;
    public PlayerSettings player;
    float newSpeed;
    float newTemperature;
    float temp;

    bool initialized;
    void OnEnable()
    {
        initialized = false;
    }

    void OnDestroy(){
        PlayerHandler.current.OnPlayerHudUpdate -= HudUpdate;
    }

    void Update(){
        if (!initialized){
            PlayerHandler.current.OnPlayerHudUpdate += HudUpdate;
            initialized = true;
        }
    }

    private void HudUpdate(object sender, PlayerArgs playerArgs){
        temp = PlayerHandler.current.playerArgs.temperature;
        if (!hudUpdateInProgress){
            Vector3 adjAccelXZ = InputManager.current.naiveAccelXY;
            StartCoroutine(HudUpdateCycle(sender, playerArgs.localVelocity, adjAccelXZ));
        }
    }

    IEnumerator HudUpdateCycle(object sender, Vector3 velocity, Vector3 accelXZ){
        float i = 0;
        Thermometer(i);

        //speedometer text
        float oldSpeed = newSpeed;
        newSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude;

        while (i < 1){
            AccelIndicator(i, accelXZ);
            VelocityIndicator(i, velocity, newSpeed);
            
            //speedometer text
            speedText.text = $"v = {Mathf.Round(Mathf.Lerp(oldSpeed, newSpeed, i))} m/s";

            hudUpdateInProgress = true;
            i += Time.deltaTime * 30;
            yield return null;
        }
        hudUpdateInProgress = false;
        i=0;
    }

    void Thermometer(float i){
        //thermometer
        float oldTemperature = newTemperature;
        newTemperature = temp;
        heatBar.value = (Mathf.Lerp(oldTemperature, newTemperature, i)) / 100;
        heatText.text = $"{Mathf.Round((Mathf.Lerp(oldTemperature, newTemperature, i)))}°C";
    }

    void AccelIndicator(float i, Vector3 accelXZ){
        Quaternion oldAccelRotation = accelDisplay.transform.rotation;
        float newAccelAngle = Vector3.SignedAngle(accelXZ, Vector3.up, -Vector3.forward);
        Quaternion newAccelRotation = Quaternion.identity;

        if (accelXZ.magnitude >= 0.1F){  
            newAccelRotation.eulerAngles = Vector3.forward * newAccelAngle;
            //brake factor means the indicator won't whip around at ridiculous speeds when rotating by a larger angle
            float brakeAccelFactor = 1 - Quaternion.Angle(oldAccelRotation, newAccelRotation) / 180;
            if (brakeAccelFactor <= 0.3F){brakeAccelFactor += 0.5F;}
            Quaternion trueAccelRotation = Quaternion.Lerp(oldAccelRotation, newAccelRotation, i * brakeAccelFactor);
            accelDisplay.transform.SetPositionAndRotation(accelDisplay.transform.position, trueAccelRotation);
        }
        else {
            //resets values to 1/2tau when not recieving anything to put them at the bottom of the crosshair
            newAccelRotation.eulerAngles = new Vector3(0, 0, 180F);
            Quaternion trueAccelRotation = Quaternion.Lerp(oldAccelRotation, newAccelRotation, i);
            accelDisplay.transform.SetPositionAndRotation(accelDisplay.transform.position, trueAccelRotation);
        }
    }

    void VelocityIndicator(float i, Vector3 velocity, float newSpeed){
        //corrects velocity vector to be XY instead of XZ
        Vector3 newVelocityRotationVector = new Vector3(velocity.x, velocity.z, 0);
        Quaternion oldVelocityRotation = velocityDisplay.transform.rotation;
        float newVelocityAngle = Vector3.SignedAngle(newVelocityRotationVector, Vector3.up, -Vector3.forward);
        Quaternion newVelocityRotation = Quaternion.identity;

        if (newSpeed >= 2F){  
            newVelocityRotation.eulerAngles = Vector3.forward * newVelocityAngle;
            //brake factor means the indicator won't whip around at ridiculous speeds when rotating by a larger angle
            float brakeVelocityFactor = 1 - Quaternion.Angle(oldVelocityRotation, newVelocityRotation) / 180;
            if (brakeVelocityFactor <= 0.3F){brakeVelocityFactor += 0.5F;}
            Quaternion trueVelocityRotation = Quaternion.Lerp(oldVelocityRotation, newVelocityRotation, i * brakeVelocityFactor);
            velocityDisplay.transform.SetPositionAndRotation(velocityDisplay.transform.position, trueVelocityRotation);
        }
        else {
            //resets values to 1/2tau when not recieving anything to put them at the bottom of the crosshair
            newVelocityRotation.eulerAngles = new Vector3(0, 0, 180F);
            Quaternion trueVelocityRotation = Quaternion.Lerp(oldVelocityRotation, newVelocityRotation, i);
            velocityDisplay.transform.SetPositionAndRotation(velocityDisplay.transform.position, trueVelocityRotation);
        }
    }
}
