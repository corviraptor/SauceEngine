using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class hudManager : MonoBehaviour
{
    [SerializeField] private GameObject crosshair;
    [SerializeField] private GameObject velocityDisplay;
    [SerializeField] private GameObject accelDisplay;
    [SerializeField] private GameObject speedometerDisplay;
    [SerializeField] private Text speedText;
    bool hudUpdateInProgress = false;

    // Start is called before the first frame update
    void Start()
    {
        GameEvents.current.onPlayerHudUpdate += hudUpdate;
    }

    private void hudUpdate(object sender, Transform transform, Vector3 velocity, float accelX, float accelZ){
        if (!hudUpdateInProgress){
            StartCoroutine(hudUpdateCycle(sender, velocity, accelX, accelZ));
        }
    }

    float newSpeed;
    IEnumerator hudUpdateCycle(object sender, Vector3 velocity, float accelX, float accelZ){
        //accel indicator
        float i = 0;
        Vector3 newAccelRotationVector = new Vector3(accelX, accelZ, 0);
        Quaternion oldAccelRotation = accelDisplay.transform.rotation;
        float newAccelAngle = Vector3.SignedAngle(newAccelRotationVector, Vector3.up, -Vector3.forward);
        Quaternion newAccelRotation = Quaternion.identity;

        //velocity indicator
        Vector3 newVelocityRotationVector = new Vector3(velocity.x, velocity.z, 0);
        Quaternion oldVelocityRotation = velocityDisplay.transform.rotation;
        float newVelocityAngle = Vector3.SignedAngle(newVelocityRotationVector, Vector3.up, -Vector3.forward);
        Quaternion newVelocityRotation = Quaternion.identity;

        //speedometer text
        float oldSpeed = newSpeed;
        newSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude;

        while (i < 1 && newAccelRotationVector.magnitude > 0.1){
            //accel indicator
            newAccelRotation.eulerAngles = Vector3.forward * newAccelAngle;
            //brake factor means the indicator won't whip around at ridiculous speeds when rotating by a larger angle
            float brakeAccelFactor = 1 - Quaternion.Angle(oldAccelRotation, newAccelRotation) / 180;
            if (brakeAccelFactor <= 0.3F){brakeAccelFactor += 0.5F;}
            Quaternion trueAccelRotation = Quaternion.Lerp(oldAccelRotation, newAccelRotation, i * brakeAccelFactor);
            accelDisplay.transform.SetPositionAndRotation(accelDisplay.transform.position, trueAccelRotation);

            //velocity indicator
            newVelocityRotation.eulerAngles = Vector3.forward * newVelocityAngle;
            //brake factor means the indicator won't whip around at ridiculous speeds when rotating by a larger angle
            float brakeVelocityFactor = 1 - Quaternion.Angle(oldVelocityRotation, newVelocityRotation) / 180;
            if (brakeVelocityFactor <= 0.3F){brakeVelocityFactor += 0.5F;}
            Quaternion trueVelocityRotation = Quaternion.Lerp(oldVelocityRotation, newVelocityRotation, i * brakeVelocityFactor);
            velocityDisplay.transform.SetPositionAndRotation(velocityDisplay.transform.position, trueVelocityRotation);
            
            //speedometer text
            speedText.text = $"v = {Mathf.Round(Mathf.Lerp(oldSpeed, newSpeed, i))} m/s";

            hudUpdateInProgress = true;
            i += Time.deltaTime * 30;
            yield return null;
        }
        while (i < 1 && newAccelRotationVector.magnitude < 0.1){
            //resets values to 1pi when not recieving anything to put them at the bottom of the crosshair
            newVelocityRotation.eulerAngles = new Vector3(0, 0, 180F);
            Quaternion trueAccelRotation = Quaternion.Lerp(oldAccelRotation, newVelocityRotation, i);
            accelDisplay.transform.SetPositionAndRotation(accelDisplay.transform.position, trueAccelRotation);

            Quaternion trueVelocityRotation = Quaternion.Lerp(oldVelocityRotation, newVelocityRotation, i);
            velocityDisplay.transform.SetPositionAndRotation(velocityDisplay.transform.position, trueVelocityRotation);
            
            //speedometer text
            speedText.text = $"v = {Mathf.Round(Mathf.Lerp(oldSpeed, 0, i))} m/s";
            i += Time.deltaTime * 30;
            yield return null;
        }
        hudUpdateInProgress = false;
        i=0;
    }

    void OnDestroy() {
        GameEvents.current.onPlayerHudUpdate -= hudUpdate;
    }
}
