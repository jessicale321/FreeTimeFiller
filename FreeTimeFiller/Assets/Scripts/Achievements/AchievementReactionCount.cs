using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;

public class AchievementReactionCount : MonoBehaviour
{
    [SerializeField] private TMP_Text reactionText;

    // Start is called before the first frame update
    private async void OnEnable()
    {
        // Load how many reactions this user has received from other users and display it on screen
        int reactionCount = await UserDatabase.Instance.GetDataFromUserId<int>(AuthenticationService.Instance.PlayerId,
            "number_of_achievement_reactions");

        reactionText.text = $": {reactionCount}";
    }
}
