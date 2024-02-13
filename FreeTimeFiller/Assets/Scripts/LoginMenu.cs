using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//-/////////////////////////////////////////////////////////////////////
///
public class LoginMenu : MonoBehaviour
{
    //-/////////////////////////////////////////////////////////////////////
    ///
    public void OnLoginButtonClicked()
    {
        SceneManager.LoadScene(1);
    }
}
