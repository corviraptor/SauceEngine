using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class inputManager : MonoBehaviour
{

        private void Update() {
            float inZ = Input.GetAxis("Vertical");
            float inX = Input.GetAxis("Horizontal");
            bool duck = Input.GetButton("Crouch");
            bool jump = Input.GetButton("Jump");

            float[] axes = {inZ, inX};
            bool[] buttons = {duck, jump};

            GameEvents.current.inputUpdate(this, axes, buttons);
        }
}
