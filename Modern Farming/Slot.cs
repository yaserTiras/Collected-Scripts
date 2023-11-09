using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Procedural;

public class Slot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform tr;
    [SerializeField] private ImageModifier imageOutline;
    private SlotItem item;


    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;
        SlotItem item = eventData.pointerDrag.GetComponent<SlotItem>();
        if (this.item == null)
        {
            ItemPlaced(item, true);
        }
        else
        {
            imageOutline.color = Color.white;
        }
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

    public void OnPointerUp(PointerEventData eventData)
    {
        imageOutline.color = Color.white;
    }

    public async void ItemPlaced(SlotItem item, bool snapPosition)
    {
        this.item = item;
        if (snapPosition)
           await item.PlaceOnNewSlot(tr.position);
        item.onPlaced += OnItemOut;
        imageOutline.color = Color.white;
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

    }

    internal bool IsEmpty()
    {
        if (item == null)
            return true;
        else
            return false;
    }

}
