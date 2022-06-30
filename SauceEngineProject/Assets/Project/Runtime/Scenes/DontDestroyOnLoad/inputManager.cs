using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager current;
    public PlayerSettings player;

    private FirstPersonActions input;
    private InputAction horizontal;
    private InputAction look;
    private InputAction crouch;
    private InputAction attack;
    private InputAction attack2;

    public Vector3 naiveAccelXY;
    public Vector2 lookVector;
    public bool attacking;
    public bool attack2ing;
    public bool crouched;
    
    public Dictionary<string, bool> buttons = new Dictionary<string, bool>();

    void Awake(){
        input = new FirstPersonActions();
        current = this;
    }

    void OnEnable(){

        string[] initializedButtons = {"jump", "crouched", "slide", "onAttack", "onAttack2", "reload", "spell", "menu", "console", "weapon0", "weapon1", "weapon2", "weapon3", "weapon4"};
        foreach (string s in initializedButtons){
            buttons.Add(s, false);
        }

        horizontal = input.Player.Horizontal;
        look = input.Player.Look;
        crouch = input.Player.Crouch;
        attack = input.Player.PrimaryFire;
        attack2 = input.Player.SecondaryFire;

        input.Player.Jump.performed += Jump;
        input.Player.Crouch.performed += Slide;
        input.Player.PrimaryFire.performed += OnPrimaryFire;
        input.Player.SecondaryFire.performed += OnSecondaryFire;
        input.Player.Reload.performed += Reload;
        input.Player.CastSpell.performed += CastSpell;
        input.Player.Menu.performed += Menu;
        input.Player.Console.performed += Console;

        input.Player.Weapon0.performed += Weapon0;
        input.Player.Weapon1.performed += Weapon1;
        input.Player.Weapon2.performed += Weapon2;
        input.Player.Weapon3.performed += Weapon3;
        input.Player.Weapon4.performed += Weapon4;

        input.Player.Enable();
    }

    void onDisable(){
        input.Player.Jump.performed -= Jump;
        input.Player.Crouch.performed -= Slide;
        input.Player.PrimaryFire.performed -= OnPrimaryFire;
        input.Player.SecondaryFire.performed -= OnSecondaryFire;
        input.Player.Reload.performed -= Reload;
        input.Player.CastSpell.performed -= CastSpell;
        input.Player.Menu.performed -= Menu;
        input.Player.Console.performed -= Console;

        input.Player.Weapon0.performed -= Weapon0;
        input.Player.Weapon1.performed -= Weapon1;
        input.Player.Weapon2.performed -= Weapon2;
        input.Player.Weapon3.performed -= Weapon3;
        input.Player.Weapon4.performed -= Weapon4;

        input.Player.Disable();
    }

    void Update(){
        naiveAccelXY = horizontal.ReadValue<Vector2>();
        lookVector = player.sens * look.ReadValue<Vector2>();

        crouched = false;
        if (crouch.ReadValue<float>() != 0){
            crouched = true;
        }

        attacking = false;
        if (attack.ReadValue<float>() != 0){
            attacking = true;
        }

        attack2ing = false;
        if (attack2.ReadValue<float>() != 0){
            attack2ing = true;
        }
    }

    void Jump(InputAction.CallbackContext obj){
        StartCoroutine(Reset("jump"));
    }

    void Slide(InputAction.CallbackContext obj){
        StartCoroutine(Reset("slide"));
    }

    void OnPrimaryFire(InputAction.CallbackContext obj){
        StartCoroutine(Reset("onAttack"));
    }

    void OnSecondaryFire(InputAction.CallbackContext obj){
        StartCoroutine(Reset("onAttack2"));
    }

    void Reload(InputAction.CallbackContext obj){
        StartCoroutine(Reset("reload"));
    }

    void CastSpell(InputAction.CallbackContext obj){
        StartCoroutine(Reset("spell"));
    }

    void Menu(InputAction.CallbackContext obj){
        StartCoroutine(Reset("menu"));
    }

    void Console(InputAction.CallbackContext obj){
        StartCoroutine(Reset("console"));
    }

    void Weapon0(InputAction.CallbackContext obj){
        StartCoroutine(Reset("weapon0"));
    }

    void Weapon1(InputAction.CallbackContext obj){
        StartCoroutine(Reset("weapon1"));
    }

    void Weapon2(InputAction.CallbackContext obj){
        StartCoroutine(Reset("weapon2"));
    }

    void Weapon3(InputAction.CallbackContext obj){
        StartCoroutine(Reset("weapon3"));
    }

    void Weapon4(InputAction.CallbackContext obj){
        StartCoroutine(Reset("weapon0"));
    }

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
