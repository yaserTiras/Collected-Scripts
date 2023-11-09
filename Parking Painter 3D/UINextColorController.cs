using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UINextColorController : MonoBehaviour
{
    [SerializeField] private List<RectTransform> colorPoints;
    [SerializeField] private List<NextColor> nextColors;
    [SerializeField] private int activeColorsCount;

    private bool isSwapping = false;
    private IEnumerator Start()
    {
        if (!ColorsController.instance.IsColorsLoaded())
            yield break;
        yield return new WaitForEndOfFrame();
        ColorsLoaded(ColorsController.instance.GetColors());
    }

    public async void OnColorsSwapped(VehicleColorValue color)
    {
        isSwapping = true;
        int lastIndex = activeColorsCount - 1;
        await nextColors[0].Kill(colorPoints[lastIndex], color);
        await nextColors[1].MoveNext(colorPoints[0], true);
        await nextColors[2].MoveNext(colorPoints[1]);
        SwapNextColors();
    }

    private void SwapNextColors()
    {
        NextColor nextColor = nextColors[0];
        for (int i = 0; i < activeColorsCount - 1; i++)
        {
            nextColors[i] = nextColors[i + 1];
        }
        int lastIndext = nextColors.Count - 1;
        nextColors[lastIndext] = nextColor;
        isSwapping = false;
    }

    private void ColorsLoaded(List<VehicleColorValue> nextColors)
    {
        activeColorsCount = nextColors.Count;
        int countToRemove = this.nextColors.Count - nextColors.Count;
        int lastElement = this.nextColors.Count - 1;

        for (int i = 0; i < countToRemove; i++)
        {
            this.nextColors[lastElement].RemovedFromList();
            this.nextColors.RemoveAt(lastElement);
            lastElement--;
        }

        for (int i = 0; i < nextColors.Count; i++)
        {
            this.nextColors[i].SetColor(nextColors[i]);
        }
    }

    private void ColorModified(VehicleColorValue color, int index)
    {
        nextColors[index].SetColor(color);
    }

    private RectTransform BarFilled(int index)
    {
        return nextColors[index].GetRectTransform();
    }

    private bool IsSwapping()
    {
        return isSwapping;
    }

    private void OnEnable()
    {
        ColorsController.instance.onSwapped += OnColorsSwapped;
        ColorsController.instance.onColorsLoaded += ColorsLoaded;
        ColorsController.instance.onModified += ColorModified;
        ColorsController.instance.onPrizeReady += BarFilled;
        ColorsController.instance.onSwappingProcces += IsSwapping;
    }

    private void OnDisable()
    {
        if (ColorsController.instance == null)
            return;
        ColorsController.instance.onSwapped -= OnColorsSwapped;
        ColorsController.instance.onColorsLoaded -= ColorsLoaded;
        ColorsController.instance.onSwappingProcces -= IsSwapping;
        ColorsController.instance.onModified -= ColorModified;
        ColorsController.instance.onPrizeReady -= BarFilled;
    }
}
