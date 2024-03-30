using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckmarkAnimation : MonoBehaviour
{
    [SerializeField] private float scaleFactor = 2f; // Scale factor for how big the object should get
    [SerializeField, Range(0.05f, .5f)] private float duration = 0.2f; // Duration of each part of the animation

    private Vector3 _originalScale;

    private void Awake()
    {
        // Save the original size of the checkmark
        _originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        // Scale up quickly
        LeanTween.scale(gameObject, transform.localScale * scaleFactor, duration)
                 .setEase(LeanTweenType.easeOutQuad) // Apply an ease-out effect for smooth scaling
                 .setOnComplete(() =>
                 {
                     // Scale back down quickly after scaling up
                     LeanTween.scale(gameObject, _originalScale, duration)
                              .setEase(LeanTweenType.easeInQuad); // Apply an ease-in effect for smooth scaling back
                 });
    }
}
