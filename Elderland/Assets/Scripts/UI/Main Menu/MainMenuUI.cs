using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Holds logic for main menu buttons and UI details.
public class MainMenuUI : MonoBehaviour
{
    public void ContinueButtonLogic()
    {
        SceneManager.LoadScene("LoadingScreen", LoadSceneMode.Single);
    }

    // Tested in build, passed 10/2/21.
    public void QuitButtonLogic()
    {
        Application.Quit();
    }
}
