using DG.Tweening;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Procedural;

public class SlotItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private RectTransform myRectTransform;
    [SerializeField] private RectTransform rectParent;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private ImageModifier image;
    [SerializeField] private PlantType plantType;
    public PlantType PlantType { get => plantType; }
    public Action onPlaced;
    public Action onMerged;
    public Action<SlotItem> onDestroyed;
    private Vector3 initialPosition;

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = false;
        myRectTransform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        myRectTransform.position = eventData.position;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;
        SlotItem item = eventData.pointerDrag.GetComponent<SlotItem>();

        if (CanMerge(item))
        {
            Merge(item);
        }
        else
        {
            item.ResetPosition();
        }
    }

    public async void OnEndDrag(PointerEventData eventData)
    {
        await GoToPosition(initialPosition);
    }

    internal async void Merge(SlotItem item)
    {
        await item.PlaceOnNewSlot(initialPosition);
        item.Disable();
        int current = (int)plantType;
        current++;
        plantType = (PlantType)current;
        image.raycastTarget = false;

        myRectTransform.DOScale(Vector3.zero, GlobalSettings.instance.mergeDuration).SetEase(Ease.InCubic).OnComplete(() =>
        {
            image.sprite = GlobalSettings.instance.items.GetPlantSprite(plantType);
            myRectTransform.DOScale(Vector3.one, GlobalSettings.instance.mergeDuration).SetEase(Ease.InOutExpo).OnComplete(() =>
            {
                image.raycastTarget = true;
                if (onMerged != null)
                    onMerged();
            });
        });
    }

    public void Disable()
    {
        if (!gameObject.activeInHierarchy)
            return;
        if (onPlaced != null)
            onPlaced();
        if (onDestroyed != null)
            onDestroyed(this);
        gameObject.SetActive(false);
        plantType = (PlantType)0;
        image.sprite = GlobalSettings.instance.items.GetPlantSprite(plantType);
    }

    internal async Task PlaceOnNewSlot(Vector3 position)
    {
        initialPosition = position;
        await GoToPosition(position);
        if (onPlaced != null)
            onPlaced();
    }

    private async Task GoToPosition(Vector3 position)
    {
        myRectTransform.DOKill();
        initialPosition = position;
        await myRectTransform.DOMove(position, GlobalSettings.instance.itemPlacementDuration).AsyncWaitForCompletion();
        canvasGroup.blocksRaycasts = true;
    }

    internal async void ResetPosition()
    {
        await GoToPosition(initialPosition);
    }

    internal void Created(Transform parent)
    {
        gameObject.SetActive(false);
        myRectTransform.SetParent(parent, false);
        myRectTransform.localScale = Vector3.zero;
    }

    internal void Spawn(Vector3 targetPosition)
    {
        initialPosition = targetPosition;
        myRectTransform.position = targetPosition;
        image.sprite = GlobalSettings.instance.items.GetPlantSprite(plantType);
        gameObject.SetActive(true);
        myRectTransform.DOScale(Vector3.one * 1.2f, GlobalSettings.instance.itemPopupDuration).SetEase(Ease.OutQuart).OnComplete(() =>
           {
               myRectTransform.DOScale(Vector3.one, GlobalSettings.instance.itemPlacementDuration * 1.5f).SetEase(Ease.OutQuart).OnComplete(() =>
                 {
                     image.raycastTarget = true;
                 });
           });
    }




    private bool CanMerge(SlotItem item)
    {
        if (PlantType == item.PlantType && (int)PlantType < Enum.GetValues(typeof(PlantType)).Length)
            return true;
        return false;
    }
}
