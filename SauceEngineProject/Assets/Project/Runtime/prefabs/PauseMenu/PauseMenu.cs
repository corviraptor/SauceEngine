using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject windows;
    [SerializeField] private GameObject consolePrefab;
    [SerializeField] private GameObject pauseMenuUI;

    public static bool isPaused = false;
    
    void Start()
    {
        InputManager.current.input.Player.Menu.performed += Menu;
        InputManager.current.input.Player.Console.performed += Console;
    }

    void OnDestroy()
    {
        InputManager.current.input.Player.Console.performed -= Console;
    }

    void Menu(InputAction.CallbackContext obj){
        if (!isPaused){
            Pause();
        }
        else {
            Resume();
        }
    }

    GameObject console;
    void Console(InputAction.CallbackContext obj){
        if (!isPaused){
            Pause();
            return; //return here so that you dont accidentally close the console if its open in the background
        }

        if (console == null){
            console = Instantiate(consolePrefab, windows.transform.position, Quaternion.identity, windows.transform);
        }
        else {
            Destroy(console);
        }
    }

    void Pause(){
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0;
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Resume(){
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Settings(){
        Debug.Log("Settings");
    }

    public void MainMenu(){
        GamemodeSystem.current.SwitchMode("Start", gameObject.scene);
        Debug.Log("Menu");
    }

    public void QuitToDesktop(){
        Debug.Log("QuitToDesktop");
        Application.Quit();
    }
}
