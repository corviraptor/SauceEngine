using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class Console : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text outputField;

    string input;

    public static bool cheatsEnabled;

    public static List<object> commandList;

    public static DebugCommand<int> SV_CHEATS;
    public static DebugCommand<string> GIVE;
    public static DebugCommand<float> SETHEAT;

    void Awake(){
        SV_CHEATS = new DebugCommand<int>("sv_cheats", "Enables cheats", "sv_cheats <int (1, 0)>", (x) => {
            if (x == 1 && cheatsEnabled == true){
                OutputString("Cheats are alaready enabled!");
                return;
            }
            else if (x == 0 && cheatsEnabled == false){
                OutputString("Cheats are alaready disabled!");
                return;
            }

            if (x == 1){
                OutputString("Cheats enabled");
                cheatsEnabled = true;
            }
            else if (x == 0){
                OutputString("Cheats disabled");
                cheatsEnabled = false;
            }
            else {
                OutputString("sv_cheats only accepts integers 0 and 1!");
            }
        });

        GIVE = new DebugCommand<string>("give", "Gives weapon (Will be generalized in the future)", "give <string weaponName>", (x) => {
            if (!cheatsEnabled){
                return;
            }

            try {
                GameEvents.current.GetWeapon(this, x);
                OutputString("Gave player the " + x);
            }
            catch {
                OutputString(x + " is not an implemented weapon!");
            }
        });

        SETHEAT = new DebugCommand<float>("setheat", "Sets player's heat", "setheat <float value>", (x) => {
            if (!cheatsEnabled){
                return;
            }
            
            GameEvents.current.SetPlayerHeat(this, x);
            OutputString($"Set player heat to {x}");
        });

        commandList = new List<object>{
            SV_CHEATS,
            GIVE,
            SETHEAT,
        };
    }

    void OnEnable()
    {
        InputManager.current.input.Player.Submit.performed += OnSubmit;
        inputField.textComponent.color = Color.white;
    }

    void OnDestroy(){
        InputManager.current.input.Player.Submit.performed -= OnSubmit;
    }

    void OnSubmit(InputAction.CallbackContext obj){
        Submit();
    }

    public void Submit(){
        inputField.text = "";

        string[] properties = input.Split(' ');

        for (int i = 0; i < commandList.Count; i++){
            DebugCommandBase commandBase = commandList[i] as DebugCommandBase;
            if (input.Contains(commandBase.commandId)){
                if (commandList[i] as DebugCommand != null){
                    (commandList[i] as DebugCommand).Invoke();
                }
                else if(commandList[i] as DebugCommand<int> != null){
                    (commandList[i] as DebugCommand<int>).Invoke(int.Parse(properties[1]));
                }
                else if(commandList[i] as DebugCommand<float> != null){
                    (commandList[i] as DebugCommand<float>).Invoke(float.Parse(properties[1]));
                }
                else if(commandList[i] as DebugCommand<string> != null){
                    (commandList[i] as DebugCommand<string>).Invoke(properties[1]);
                }
            }
        }
    }

    void OutputString(string output){
        string newLine = "\n";
        newLine += output;
        outputField.text += newLine;
    }

    void OnGUI(){
        input = inputField.text;
    }
}
