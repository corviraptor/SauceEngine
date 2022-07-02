using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class DraggableWindow : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform dragBar;
    [SerializeField] private RectTransform window;
    [SerializeField] private RectTransform[] edges;
    [SerializeField] private Canvas canvas;

    bool isDraggingWindow;
    bool isResizingWindow;
    int dragDirection;

    public void OnPointerDown(PointerEventData pointerData){
        //pointer down and pointer up are more responsive than drag enter, i need that!!!
        window.SetAsLastSibling();

        if(RectTransformUtility.RectangleContainsScreenPoint(dragBar, pointerData.position)){
            isDraggingWindow = true;
        }

        int i = 0;
        foreach (RectTransform edge in edges){
            if(RectTransformUtility.RectangleContainsScreenPoint(edge, pointerData.position)){
                isResizingWindow = true;
                dragDirection = i;
                return;
            }
            i++;
        }
    }

    public void OnPointerUp(PointerEventData pointerData){
        isResizingWindow = false;
        isDraggingWindow = false;
    }

    public void OnDrag(PointerEventData pointerData){
        Vector2 scaledPointerDelta = pointerData.delta / canvas.scaleFactor;
        if (isDraggingWindow){
            window.anchoredPosition += scaledPointerDelta;
        }

        if (isResizingWindow){
            if (dragDirection == 0 || dragDirection == 2){
                if (dragDirection == 0){
                    window.offsetMax += Vector2.up * scaledPointerDelta.y;
                }
                else {
                    window.offsetMin += Vector2.up * scaledPointerDelta.y;
                }
            }
            
            if (dragDirection == 1 || dragDirection == 3){
                if (dragDirection == 1){
                    window.offsetMax += Vector2.right * scaledPointerDelta.x;
                }
                else {
                    window.offsetMin += Vector2.right * scaledPointerDelta.x;
                }
            }
        }
    }

    public void Close(){
        Destroy(gameObject);
    }
}
