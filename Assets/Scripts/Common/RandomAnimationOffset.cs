using System.Collections;
using UnityEngine;

public class RandomAnimationOffset : MonoBehaviour
{
    [Tooltip("Délai max avant de lancer l'animation (s)")]
    [SerializeField] private float maxStartDelay = 0.75f;
    
    [Tooltip("Randomiser légèrement la vitesse de l'animator")]
    [SerializeField] private bool randomizeSpeed = true;
    [SerializeField] private Vector2 speedRange = new Vector2(0.95f, 1.05f);

    private Animator animator;

    private IEnumerator Start()
    {
        animator = GetComponent<Animator>();
        if (!animator) yield break;

        float delay = Random.Range(0f, maxStartDelay);
        if (delay > 0f) yield return new WaitForSeconds(delay);

        if (randomizeSpeed)
            animator.speed = Random.Range(speedRange.x, speedRange.y);
    }
}