using System.Collections;
using UnityEngine;

namespace Common.BossAttacks
{
    public class ThirdBoss : MonoBehaviour, Attack
    {
        private enum PrefabForward { Right, Up }

        [Header("Detection")]
        [SerializeField] private float detectRadius = 5f;
        [SerializeField] private LayerMask playerLayer;

        [Header("Laser (ray) settings")]
        [SerializeField] private GameObject[] segmentPrefabs; // 0:right,1:up,2:left,3:down
        [SerializeField] private int segmentCount = 2;
        [SerializeField] private float segmentSpacing = 0.5f;
        [SerializeField] private float segmentLifetime = 1.5f;
        [SerializeField] private float betweenSegmentDelay = 0.03f;
        [SerializeField] private float betweenRayDelay = 0.02f;

        [Header("Grid")]
        [SerializeField] private float gridOffset = 1f; // distance depuis le boss pour les lignes/colonnes de la grille

        [Header("Behavior")]
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private Animator animator;
        private float lastAttackTime = 0f;
        public float Radius => detectRadius;

        private void FixedUpdate()
        {
            lastAttackTime -= Time.fixedDeltaTime;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRadius, playerLayer.value);

            if (lastAttackTime <= 0f && segmentPrefabs != null && segmentPrefabs.Length > 0 && hits.Length > 0)
            {
                if (animator != null)
                    animator.SetTrigger("Attack");

                StartCoroutine(FirePattern());
                lastAttackTime = attackCooldown;
            }
        }

        // Lance les rayons sous forme de grille 3x3 sans le centre :
        // colonnes à X = ±gridOffset (vers haut et bas)
        // lignes à Y = ±gridOffset (vers gauche et droite)
        private IEnumerator FirePattern()
        {
            // directions cardinales
            Vector2[] verticalDirs = new Vector2[] { Vector2.up, Vector2.down };
            Vector2[] horizontalDirs = new Vector2[] { Vector2.right, Vector2.left };

            Vector3 leftPos = transform.position + Vector3.left * gridOffset;
            Vector3 rightPos = transform.position + Vector3.right * gridOffset;
            Vector3 upPos = transform.position + Vector3.up * gridOffset;
            Vector3 downPos = transform.position + Vector3.down * gridOffset;

            // Colonnes (à X = ±gridOffset) : lancer vers le haut et vers le bas
            foreach (var dir in verticalDirs)
            {
                StartCoroutine(FireRayRoutine(dir, leftPos));
                if (betweenRayDelay > 0f) yield return new WaitForSeconds(betweenRayDelay);

                StartCoroutine(FireRayRoutine(dir, rightPos));
                if (betweenRayDelay > 0f) yield return new WaitForSeconds(betweenRayDelay);
            }

            // Lignes (à Y = ±gridOffset) : lancer vers la droite et vers la gauche
            foreach (var dir in horizontalDirs)
            {
                StartCoroutine(FireRayRoutine(dir, upPos));
                if (betweenRayDelay > 0f) yield return new WaitForSeconds(betweenRayDelay);

                StartCoroutine(FireRayRoutine(dir, downPos));
                if (betweenRayDelay > 0f) yield return new WaitForSeconds(betweenRayDelay);
            }
        }

        // Spawn les segments à partir d'une origine donnée, choisit le prefab en fonction de la direction (pas de diagonales)
        private IEnumerator FireRayRoutine(Vector2 direction, Vector3 origin)
        {
            for (int i = 1; i <= segmentCount; i++)
            {
                Vector3 spawnPos = origin + (Vector3)(direction * (i * segmentSpacing));

                // calculer l'angle en degrés normalisé 0..360 (utile pour déterminer l'axe principal)
                float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                float angleDeg = (baseAngle + 360f) % 360f;

                // déterminer l'indice de prefab le plus proche des axes (0:right,1:up,2:left,3:down)
                int prefabIndex = Mathf.RoundToInt(angleDeg / 90f) % 4;

                // sécurité si l'array est plus petit que 4
                if (segmentPrefabs == null || segmentPrefabs.Length == 0)
                    yield break;
                if (prefabIndex < 0 || prefabIndex >= segmentPrefabs.Length)
                    prefabIndex = Mathf.Clamp(prefabIndex, 0, segmentPrefabs.Length - 1);

                GameObject prefabToSpawn = segmentPrefabs[prefabIndex];

                // instancier sans rotation (on choisit le bon sprite/orientation via le prefab)
                GameObject seg = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

                // s'assurer d'un scale positif (évite le miroir qui casse la rotation apparente)
                Vector3 s = seg.transform.localScale;
                s.x = Mathf.Abs(s.x);
                s.y = Mathf.Abs(s.y);
                seg.transform.localScale = s;

                // si le prefab a un Rigidbody2D, débloquer la rotation (on ne l'utilise pas ici)
                Rigidbody2D rb = seg.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.freezeRotation = false;
                }

                Destroy(seg, segmentLifetime);
                yield return new WaitForSeconds(betweenSegmentDelay);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectRadius);

            Gizmos.color = Color.cyan;
            float laserRange = segmentCount * segmentSpacing;

            // dessiner la grille (colonnes et lignes décalées)
            Vector3 leftPos = transform.position + Vector3.left * gridOffset;
            Vector3 rightPos = transform.position + Vector3.right * gridOffset;
            Vector3 upPos = transform.position + Vector3.up * gridOffset;
            Vector3 downPos = transform.position + Vector3.down * gridOffset;

            // colonnes (lignes verticales)
            Gizmos.DrawLine(leftPos + Vector3.up * laserRange, leftPos + Vector3.down * laserRange);
            Gizmos.DrawLine(rightPos + Vector3.up * laserRange, rightPos + Vector3.down * laserRange);

            // lignes (lignes horizontales)
            Gizmos.DrawLine(upPos + Vector3.left * laserRange, upPos + Vector3.right * laserRange);
            Gizmos.DrawLine(downPos + Vector3.left * laserRange, downPos + Vector3.right * laserRange);
        }
    }
}
