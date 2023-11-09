using DG.Tweening;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI.Procedural;

public class NextColor : MonoBehaviour
{
    private RectTransform tr;
    [SerializeField] private ImageModifier imageModifier;
    [SerializeField] private GameObject background;
    private float popUpSpeed = 1f;
    private VehicleColorValue carColor;

    public async void SetColor(VehicleColorValue color)
    {
        carColor = color;
        if (color != VehicleColorValue.Rainbow && color != VehicleColorValue.ReversDirection)
        {
            imageModifier.color = GlobalSettings.instance.GetColor(color);
        }
        else if (color == VehicleColorValue.Rainbow)
        {
            await Popup(color, 1.1f);
            imageModifier.transform.DOLocalRotate(Vector3.forward * 360f, GlobalSettings.instance.rainbowRotateDuration, RotateMode.LocalAxisAdd).SetLoops(-1).SetEase(Ease.Linear);
        }
        else
        {
            await Popup(color, 1f, ActivateBackground);
            tr.DOScale(1.1f, 0.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
        }
    }

    private async Task Popup(VehicleColorValue color, float endScale, Action function = null)
    {
        await tr.DOScale(1.3f, 0.5f).SetEase(Ease.InCubic).AsyncWaitForCompletion();
        await tr.DOScale(0f, 0.15f).SetEase(Ease.OutCubic).AsyncWaitForCompletion();
        function?.Invoke();
        imageModifier.color = Color.white;
        imageModifier.sprite = GlobalSettings.instance.GetSprite(color);
        await tr.DOScale(endScale, 0.3f).SetEase(Ease.OutQuad).AsyncWaitForCompletion();
    }

    private void ActivateBackground()
    {
        background.SetActive(true);
    }

    public async Task MoveNext(Transform target, bool scaleUp = false)
    {
        tr.DOKill();
        await tr.DOMove(target.position, GlobalSettings.instance.uiColorMoveSpeed).SetEase(Ease.InOutCubic).AsyncWaitForCompletion();
        if (scaleUp)
            ScaleUp();
    }

    private void ScaleUp()
    {
        tr.DOScale(tr.localScale * 1.10f, 0.15f);
    }

    public async Task Kill(Transform target, VehicleColorValue color)
    {
        tr.DOKill();
        imageModifier.DOKill();
        if (background.activeInHierarchy)
            background.SetActive(false);
        await tr.DOScale(Vector3.one * GlobalSettings.instance.colorPopupScale, popUpSpeed * 0.5f).SetEase(Ease.InCubic).OnComplete(() =>
         {
             tr.DOScale(Vector3.zero, popUpSpeed).SetEase(Ease.InOutCubic).OnComplete(() =>
             {
                 Rebuild(target, color);
             });

         }).AsyncWaitForCompletion();


    }

    public void RemovedFromList()
    {
        gameObject.SetActive(false);
    }

    public RectTransform GetRectTransform()
    {
        return tr;
    }

    private void Rebuild(Transform target, VehicleColorValue color)
    {

        if (carColor == VehicleColorValue.Rainbow || carColor == VehicleColorValue.ReversDirection)
        {
            imageModifier.transform.DOKill();
            imageModifier.sprite = GlobalSettings.instance.GetDefaultSprite();
        }
        SetColor(color);
        tr.position = target.position;
        tr.localScale = Vector3.one;
    }

    private void OnEnable()
    {
        popUpSpeed = GlobalSettings.instance.popUpSpeed;
        tr = tr ? tr : GetComponent<RectTransform>();
    }


    private void OnDestroy()
    {
        tr.DOKill();
    }

    internal bool CanSet()
    {
        if (carColor == VehicleColorValue.Rainbow || carColor == VehicleColorValue.ReversDirection)
            return false;
        return true;
    }
}
