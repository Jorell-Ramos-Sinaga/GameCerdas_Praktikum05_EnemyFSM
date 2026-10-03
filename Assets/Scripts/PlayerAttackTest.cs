using UnityEngine;

public class PlayerAttackTest : MonoBehaviour
{
    [SerializeField] private EnemyHealth enemy;
    [SerializeField] private float attackDistance = 3f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private Animator animator;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryAttackEnemy();
        }
    }

    private void TryAttackEnemy()
    {
        if (animator != null)
        {
            animator.Play("f_melee_combat_attack_A");
        }

        if (enemy == null)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                enemy.transform.position
            );

        if (distance <= attackDistance)
        {
            enemy.TakeDamage(damage);

            Debug.Log(
                "Player attacks Enemy!"
            );
        }
    }
}