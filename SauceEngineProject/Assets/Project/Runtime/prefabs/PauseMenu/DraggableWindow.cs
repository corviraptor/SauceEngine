using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableWindow : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler
{
    [SerializeField] private RectTransform dragBar;
    [SerializeField] private RectTransform window;
    [SerializeField] private Canvas canvas;

    bool isDraggingWindow;

    public void OnBeginDrag(PointerEventData pointerData){
        if(RectTransformUtility.RectangleContainsScreenPoint(dragBar, pointerData.position)){
            isDraggingWindow = true;
        }
    }

    public void OnEndDrag(PointerEventData pointerData){
        isDraggingWindow = false;
    }

    public void OnDrag(PointerEventData pointerData){
        if (isDraggingWindow){
            window.anchoredPosition += pointerData.delta / canvas.scaleFactor;
        }
    }

    public void OnPointerDown(PointerEventData pointerData){
        window.SetAsLastSibling();
    }

    public void Close(){
        Destroy(gameObject);
    }
}
