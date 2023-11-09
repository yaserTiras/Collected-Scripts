using UnityEngine;
using UnityEngine.EventSystems;

public class InputHandler : MonoBehaviour
{

    void Update()
    {
        if (!PanelsManager.isPanelOpen)
            return;
        GetInputs();
    }

    private void GetInputs()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PanelsManager.PointerDown(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            PanelsManager.PointerUp(Input.mousePosition);
        }
    }

}
