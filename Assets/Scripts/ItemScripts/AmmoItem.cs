using UnityEngine;

public class AmmoItem : MonoBehaviour
{
    public int AmmoCount = 3;
    public SpawnAmmo spawnAmmo;
    public AudioClip pickupSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Collided with: {other.gameObject.name}");
        Gun gun = other.GetComponent<GunInput>().gun;
        if (gun == null) return;

        gun.ReserveAmount += AmmoCount;

        gun.ReserveAmmoText.text = string.Format("{0:00}", gun.ReserveAmount);

        spawnAmmo.AmmoInWorld--;

        audioSource.PlayOneShot(pickupSound);

        Destroy(gameObject, pickupSound.length);
    }
}
