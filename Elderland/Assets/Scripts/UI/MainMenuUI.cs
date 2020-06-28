using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MainMenuUI : MonoBehaviour 
{
	[SerializeField]
	private CameraCutsceneEvent cutscene;
	[SerializeField]
	private GameObject mainMenuCanvas;
	[SerializeField]
	private UnityEvent onStartButton;

	public void OnStartButton()
	{
		cutscene.Invoke();
		//mainMenuCanvas.SetActive(false);
		if (onStartButton != null)
			onStartButton.Invoke();
		gameObject.SetActive(false);
	}

	public void OnQuitButton()
	{
		Application.Quit();
	}
}
