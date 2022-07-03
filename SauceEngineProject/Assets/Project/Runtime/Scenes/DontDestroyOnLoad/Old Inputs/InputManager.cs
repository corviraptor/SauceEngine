using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager current;
    public PlayerSettings player;

    public FirstPersonActions actions => new FirstPersonActions();
    public FirstPersonActions.PlayerActions input;
    private InputAction horizontal => input.Horizontal;
    private InputAction look => input.Look;

    public Vector3 naiveAccelXY;
    public Vector2 lookVector;
    
    Dictionary<string, bool> buttons = new Dictionary<string, bool>();

    void Awake(){
        input = actions.Player;
        current = this;
    }

    void OnEnable(){

        string[] initializedButtons = {"jump", "slide", "reload", "spell", "weapon0", "weapon1", "weapon2", "weapon3", "weapon4"};
        foreach (string s in initializedButtons){
            buttons.Add(s, false);
        }

        input.Jump.performed += Jump;
        input.Slide.performed += Slide;
        input.Reload.performed += Reload;

        input.Weapon0.performed += Weapon0;
        input.Weapon1.performed += Weapon1;
        input.Weapon2.performed += Weapon2;
        input.Weapon3.performed += Weapon3;
        input.Weapon4.performed += Weapon4;

        input.Enable();
    }

    void onDisable(){
        input.Jump.performed -= Jump;
        input.Slide.performed -= Slide;
        input.Reload.performed -= Reload;

        input.Weapon0.performed -= Weapon0;
        input.Weapon1.performed -= Weapon1;
        input.Weapon2.performed -= Weapon2;
        input.Weapon3.performed -= Weapon3;
        input.Weapon4.performed -= Weapon4;

        input.Disable();
    }

    void Update(){
        if (PauseMenu.isPaused){ return; }

        naiveAccelXY = horizontal.ReadValue<Vector2>();
        lookVector = player.sens * look.ReadValue<Vector2>();
    }

    void Jump(InputAction.CallbackContext obj){ StartCoroutine(Reset("jump")); }
    void Slide(InputAction.CallbackContext obj){ StartCoroutine(Reset("slide")); }
    void Reload(InputAction.CallbackContext obj){ StartCoroutine(Reset("reload")); }
    void Weapon0(InputAction.CallbackContext obj){ StartCoroutine(Reset("weapon0")); }
    void Weapon1(InputAction.CallbackContext obj){ StartCoroutine(Reset("weapon1")); }
    void Weapon2(InputAction.CallbackContext obj){ StartCoroutine(Reset("weapon2")); }
    void Weapon3(InputAction.CallbackContext obj){ StartCoroutine(Reset("weapon3")); }
    void Weapon4(InputAction.CallbackContext obj){ StartCoroutine(Reset("weapon0")); }

    public event Action<Dictionary<string, bool>> OnPressButtons;
    public void PressButtons(){ OnPressButtons?.Invoke(buttons); }

    IEnumerator Reset(string name){
        buttons[name] = true;
        PressButtons();

        yield return null;

        buttons[name] = false;
        PressButtons();
    }
}
