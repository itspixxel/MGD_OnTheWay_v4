using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public event Action<Ray> OnMouseClick, OnMouseHold;
    public event Action OnMouseUp, OnEscape;
    private Vector2 mouseMovementVector = Vector2.zero;
    public Vector2 CameraMovementVector { get => mouseMovementVector; }
    [SerializeField]
    Camera mainCamera;


    void Update()
    {
        CheckClickDownEvent();
        CheckClickHoldEvent();
        CheckClickUpEvent();
        CheckArrowInput();
        CheckEscClick();
    }

    private void CheckClickHoldEvent()
    {
        if (Input.GetMouseButton(0) && EventSystem.current.IsPointerOverGameObject() == false)
        {

            OnMouseClick?.Invoke(mainCamera.ScreenPointToRay(Input.mousePosition));
        }
    }

    private void CheckClickUpEvent()
    {
        if (Input.GetMouseButtonUp(0) && EventSystem.current.IsPointerOverGameObject() == false)
        {
            OnMouseUp?.Invoke();
        }
    }

    private void CheckClickDownEvent()
    {
        if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject() == false)
        {
            OnMouseClick?.Invoke(mainCamera.ScreenPointToRay(Input.mousePosition));
        }
    }

    private void CheckEscClick()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnEscape.Invoke();
        }
    }

    private void CheckArrowInput()
    {
        mouseMovementVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    public void ClearEvents()
    {
        OnMouseClick = null;
        OnMouseHold = null;
        OnEscape = null;
        OnMouseUp = null;
    }
}
