using UnityEngine;

public class SpawnAmmo : MonoBehaviour
{
    public GameObject ammo;
    public int SpawnAmount = 3;
    public int MaxAmmoInWorld = 4;
    public float radius = 80f;

    [Header("Public Variables")]
    public int AmmoInWorld = 0;

    void Start()
    {
        Spawn();
    }

    public void Spawn()
    {
        int i = 0;
        while (i < SpawnAmount && AmmoInWorld < MaxAmmoInWorld)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * radius;
            UnityEngine.AI.NavMeshHit hit;

            if (UnityEngine.AI.NavMesh.SamplePosition(randomPos, out hit, 2f, UnityEngine.AI.NavMesh.AllAreas))
            {
                GameObject obj = Instantiate(ammo, hit.position, Quaternion.identity);
                obj.GetComponent<AmmoItem>().spawnAmmo = this; // assign this script to spawnAmmo variable

                i++;
                AmmoInWorld++;
            }
        }
    }
}
