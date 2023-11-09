using DG.Tweening;
using MoreMountains.NiceVibrations;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    [SerializeField] private Transform magazine;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private List<ColorBullet> bullets;
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer1;
    int currentBullet;
    Transform tr;
    float val;
    Sequence seq;
    Material[] mats = new Material[2];


    private void Start()
    {
        Build();
        tr = transform;
    }
    private void Build()
    {
        for (int i = 0; i < bullets.Count; i++)
        {
            bullets[i].Setup(magazine, bulletSpeed);
        }
    }

    private void ColorsLoaded(List<VehicleColorValue> obj)
    {
        for (int i = 0; i < obj.Count; i++)
        {
            bullets[i].SetColor(obj[i]);
        }
        mats = skinnedMeshRenderer1.sharedMaterials;
        mats[0] = GlobalSettings.instance.GetMaterial(obj[0]);
        skinnedMeshRenderer1.sharedMaterials = mats;
    }
    public bool canShoot = true;

    internal async void Shoot(Vector3 target)
    {

        canShoot = false;
        HCStandards.Haptics.PlayHaptic(HapticTypes.LightImpact);
        int bullet = currentBullet;
        currentBullet++;
        if (currentBullet == bullets.Count)
            currentBullet = 0;
        tr.DOLookAt(target, 0.2f);
        float duration = (val / 200f) * 0.1f;
        duration = Mathf.Clamp(duration, 0.05f, 0.1f);
        seq = DOTween.Sequence();
        AudioManager.instance.PlayAudio(AudioType.Shoot);
        seq.Append(DOTween.To(() => val, x => val = x, 100f, duration))
               .Append(DOTween.To(() => val, x => val = x, 0f, duration)).OnUpdate(() =>
               {
                   skinnedMeshRenderer.SetBlendShapeWeight(0, val);
                   skinnedMeshRenderer1.SetBlendShapeWeight(0, val);
               });
        await seq.AsyncWaitForCompletion();
        val = 0f;
        bullets[bullet].Launch(target);
        canShoot = true;
    }

    private void OnLaunched(VehicleColorValue nextColor)
    {
        bullets[currentBullet].SetColor(nextColor);
        mats[0] = GlobalSettings.instance.GetMaterial(nextColor);
        skinnedMeshRenderer1.sharedMaterials = mats;
    }


    internal void Aim(Vector3 target)
    {
        if (val == 0 && val != 100f)
        {
            seq = DOTween.Sequence();
            seq.Append(DOTween.To(() => val, x => val = x, 100f, 0.05f))
                .OnUpdate(() =>
                   {
                       skinnedMeshRenderer.SetBlendShapeWeight(0, val);
                       skinnedMeshRenderer1.SetBlendShapeWeight(0, val);
                   });
        }
        Vector3 dir = target - tr.position;
        tr.rotation = Quaternion.Slerp(tr.rotation, Quaternion.LookRotation(dir.normalized), 10f * Time.deltaTime);
    }


    internal void Cancel()
    {
        if (val == 0)
            return;
        seq = DOTween.Sequence();
        seq.Append(DOTween.To(() => val, x => val = x, 0f, 0.05f))
            .OnUpdate(() =>
            {
                skinnedMeshRenderer.SetBlendShapeWeight(0, val);
                skinnedMeshRenderer1.SetBlendShapeWeight(0, val);
            });
    }

    private void OnModified(VehicleColorValue arg1, int index)
    {
        if (index > 0)
            return;
        bullets[currentBullet+index].SetColor(arg1);
        mats[0] = GlobalSettings.instance.GetMaterial(arg1);
        skinnedMeshRenderer1.sharedMaterials = mats;
    }

    private void OnEnable()
    {
        ColorsController.instance.onColorsLoaded += ColorsLoaded;
        ColorsController.instance.onLaunched += OnLaunched;
        ColorsController.instance.onModified += OnModified;
    }


    private void OnDisable()
    {
        if (ColorsController.instance == null)
            return;
        ColorsController.instance.onColorsLoaded -= ColorsLoaded;
        ColorsController.instance.onLaunched -= OnLaunched;
        ColorsController.instance.onModified -= OnModified;
    }

}
