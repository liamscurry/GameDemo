using UnityEngine;

//Handles menu input and deals with its effects, such as pausing the game.

public class MenuManager : MonoBehaviour 
{
    [SerializeField]
    private GameObject startMenuUI;
    [SerializeField]
    private GameObject gameplayUI;
    [SerializeField]
    private ObjectiveUIManager objectiveUIManager;

    private bool pausedLastFrame;

    private void Start()
    {
        pausedLastFrame = false;
    }

    //Change state of menus only after the majority of current frame has been calculated.
    private void LateUpdate()
    {
        UpdateMenuInput();
    }

    //Calls menu toggles
    private void UpdateMenuInput()
    {
        if (!pausedLastFrame)
        {
            PauseMenu();
            StartMenu();
            ObjectiveMenu();
        }

        pausedLastFrame = GameInfo.Paused;
    }

    //Menu Functionality
    private void PauseMenu()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {      
            //If not paused
            if(!GameInfo.Paused)
            {
                GameInfo.Paused = true;
                Time.timeScale = 0;
                GameInfo.Manager.OverlayFreezeInput();
            }
            //If paused
            else
            {
                GameInfo.Paused = false;
                Time.timeScale = 1;
                GameInfo.Manager.OverlayUnfreezeInput();
            }
        }
    }

    private void StartMenu()
    {
        if (Input.GetKeyDown(KeyCode.Joystick1Button7))
        {
            OpenStartMenu();
        }
    }

    public void OpenStartMenu()
    {
        startMenuUI.SetActive(true);
        gameplayUI.SetActive(false);

        GameInfo.Paused = true;
        Time.timeScale = 0;
        GameInfo.Manager.OverlayFreezeInput();
    }

    private void ObjectiveMenu()
    {
        if (Input.GetKeyDown(KeyCode.Joystick1Button9))
        {
            objectiveUIManager.PingObjectives();
        }
    }
}
