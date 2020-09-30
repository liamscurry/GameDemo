using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Pause menu controller.
public class PauseMenuUI : MonoBehaviour
{
    [SerializeField]
    protected GameObject startUIObject;
    [SerializeField]
    protected EventSystem eventSystem;

    protected virtual void OnEnable()
    {
        StartCoroutine(DefaultSelectedCoroutine());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Joystick1Button6))
        {
            DisableMenu();
            return;
        }
    }

    public void SetStartUIObject(GameObject UIObject)
    {
        startUIObject = UIObject;
    }

    public virtual void DisableMenu()
    {
        transform.gameObject.SetActive(false);

        GameInfo.Paused = false;
        Time.timeScale = 1;
        GameInfo.Manager.OverlayUnfreezeInput();

        // Deselect active button to have highlight by default when turning on
        eventSystem.SetSelectedGameObject(null);

        //gameplayUI.SetActive(true);
    }

    // Set active button to have highlight by default on enable.
    // Was not highlighted when switching to tab before.
    protected IEnumerator DefaultSelectedCoroutine()
    {
        yield return new WaitForEndOfFrame();
        
        eventSystem.SetSelectedGameObject(startUIObject);
    }
}
