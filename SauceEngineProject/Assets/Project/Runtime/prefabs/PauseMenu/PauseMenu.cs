using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    FirstPersonActions.PlayerActions input => InputManager.current.input;

    [SerializeField] private GameObject windows;
    [SerializeField] private GameObject consolePrefab;
    [SerializeField] private GameObject settingsPrefab;
    [SerializeField] private GameObject pauseMenuUI;

    public static bool isPaused = false;
    
    void Start()
    {
        Resume();
        input.Menu.performed += Menu;
        input.Console.performed += Console;
    }

    void OnDestroy()
    {
        input.Menu.performed -= Menu;
        input.Console.performed -= Console;
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
        if (console == null){
            console = Instantiate(consolePrefab, windows.transform.position, Quaternion.identity, windows.transform);
        }
        else if (isPaused){
            // only destroy if we're about to unpause
            Destroy(console);
        }

        if (!isPaused){
            Pause();
        }
        else if (console == null){
            Resume();
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

    GameObject settings;
    public void Settings(){
        if (settings == null){
            settings = Instantiate(settingsPrefab, windows.transform.position, Quaternion.identity, windows.transform);
            settings.transform.position += new Vector3(-20, -20, 0);
        }
        else{
            Destroy(settings);
        }
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
