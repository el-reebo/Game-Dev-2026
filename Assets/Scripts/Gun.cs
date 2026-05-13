using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]

// Code taken and tweaked from Youtube tutorial https://www.youtube.com/watch?v=cI3E7_f74MA
public class Gun : MonoBehaviour
{
    [Header("UI Text")]
    public Text ReserveAmmoText;
    public Text InMagText;

    [Header("Ammo Values")]
    public int MagSize = 2;
    public int BulletsInMag = 2;
    public int ReserveAmount = 0;

    [Header("Gun Settings")]
    [SerializeField] private int NumBulletsPerShot = 1;
    [SerializeField] private Vector3 BulletSpreadVariance = new Vector3( 0f, 0f, 0f );
    [SerializeField] private float VerticalBulletAngle = 0f;
    [SerializeField] private float HorizontalBulletAngle = 0f;
    [SerializeField] private ParticleSystem ShootingSystem;
    [SerializeField] private Transform BulletOrigin;
    [SerializeField] private GameObject ImpactParticleSystem;
    [SerializeField] private TrailRenderer BulletTrail;
    [SerializeField] private float ShootDelay;
    [SerializeField] private float BulletSpeed;
    [SerializeField] private float ReloadDuration = 3f;
    [SerializeField] private LayerMask Mask; // Where bullets can hit

    [SerializeField] private Animator gunController;
    [SerializeField] private Transform Camera;

    [Header("Sound Settings")]
    [SerializeField] private AudioClip ShootSFX = null;
    [SerializeField] private float SoundRange = 200f;

    public event System.Action GunShot;

    private AudioSource GunAudioSource;

    private float lastShootTime;
    private bool isReloading = false;

    void Start()
    {
        GunAudioSource = GetComponent<AudioSource>();
        ReserveAmmoText.text = string.Format("{0:00}", ReserveAmount);
        InMagText.text = BulletsInMag.ToString();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, SoundRange);
    }

    private void PlayShootAudio()
    {
        GunAudioSource.PlayOneShot(ShootSFX, 0.8f);

        var sound = new Sound(transform.position, SoundRange, 100);
        Sounds.MakeSound(sound);
    }

    private Vector3 GetDirection(Transform T)
    {
        // Set shoot direction to gun blue axis direction
        Vector3 direction = T.forward;

        // Apply bullet angle adjustments
        direction = Quaternion.AngleAxis(VerticalBulletAngle, transform.right) *
                    Quaternion.AngleAxis(HorizontalBulletAngle, transform.up) * direction;


        // Apply random bullet spread
        direction += new Vector3(
            Random.Range(-BulletSpreadVariance.x, BulletSpreadVariance.x),
            Random.Range(-BulletSpreadVariance.y, BulletSpreadVariance.y),
            Random.Range(-BulletSpreadVariance.z, BulletSpreadVariance.z)
        );

        direction.Normalize();

        return direction;
    }

    public void Reload()
    {
        
        if (isReloading) return;
        if (BulletsInMag >= MagSize) return; 
        if (ReserveAmount == 0) return;

        Debug.Log("Reloading...");
        StartCoroutine(ReloadCoroutine());

        Debug.Log($"{BulletsInMag} in the chamber");
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        gunController.SetTrigger("Reloading");

        yield return new WaitForSeconds(ReloadDuration);

        while (ReserveAmount > 0 && BulletsInMag < MagSize)
        {
            BulletsInMag++;
            ReserveAmount--;
        }

        // Set UI
        InMagText.text = BulletsInMag.ToString();
        ReserveAmmoText.text = string.Format("{0:00}", ReserveAmount);

        isReloading = false;
        Debug.Log("Finished Reloading");
    }

    public void Shoot()
    {
        if (isReloading) return;
        if (BulletsInMag <= 0)
        {
            Debug.Log("Reload");
            return;
        }

        if (lastShootTime + ShootDelay < Time.time)
        {
            //Debug.Log("Shooting Now");
            gunController.SetTrigger("Shooting");
            GunShot?.Invoke(); 
            ShootingSystem.Play();
            PlayShootAudio();
            BulletsInMag -= 1;

            // Update UI
            InMagText.text = BulletsInMag.ToString();

            for (int i = 0; i < NumBulletsPerShot; i++)
            {
                Vector3 direction = GetDirection(transform);

                if (Physics.Raycast(Camera.position, direction, out RaycastHit hit, 1000f, Mask))
                {

                    GameObject impact = Instantiate(ImpactParticleSystem, hit.point, Quaternion.LookRotation(hit.normal));

                    Destroy(impact, 1);

                    // Coroutine allows multi-frame sequencing for animating bullet tracer
                    if (Vector3.Distance(Camera.position, BulletOrigin.position) < Vector3.Distance(Camera.position, hit.point))
                    {
                        TrailRenderer trail = Instantiate(BulletTrail, BulletOrigin.position, Quaternion.identity);
                        StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, true));
                        Destroy(trail.gameObject, 2);
                    }
                    // Debug.Log($"Hit obj: {hit.transform.name}");

                    IDamageable obj = hit.transform.gameObject.GetComponentInParent<IDamageable>();
                    if (obj != null)
                    {   
                        Debug.Log("Hit IDamageable obj");
                        obj.TakeDamage();
                    }
                }
                else
                {
                    TrailRenderer trail = Instantiate(BulletTrail, BulletOrigin.position, Quaternion.identity);

                    StartCoroutine(SpawnTrail(trail, GetDirection(transform) * 100, Vector3.zero, false));
                    Destroy(trail.gameObject, 2);
                }
            }
            lastShootTime = Time.time;
        }
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
        Trail.transform.position = HitPoint;

        // Only play bullet impact when bullet made impact
        //if (MadeImpact)
        //{
        //    GameObject impact = Instantiate(ImpactParticleSystem, HitPoint, Quaternion.LookRotation(HitNormal));

        //    Destroy(impact, 1);
        //}

        Destroy(Trail.gameObject, Trail.time);
    }
}
