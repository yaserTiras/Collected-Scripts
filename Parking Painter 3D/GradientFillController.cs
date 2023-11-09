using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI.Procedural;

public class GradientFillController : MonoBehaviour
{
    [SerializeField] private ParticleSystem vfxFill;
    [SerializeField] private RectTransform fillComponent;
    [SerializeField] private ImageModifier imageFill;
    [SerializeField] private RectTransform rectVfxFill;
    Tween tweenDecreaseFill;
    Tween tweenFillBarPopup;
    float width;
    private float fill;

    public void OnVehiclePassed()
    {
        if (fillComponent.gameObject.activeInHierarchy)
        {
            if (fill >= 1f)
                return;
            if (!HCStandards.Game.IsGameStarted)
                return;
            fill += GlobalSettings.instance.vehiclePassFillAmount;
            ProceedFill();
        }
        else
        {
            if (fillComponent.gameObject.activeInHierarchy)
                return;
            ActivateBar(true);
        }
    }



    private void ActivateBar(bool setTo)
    {
        if (setTo)
        {
            if (tweenFillBarPopup.IsActive())
                tweenFillBarPopup.Kill();
            vfxFill.transform.localPosition = Vector3.zero;
            fillComponent.localScale = Vector3.zero;
            fillComponent.gameObject.SetActive(setTo);
            tweenFillBarPopup = fillComponent.DOScale(Vector3.one * GlobalSettings.instance.rainbowBarPopupValue, GlobalSettings.instance.fillBarActivateDuration).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                fillComponent.DOScale(Vector3.one, GlobalSettings.instance.fillBarActivateDuration).SetEase(Ease.InQuad).onComplete += ProceedFill;
            });
        }
        else
        {
            if (tweenFillBarPopup.IsActive())
                tweenFillBarPopup.Kill();
            fill = 0f;
            imageFill.fillAmount = 0f;
            tweenFillBarPopup = fillComponent.DOScale(Vector3.one * GlobalSettings.instance.rainbowBarPopupValue, GlobalSettings.instance.fillBarActivateDuration * 0.4f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                fillComponent.DOScale(Vector3.zero, GlobalSettings.instance.fillBarActivateDuration).SetEase(Ease.InQuad).OnComplete(() =>
                {
                    fill = 0f;
                    imageFill.fillAmount = 0f;
                    UpdateFill();
                    fillComponent.gameObject.SetActive(setTo);
                });
            });
        }
    }

    private void ProceedFill()
    {
        if (tweenDecreaseFill.IsActive())
            tweenDecreaseFill.Kill();
        if (!vfxFill.isEmitting)
            vfxFill.Play();

        tweenDecreaseFill = DOTween.To(x => fill = x, imageFill.fillAmount, fill, GlobalSettings.instance.vehiclePassFillDuration).SetEase(Ease.Linear).OnUpdate(() =>
        {
            imageFill.fillAmount = fill;
            UpdateFill();
        }).OnComplete(() =>
        {
            if (fill >= 0.99f)
            {
                ActivateBar(false);
                if (HCStandards.Game.IsGameStarted)
                {
                    InputManager.instance.SetInput(false);
                    ColorsController.instance.RainbowBarFilled(rectVfxFill);
                }

            }
            else
            {
                DecreaseFill();
            }
        });
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            OnVehiclePassed();
    }
#endif

    private void DecreaseFill()
    {
        fill -= GlobalSettings.instance.vehiclePassFillAmount;
        tweenDecreaseFill = DOTween.To(x => fill = x, imageFill.fillAmount, 0f, GlobalSettings.instance.fillDecreaseDuration).SetDelay(1f).SetEase(Ease.Linear).OnUpdate(() =>
        {
            imageFill.fillAmount = fill;
            UpdateFill();

        }).OnComplete(() =>
        {
            ActivateBar(false);
        });
    }

    private void UpdateFill()
    {
        if (fill <= 0.05f && vfxFill.isEmitting)
        {
            vfxFill.Stop(true);
        }
        width = imageFill.rectTransform.rect.width;
        Vector3 tempV = rectVfxFill.anchoredPosition;
        tempV.x = -width / 2;
        tempV.x += width * imageFill.fillAmount;
        rectVfxFill.anchoredPosition = tempV;
    }

    private void GameEnded(bool win)
    {
        if (tweenDecreaseFill.IsPlaying())
            tweenDecreaseFill.Kill();
        if (tweenFillBarPopup.IsActive())
            tweenFillBarPopup.Kill();
        ActivateBar(false);
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        VehiclesController.instance.onVehiclePassed += OnVehiclePassed;
        HCStandards.Game.onGameEnded += GameEnded;
    }

    private void OnDisable()
    {
        if (tweenDecreaseFill.IsActive())
            tweenDecreaseFill.Kill();
        if (tweenFillBarPopup.IsActive())
            tweenFillBarPopup.Kill();
        if (VehiclesController.instance != null)
            VehiclesController.instance.onVehiclePassed -= OnVehiclePassed;
        HCStandards.Game.onGameEnded -= GameEnded;
    }
}
