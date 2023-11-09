using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Procedural;
using static UnityEngine.UI.Toggle;

public class PlantingPlant : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform tr;
    [SerializeField] private ImageModifier imageOutline;

    public SlotItem item;
    public ToggleEvent onVariableChanged;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        SlotItem item = eventData.pointerDrag.GetComponent<SlotItem>();
        ItemPlaced(item, true);

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;
        if (!eventData.pointerDrag.CompareTag("SlotItem"))
            return;
        SlotItem item = eventData.pointerDrag.GetComponent<SlotItem>();
        if (item == this.item)
            return;
        if (CanPlace(item))
        {
            imageOutline.color = Color.green;
        }
        else
        {
            imageOutline.color = Color.red;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;
        imageOutline.color = Color.white;
    }

    public async void ItemPlaced(SlotItem item, bool snapPosition)
    {
        this.item = item;
        if (snapPosition)
            await item.PlaceOnNewSlot(tr.position);
        item.onPlaced += OnItemOut;
        imageOutline.color = Color.white;
        onVariableChanged?.Invoke(false);
    }

    private bool CanPlace(SlotItem item)
    {
        if (this.item == null)
            return true;
        if (this.item.PlantType == item.PlantType && (int)this.item.PlantType < Enum.GetValues(typeof(PlantType)).Length)
            return true;
        return false;
    }

    private void OnItemOut()
    {
        item.onPlaced -= OnItemOut;
        item = null;
        imageOutline.color = Color.white;
        onVariableChanged?.Invoke(true);

    }

    internal bool IsEmpty()
    {
        if (item == null)
            return true;
        else
            return false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        imageOutline.color = Color.white;
    }

    internal void DisableItem()
    {
        if (item != null)
            item.Disable();
    }

    public void ContinuePlanting()
    {
        UIManager.instance.DisableMergePanel(item.PlantType);
    }
}
