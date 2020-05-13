using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Ability menu controller. Used to disable/switch UI.
public class StartMenuUI : MonoBehaviour
{
    [SerializeField]
    protected GameObject startUIObject;
    [SerializeField]
    protected EventSystem eventSystem;
    [SerializeField]
    protected GameObject leftNeighborMenu;
    [SerializeField]
    protected GameObject rightNeighborMenu;

    protected void OnEnable()
    {
        StartCoroutine(DefaultSelectedCoroutine());
    }

     private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Joystick1Button7))
        {
            DisableMenu();
            return;
        }

        TabSwap();
    }
    protected void TabSwap()
    {
        if (Input.GetKeyDown(KeyCode.Joystick1Button4) &&
            leftNeighborMenu != null)
        {
            transform.gameObject.SetActive(false);
            leftNeighborMenu.gameObject.SetActive(true);
        }
        else if (Input.GetKeyDown(KeyCode.Joystick1Button5) &&
                 rightNeighborMenu != null)
        {
            transform.gameObject.SetActive(false);
            rightNeighborMenu.gameObject.SetActive(true);
        }
    }

    protected void DisableMenu()
    {
        transform.parent.gameObject.SetActive(false);

        GameInfo.Paused = false;
        Time.timeScale = 1;
        //GameInfo.Manager.OverlayUnfreezeInput();

        // Deselect active button to have highlight by default when turning on
        eventSystem.SetSelectedGameObject(null);
    }

    // Set active button to have highlight by default on enable.
    // Was not highlighted when switching to tab before.
    protected IEnumerator DefaultSelectedCoroutine()
    {
        yield return new WaitForEndOfFrame();
        
        eventSystem.SetSelectedGameObject(startUIObject);
    }
}
