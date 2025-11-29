using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(DamageReciever))]
public class KnockbackOnHit : MonoBehaviour, IHitReactable
{
    private Rigidbody rb;
    private EnemyBaseAI aiBase;
    private bool isStunned = false;

    [Header("Knockback")]
    [SerializeField] private float postKnockbackDamping = 0.05f;
    // how aggressively we kill leftover velocity after stun (0 = instant stop, 1 = no damping)

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        aiBase = GetComponent<EnemyBaseAI>();
    }

    public void OnHit(RaycastHit hitInfo)
    {
        var weapon = WeaponManager.CurrentWeapon;
        if (weapon == null) return;

        Vector3 dir = (transform.position - hitInfo.point).normalized;
        dir = (dir + Vector3.up * 0.2f).normalized;

        rb.AddForce(dir * weapon.knockbackForce, ForceMode.VelocityChange);

        if (aiBase != null)
            StartCoroutine(StunRoutine(weapon.hitStunDuration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        if (isStunned) yield break;

        isStunned = true;
        aiBase.SetStunned(true);

        yield return new WaitForSeconds(duration);

        aiBase.SetStunned(false);
        isStunned = false;

        // Remove leftover knockback velocity so AI regains clean movement control
        rb.linearVelocity *= postKnockbackDamping;
    }
}
