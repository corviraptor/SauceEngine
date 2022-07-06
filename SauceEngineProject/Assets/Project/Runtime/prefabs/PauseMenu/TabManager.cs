using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class TabManager : MonoBehaviour
{
    [SerializeField] private RectTransform body;
    [SerializeField] private GameObject defaultPage;
    
    private GameObject currentPage;

    void OnEnable(){
        //SwitchTab(defaultPage);
    }

    public void SwitchTab(GameObject page){
        if (page == currentPage){ return; } //dont re-load tab if its already open
        if (currentPage != null){
            Destroy(currentPage);
        }

        currentPage = Instantiate(page, body);
        RectTransform currentPageTransform = currentPage.GetComponent<RectTransform>();

        currentPageTransform = body;

    }
}
