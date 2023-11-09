using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private InputController inputController;
    [SerializeField] private Shooter shooter;
    private float clickingRate = 0f;

    private void FixedUpdate()
    {
        if (!HCStandards.Game.IsGameStarted)
            return;
        if (clickingRate > 0)
            clickingRate -= Time.fixedDeltaTime;
    }

    private void OnClick(Vector3 target)
    {
        if (clickingRate > 0 || !shooter.canShoot)
            return;
        clickingRate = GlobalSettings.instance.ClickingRate;
        shooter.Shoot(target);
        ColorsController.instance.OnColorBulletShooted();
    }

    private void OnHold(Vector3 target)
    {
        if (clickingRate > 0)
        {
            shooter.Cancel();
            return;
        }
        shooter.Aim(target);
    }

    private void OnCanceled()
    {
        shooter.Cancel();
    }

    private void OnEnable()
    {
        inputController.onMouseDown += OnClick;
        inputController.onMouseHold += OnHold;
        inputController.onMaouseCanceled += OnCanceled;
    }

    private void OnDisable()
    {
        inputController.onMouseDown -= OnClick;
        inputController.onMouseHold -= OnHold;
        inputController.onMaouseCanceled -= OnCanceled;
    }
}
