using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* To use the LeanTween library, go to the Unity Asset Store and download the free "LeanTween" asset and import it your project. https://assetstore.unity.com/packages/tools/animation/leantween-3595 */
public class ErrorAnimation : MonoBehaviour
{
    [Header("Animation Values")]
    [SerializeField] private Vector3 errorShakeDirection;
    [SerializeField, Range(0.05f, 0.5f)] private float errorShakeDuration;
    
    private bool _performingErrorAnimation;

    ///-///////////////////////////////////////////////////////////
    /// Animation of the gameObject shaking left and right when something goes wrong.
    /// 
    public void PlayAnimation(GameObject gameObjectToShake)
    {
        if (_performingErrorAnimation) return;
        
        // Position that the task should go back after performing an error animation
        Vector3 originalPosition = gameObjectToShake.transform.position;
            
        _performingErrorAnimation = true;
            
        // Move the task button around to showcase that an error occurred
        LeanTween.move(gameObjectToShake, originalPosition + errorShakeDirection, errorShakeDuration)
            .setEase(LeanTweenType.easeInOutQuad)
            .setLoopPingPong(3)
            .setOnComplete(() => {
                // Return to original position
                Vector3 returnPosition = originalPosition;
                LeanTween.move(gameObjectToShake, returnPosition, 0f);
                _performingErrorAnimation = false;
            });
    }
}
