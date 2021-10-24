using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Holds logic for loading screen logic such as loading the last save file and updating UI info.
public class LoadingMenu : MonoBehaviour
{
    private AsyncOperation loadOperation;

    private void Start()
    {
        loadOperation = SceneManager.LoadSceneAsync("Combat Testing", LoadSceneMode.Additive);
        loadOperation.completed += OnSceneLoadComplete;
    }

    private void OnSceneLoadComplete(AsyncOperation currentLoadOperation)
    {
        SceneManager.UnloadSceneAsync("LoadingScreen");
    }
}
