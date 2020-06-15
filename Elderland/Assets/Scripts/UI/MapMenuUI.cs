using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class MapMenuUI : StartMenuUI
{
    [SerializeField]
    private GameObject teleporterParent;

    private Button[] teleporterButtons;

    private bool tabSwapEnabled;

    private void Awake()
    {
        teleporterButtons =
            teleporterParent.GetComponentsInChildren<Button>();
        tabSwapEnabled = true;
    }

    public void DisableTeleporters()
    {
        foreach (Button button in teleporterButtons)
        {
            button.interactable = false;
        }
        tabSwapEnabled = true;
    }

    public void EnableTeleporters()
    {
        foreach (Button button in teleporterButtons)
        {
            button.interactable = true;
        }

        tabSwapEnabled = false;
    }

    public override void DisableMenu()
    {
        transform.parent.gameObject.SetActive(false);

        GameInfo.Paused = false;
        Time.timeScale = 1;
        GameInfo.Manager.OverlayUnfreezeInput();

        // Deselect active button to have highlight by default when turning on
        eventSystem.SetSelectedGameObject(null);

        gameplayUI.SetActive(true);

        DisableTeleporters();
    }

    protected override void TabSwap()
    {
        if (tabSwapEnabled)
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
    }
}
