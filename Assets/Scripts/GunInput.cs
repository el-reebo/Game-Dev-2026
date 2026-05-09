using UnityEngine;

// Code taken from Youtube video https://www.youtube.com/watch?v=cI3E7_f74MA
public class GunInput : MonoBehaviour
{
    [SerializeField] private Gun gun;
    [SerializeField] private PlayerInputHandler pih;

    private bool lastShootState;
    private bool lastReloadState;

    void Update()
    {
        // Fire gun only once if button held
        if (pih.ShootInput && !lastShootState)
        {
            gun.Shoot();
        }

        if (pih.ReloadInput && !lastReloadState)
        {
            gun.Reload();
        }

        lastReloadState = pih.ReloadInput;
        lastShootState = pih.ShootInput;
    }
}
