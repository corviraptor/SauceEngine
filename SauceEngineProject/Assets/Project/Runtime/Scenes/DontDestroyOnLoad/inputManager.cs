using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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

    public Vector3 naiveAccelXY;
    public Vector2 lookVector;
    public bool jump;
    public bool crouched;
    public bool slide;
    public bool attack;
    public bool attack2;
    public bool spell;
    public bool menu;
    public bool weapon0;
    public bool weapon1;
    public bool weapon2;
    public bool weapon3;
    public bool weapon4;
    
    public Dictionary<string, bool> bools = new Dictionary<string, bool>();

    void Awake(){
        input = new FirstPersonActions();
        current = this;
    }

    void OnEnable(){
        bools.Add("jump", false);
        bools.Add("crouched", false);
        bools.Add("slide", false);
        bools.Add("attack", false);
        bools.Add("attack2", false);
        bools.Add("spell", false);
        bools.Add("menu", false);
        bools.Add("weapon0", false);
        bools.Add("weapon1", false);
        bools.Add("weapon2", false);
        bools.Add("weapon3", false);
        bools.Add("weapon4", false);

        horizontal = input.Player.Horizontal;
        look = input.Player.Look;
        crouch = input.Player.Crouch;

        input.Player.Jump.performed += Jump;

        input.Player.Crouch.performed += Slide;

        input.Player.PrimaryFire.performed += PrimaryFire;

        input.Player.SecondaryFire.performed += SecondaryFire;

        input.Player.CastSpell.performed += CastSpell;

        input.Player.Menu.performed += Menu;

        input.Player.Weapon0.performed += Weapon0;
        input.Player.Weapon1.performed += Weapon1;
        input.Player.Weapon2.performed += Weapon2;
        input.Player.Weapon3.performed += Weapon3;
        input.Player.Weapon4.performed += Weapon4;

        input.Player.Enable();
    }

    void onDisable()
    {

        input.Player.Jump.performed -= Jump;

        input.Player.Crouch.performed -= Slide;

        input.Player.PrimaryFire.performed -= PrimaryFire;

        input.Player.SecondaryFire.performed -= SecondaryFire;

        input.Player.CastSpell.performed -= CastSpell;

        input.Player.Menu.performed -= Menu;

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
    }

    void LateUpdate(){
        jump = bools["jump"];
        slide = bools["slide"];
        attack = bools["attack"];
        attack2 = bools["attack2"];
        spell = bools["spell"];
        menu = bools["menu"];
        weapon0 = bools["weapon0"];
        weapon1 = bools["weapon1"];
        weapon2 = bools["weapon2"];
        weapon3 = bools["weapon3"];
        weapon4 = bools["weapon4"];
    }

    void Jump(InputAction.CallbackContext obj){
        bools["jump"] = true;
        StartCoroutine(Reset("jump"));
    }

    void Slide(InputAction.CallbackContext obj){
        bools["slide"] = true;
        StartCoroutine(Reset("slide"));
    }

    void PrimaryFire(InputAction.CallbackContext obj){
        bools["attack"] = true;
        StartCoroutine(Reset("attack"));
    }

    void SecondaryFire(InputAction.CallbackContext obj){
        bools["attack2"] = true;
        StartCoroutine(Reset("attack2"));
    }

    void CastSpell(InputAction.CallbackContext obj){
        bools["spell"] = true;
        StartCoroutine(Reset("spell"));
    }

    void Menu(InputAction.CallbackContext obj){
        bools["menu"] = true;
        StartCoroutine(Reset("menu"));
    }

    void Weapon0(InputAction.CallbackContext obj){
        bools["weapon0"] = true;
        StartCoroutine(Reset("weapon0"));
    }

    void Weapon1(InputAction.CallbackContext obj){
        bools["weapon1"] = true;
        StartCoroutine(Reset("weapon1"));
    }

    void Weapon2(InputAction.CallbackContext obj){
        bools["weapon2"] = true;
        StartCoroutine(Reset("weapon2"));
    }

    void Weapon3(InputAction.CallbackContext obj){
        bools["weapon3"] = true;
        StartCoroutine(Reset("weapon3"));
    }

    void Weapon4(InputAction.CallbackContext obj){
        bools["weapon4"] = true;
        StartCoroutine(Reset("weapon0"));
    }

    IEnumerator Reset(string name){
        int i = 0;
        while (i == 0){
            i++;
            yield return null;
        }
        while (i == 1){
            i++;
            bools[name] = false;
            yield return null;
        }
    }
}
