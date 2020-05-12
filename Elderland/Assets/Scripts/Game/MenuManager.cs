using UnityEngine;

//Handles menu input and deals with its effects, such as pausing the game.

public class MenuManager : MonoBehaviour 
{
    //Change state of menus only after the majority of current frame has been calculated.
    private void LateUpdate()
    {
        UpdateMenuInput();
    }

    //Calls various menu input methods
    private void UpdateMenuInput()
    {
        GameMenu();
    }

    //Menu Toggles
    private void GameMenu()
    {
        Pause();
    }

    //Menu Functionality
    private void Pause()
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
}
