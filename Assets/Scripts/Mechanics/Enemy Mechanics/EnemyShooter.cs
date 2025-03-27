using UnityEngine;
using System.Collections;

public class EnemyShooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float shootInterval = 2f;
    public float shootDelay = 4f; // Delay before shooting + looking at player

    private float nextShootTime = 0f;
    private Transform player;
    private bool shootingEnabled = false;
    private bool canLookAtPlayer = false; // Added flag to delay looking at the player

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        StartCoroutine(StartShootingAndLookingAfterDelay());
    }

    void Update()
    {
        if (player == null) return;

        // Only start looking at player after delay
        if (canLookAtPlayer)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        if (!shootingEnabled || !SimpleQuestManager.Instance.missionAccepted) return;

        if (Time.time >= nextShootTime)
        {
            Shoot();
            nextShootTime = Time.time + shootInterval;
        }
    }

    void Shoot()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            Vector3 direction = (player.position - firePoint.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, lookRotation);
            proj.tag = "EnemyProjectile";
        }
    }

    IEnumerator StartShootingAndLookingAfterDelay()
    {
        yield return new WaitForSeconds(shootDelay);
        shootingEnabled = true;
        canLookAtPlayer = true; // Enemy now starts looking at the player after the same delay
    }
}
