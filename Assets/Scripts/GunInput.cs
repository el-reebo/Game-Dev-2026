using UnityEngine;

// Code taken from Youtube tutorial https://www.youtube.com/watch?v=cI3E7_f74MA
public class GunInput : MonoBehaviour
{
    [SerializeField] private Gun gun;
    [SerializeField] private PlayerInputHandler pih;

    private bool lastShootState;

    void Update()
    {
        // Fire gun only once if button held
        if (pih.ShootInput && !lastShootState)
        {
            gun.Shoot();
        }

        lastShootState = pih.ShootInput;
    }
}
