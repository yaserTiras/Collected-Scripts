using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class Panel : MonoBehaviour, Ipanel
{
    private RectTransform rectTrasform;
    private Image image;

    public RectTransform panelRect
    {
        get
        {
            if (!rectTrasform)
                rectTrasform = GetComponent<RectTransform>();
            return rectTrasform;
        }
    }

    public Image panelImage
    {
        get
        {
            if (!image)
                image = GetComponent<Image>();
            return image;
        }
    }


    public void Open()
    {
        PanelsManager.Register(this);
        // Animate enable 
        gameObject.SetActive(true);
    }

    public void Close()
    {
        PanelsManager.Remove(this);
        // Animate disable 
        gameObject.SetActive(false);
    }
}
