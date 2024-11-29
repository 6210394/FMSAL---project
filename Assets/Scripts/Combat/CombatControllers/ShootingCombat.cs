using UnityEngine;

public class ShootingCombat : MonoBehaviour
{
    public bool canShoot = false;

    public int heldGunDamage = 1;

    public GameObject bulletVisualsPrefab;
    public Vector3 spawnOriginOffset;

    void Update()
    {
        if (!canShoot)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Shoot(Camera.main.ScreenToWorldPoint(Input.mousePosition), heldGunDamage);
        }
    }

    public void Shoot(Vector3 target, int damageToPassOnToBullet)
    {
        GameObject bullet = Instantiate(bulletVisualsPrefab, transform.position + transform.rotation * spawnOriginOffset, transform.rotation);
        bullet.GetComponent<AutomaticMovementScript>().target = target;
        bullet.GetComponent<Hitbox>().damage = damageToPassOnToBullet;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + transform.rotation * spawnOriginOffset, 0.1f);
    }
}
