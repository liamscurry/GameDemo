using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour 
{
	[SerializeField]
	private CameraCutsceneEvent cutscene;
	[SerializeField]
	private GameObject mainMenuCanvas;

	public void OnStartButton()
	{
		cutscene.Invoke();
		mainMenuCanvas.SetActive(false);
		gameObject.SetActive(false);
	}

	public void OnQuitButton()
	{
		Application.Quit();
	}
}
