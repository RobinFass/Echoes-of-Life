using System.Collections;
using UnityEngine;

public class RandomAnimationOffset : MonoBehaviour
{
    [Tooltip("Délai max avant de lancer l'animation (s)")]
    public float maxStartDelay = 0.2f;

    [Tooltip("Randomiser le temps normalisé de départ (0..1)")]
    public bool randomizeNormalizedTime = true;

    [Tooltip("Randomiser légèrement la vitesse de l'animator")]
    public bool randomizeSpeed = false;
    public Vector2 speedRange = new Vector2(0.95f, 1.05f);

    [Tooltip("Nom de l'état à forcer (laisser vide pour l'état courant)")]
    public string stateName = "";
    [Tooltip("Layer d'animation à cibler")]
    public int layer = 0;

    private Animator animator;

    private IEnumerator Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null) yield break;

        // petit délai aléatoire pour éviter démarrage synchro
        float delay = Random.Range(0f, maxStartDelay);
        if (delay > 0f) yield return new WaitForSeconds(delay);

        if (randomizeSpeed)
            animator.speed = Random.Range(speedRange.x, speedRange.y);

        if (randomizeNormalizedTime)
        {
            int stateHash;
            if (!string.IsNullOrEmpty(stateName))
                stateHash = Animator.StringToHash(stateName);
            else
            {
                var info = animator.GetCurrentAnimatorStateInfo(layer);
                stateHash = info.fullPathHash != 0 ? info.fullPathHash : info.shortNameHash;
            }

            // positionne l'animation sur un temps normalisé aléatoire et applique immédiatement
            animator.Play(stateHash, layer, Random.value);
            animator.Update(0f);
        }
    }
}