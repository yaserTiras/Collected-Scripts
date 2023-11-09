using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class PanelsManager
{
    private static List<Ipanel> panels = new List<Ipanel>();
    public static bool isPanelOpen { get; private set; }
    private static bool isDown = false;

    public static void PointerDown(Vector3 mousePosition)
    {
        isDown = true;
    }

    public static void PointerUp(Vector3 mousePosition)
    {
        if (!isDown)
            return;
        Vector2 localMousePos;
        int lastPanel = panels.Count - 1;
        RectTransform rect = panels[lastPanel].panelRect;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, mousePosition, null, out localMousePos);

        if (!RectTransformUtility.RectangleContainsScreenPoint(rect, mousePosition))
        {
            panels[lastPanel].Close();
        }

    }

    public static void Register(Ipanel panel)
    {
        if (panels.Count == 0)
        {
            isPanelOpen = true;
        }
        else
        {
            int lastPanel = panels.Count - 1;
            panels[lastPanel].Enable(false);
        }
        panels.Add(panel);
    }

    public static void Remove(Ipanel panel)
    {
        panels.Remove(panel);
        if (panels.Count == 0)
        {
            isPanelOpen = false;
        }
        else
        {
            int lastPanel = panels.Count - 1;
            panels[lastPanel].Enable(true);
        }
    }

    public static void ClearPanels()
    {
        if (panels.Count == 0)
            return;
        panels.Clear();
    }

    public static void CreateDynamicPanel()
    {
        CreateCanvas();

        GameObject obj = new GameObject("Custom Panel", typeof(RectTransform));

        RectTransform rectTransform = obj.GetComponent<RectTransform>();
        SetParent(obj, CustomType.UI);
        if (rectTransform != null)
        {
            rectTransform.anchorMin = Vector2.one * 0.5f;
            rectTransform.anchorMax = Vector2.one * 0.5f;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(100f, 100f);
        }

        SetupObjectInHierarchy(obj, CustomType.UI);
    }

    private static Canvas CreateCanvas()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();

        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        return canvas;
    }

    private static void SetParent(GameObject obj, CustomType type)
    {
        Transform tr = null;

        switch (type)
        {
            case CustomType.Object:
                break;
            case CustomType.UI:
                if (Selection.gameObjects.Length == 0)
                {
                    tr = CreateCanvas().transform;
                }
                else
                {
                    tr = Selection.transforms[0];
                    //Transform canvasTr = tr.GetComponentInParent<Canvas>().transform;
                }
                
                break;
            default:
                break;
        }
        obj.transform.SetParent(tr, false);
    }

    private static void SetupObjectInHierarchy(GameObject obj, CustomType type)
    {
        obj.AddComponent<Panel>();
        SceneView view = SceneView.lastActiveSceneView;
        if (!view)
        {
            Debug.LogError("Scene Error");
            return;
        }

        obj.transform.localPosition = Vector3.zero;

        StageUtility.PlaceGameObjectInCurrentStage(obj);
        GameObjectUtility.EnsureUniqueNameForSibling(obj);

        Undo.RegisterCreatedObjectUndo(obj, obj.name);
        Selection.activeObject = obj;

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

    }
}

public static class CreateUtility
{
    [MenuItem("GameObject/UI/Custom Panel")]
    public static void CreatePanel()
    {
        PanelsManager.CreateDynamicPanel();
    }
}

public enum CustomType
{
    Object, UI
}
