using UnityEngine;

public class ShootingCombat : MonoBehaviour
{
    public bool canShoot = false;

    public int heldGunDamage = 1;

    public Vector3 reticleOffset;

    public GameObject bulletVisualsPrefab;
    public Transform spawnOriginOffset;

    void Update()
    {
        if (!canShoot)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        RaycastHit hit;
        Camera renderingCamera = Camera.main;
        Ray ray = renderingCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        ray.origin += renderingCamera.transform.TransformDirection(reticleOffset);

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.GetComponent<CombatEntity>() != null)
            {
                hit.collider.gameObject.GetComponent<CombatEntity>().LoseHealth(heldGunDamage);
            }
        }
        GameObject bulletVisuals = Instantiate(bulletVisualsPrefab, transform.position + transform.rotation * spawnOriginOffset.position, transform.rotation);
        bulletVisuals.GetComponent<AutomaticMovementScript>().target = hit.point;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + transform.rotation * spawnOriginOffset.position, 0.1f);
    }
}
