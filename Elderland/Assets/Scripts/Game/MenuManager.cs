using UnityEngine;

//Handles menu input and deals with its effects, such as pausing the game.

public class MenuManager : MonoBehaviour 
{
    [SerializeField]
    private GameObject startMenuUI;
    [SerializeField]
    private GameObject pauseMenuUI;
    [SerializeField]
    private GameObject gameplayUI;
    [SerializeField]
    private GameObject fightingUI;
    [SerializeField]
    private GameObject puzzleUI;
    [SerializeField]
    private ObjectiveUIManager objectiveUIManager;

    private bool pausedLastFrame;

    public ObjectiveUIManager ObjectiveManager { get { return objectiveUIManager; } }
    public GameObject PuzzleUI { get { return puzzleUI; } }
    public GameObject GameplayUI { get { return gameplayUI; } }
    public GameObject FightingUI { get { return fightingUI; } }

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
        if (!pausedLastFrame &&
            GameInfo.CameraController.CameraState == CameraController.State.Gameplay &&
            !GameInfo.Manager.Respawning)
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
        /*
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
        }*/

        if (Input.GetKeyDown(KeyCode.Joystick1Button6))
        {
            OpenPauseMenu();
        }
    }

    private void StartMenu()
    {
        if (Input.GetKeyDown(KeyCode.Joystick1Button7) &&
            GameInfo.CameraController.CameraState != CameraController.State.Cutscene && 
            GameInfo.CameraController.CameraState != CameraController.State.GameplayCutscene)
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
        GameInfo.Manager.ReceivingInput.ClaimTempLock(GameInput.None);
    }

    public void OpenPauseMenu()
    {
        pauseMenuUI.SetActive(true);
        //gameplayUI.SetActive(false);

        GameInfo.Paused = true;
        Time.timeScale = 0;
        GameInfo.Manager.ReceivingInput.ClaimTempLock(GameInput.None);
    }

    private void ObjectiveMenu()
    {
        if (GameInfo.Settings.ObjectiveTrigger == -1)
        {
            objectiveUIManager.PingObjectives();
        }
    }
}
