using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class InputController : MonoBehaviour
{
    public Action<Vector3> onMouseDown { get; set; }
    public Action<Vector3> onMouseHold { get; set; }
    public Action onMaouseCanceled { get; set; }
    private bool getInputs = false;
    private bool isDown = false;
    Camera mainCamera;
    Ray ray;
    RaycastHit hit;
    private Action clickListner;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!HCStandards.Game.IsGameStarted || !getInputs)
            return;

        GetInputs();
    }

    private void GetInputs()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && !isDown)
        {
            isDown = true;
        }

        if (Input.GetMouseButton(0) && isDown)
        {
            ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (EventSystem.current.IsPointerOverGameObject())
            {
                isDown = false;
                if (onMaouseCanceled != null)
                    onMaouseCanceled();
                return;
            }

            // Raycast against only the specified layer
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Vector3 hitPoint = hit.point;
                if (onMouseHold != null)
                    onMouseHold(hitPoint);
            }
        }

        if (Input.GetMouseButtonUp(0) && isDown)
        {
            isDown = false;

            ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            // Raycast against only the specified layer
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Vector3 hitPoint = hit.point;
                if (onMouseDown != null)
                    onMouseDown(hitPoint);
                if (clickListner != null)
                {
                    clickListner.Invoke();
                    clickListner = null;
                }
            }

        }
    }

    private void InputChanged(bool obj)
    {
        isDown = false;
        getInputs = obj;
        if (obj)
        {
            if (onMaouseCanceled != null)
                onMaouseCanceled();
        }
    }

    private void ListnerAdded(Action obj)
    {
        clickListner = obj;
    }

    private void OnEnable()
    {
        InputManager.instance.onInputChanged += InputChanged;
        InputManager.instance.onClicklistnerAdd += ListnerAdded;
    }


    private void OnDisable()
    {
        if (InputManager.instance != null)
            InputManager.instance.onInputChanged -= InputChanged;
    }

}
