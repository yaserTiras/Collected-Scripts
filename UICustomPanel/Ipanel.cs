using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public interface Ipanel
{
    public Image panelImage { get; }
    public RectTransform panelRect { get; }

    void Open();
    void Close();

    void Enable(bool v)
    {
        panelImage.raycastTarget = v;
    }

}
