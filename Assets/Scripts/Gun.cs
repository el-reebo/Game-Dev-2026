using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]

// Code taken and tweaked from Youtube tutorial https://www.youtube.com/watch?v=cI3E7_f74MA
public class Gun : MonoBehaviour
{

    [Header("Gun Settings")]
    [SerializeField] private int NumBulletsPerShot = 1;
    [SerializeField] private Vector3 BulletSpreadVariance = new Vector3( 0f, 0f, 0f );
    [SerializeField] private ParticleSystem ShootingSystem;
    [SerializeField] private Transform BulletOrigin;
    [SerializeField] private ParticleSystem ImpactParticleSystem;
    [SerializeField] private TrailRenderer BulletTrail;
    [SerializeField] private float ShootDelay;
    [SerializeField] private float BulletSpeed;
    [SerializeField] private LayerMask Mask; // Where bullets can hit

    [SerializeField] private Animator gunController;
    private float lastShootTime;

    public void Shoot()
    {
        if (lastShootTime + ShootDelay < Time.time)
        {
            //Debug.Log("Shooting Now");
            gunController.SetBool("Shooting", true);
            ShootingSystem.Play();

            Vector3 direction = GetDirection();

            if (Physics.Raycast(BulletOrigin.position, direction, out RaycastHit hit, float.MaxValue, Mask))
            {
                TrailRenderer trail = Instantiate(BulletTrail, BulletOrigin.position, Quaternion.identity);

                // Coroutine allows multi-frame sequencing for animating bullet tracer
                StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, true));
            }
            else
            {
                TrailRenderer trail = Instantiate(BulletTrail, BulletOrigin.position, Quaternion.identity);

                StartCoroutine(SpawnTrail(trail, GetDirection() * 100, Vector3.zero, false));
            }

            lastShootTime = Time.time;
        }
    }

    private Vector3 GetDirection()
    {
        Vector3 direction = transform.forward;

        direction += new Vector3(
            Random.Range(-BulletSpreadVariance.x, BulletSpreadVariance.x),
            Random.Range(-BulletSpreadVariance.y, BulletSpreadVariance.y),
            Random.Range(-BulletSpreadVariance.z, BulletSpreadVariance.z)
        );

        direction.Normalize();

        return direction;
    }

    private IEnumerator SpawnTrail(TrailRenderer Trail, Vector3 HitPoint, Vector3 HitNormal, bool MadeImpact)
    {

        Vector3 startPosition = Trail.transform.position;
        float distance = Vector3.Distance(startPosition, HitPoint);
        float remainingDistance = distance;

        while (remainingDistance > 0)
        {
            // Calculates path from start to hit, with time being percentage of journey
            Trail.transform.position = Vector3.Lerp(startPosition, HitPoint, 1 - (remainingDistance/distance));
            remainingDistance -= BulletSpeed * Time.deltaTime; // bullet speed * time inbetween frames

            yield return null;
        }
        gunController.SetBool("Shooting", false);
        Trail.transform.position = HitPoint;

        // Only play bullet impact when bullet made impact
        if (MadeImpact)
        {
            Instantiate(ImpactParticleSystem, HitPoint, Quaternion.LookRotation(HitNormal));
        }
        

        Destroy(Trail.gameObject, Trail.time);
    }
}
