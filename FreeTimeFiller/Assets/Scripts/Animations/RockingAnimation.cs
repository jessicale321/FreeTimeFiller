using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockingAnimation : MonoBehaviour
{
    // Time to wait until starting the idle animation
    [SerializeField] private float startDelay;

    [SerializeField] private float rockingAngle = 15f;    // Maximum angle to rock
    [SerializeField] private float rockingDuration = 1f;  // Time taken for each rock

    [SerializeField] private bool playOnAwake = true;

    private void Awake()
    {
        // Start rocking
        if(playOnAwake)
            Invoke(nameof(RockRight), startDelay);
    }

    public void PlayAnimation()
    {
        Invoke(nameof(RockRight), startDelay);
    }

    private void RockRight()
    {
        LeanTween.rotateZ(gameObject, rockingAngle, rockingDuration)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnComplete(RockLeft);
    }

    private void RockLeft()
    {
        LeanTween.rotateZ(gameObject, -rockingAngle, rockingDuration)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnComplete(RockRight);
    }
}
