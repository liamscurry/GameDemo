﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

//Deals with initialization, loading and quiting of the game. Manager of all classes.

public class GameManager : MonoBehaviour 
{
    [SerializeField]
    private Image respawnImage;
    [SerializeField]
    private GameObject gameplayUI;
    [SerializeField]
    private EventSystem eventSystem;
    [SerializeField]
    private UnityEvent onFadeOutroIn;
    [SerializeField]
    private UnityEvent onFadeOutroOut;
    //[SerializeField]
    //private UnityEvent onUseUnfreeze;

    private IEnumerator slowEnumerator;
    private float slowFreezeTimer;

    private static bool receivingInput;
    private static bool previousReceivingInput;
    private static object receivingInputSetter;

    private bool respawning;

    public bool ReceivingInput { get { return receivingInput; } }
    public bool Respawning { get { return respawning; } }

    public event EventHandler OnRespawn;

    //Set up all components of the game.
	private void Awake() 
    {
        GetComponent<GameInitializer>().Initialize();
        receivingInput = false;
        Application.targetFrameRate = 300;
        respawning = false;
	}

    private void Update()
    {
        //Fix UI focus loss when clicking with mouse. There should be a better solution built in,
        //but this seems like the best solution at the moment annoyingly, as I have read from the forum.
        //Credit idea of logic goes to them.
        if (eventSystem.currentSelectedGameObject == null)
        {
            eventSystem.SetSelectedGameObject(eventSystem.firstSelectedGameObject);
        }
    }

    private void OnApplicationFocus()
    {
        Cursor.visible = false;
    }

    public void Load()
    {

    }

    public void Save()
    {
        
    }

    public void Quit()
	{
		Application.Quit();
	}

    public void SlowFreezeGame(float duration)
    {   
        slowFreezeTimer = 0;
        slowEnumerator = SlowFreezeCoroutine(duration);
        StartCoroutine(slowEnumerator);
    }

    public void FreezeGame()
    {
        Time.timeScale = 0;
    }

    public void UnfreezeGame()
    {
        StopCoroutine(slowEnumerator);
        Time.timeScale = 1;
        Time.fixedDeltaTime = 1 / 50f;
    }

    public void FreezeInput(object setter)
    {
        receivingInputSetter = setter;
        receivingInput = false;
    }

    public void UnfreezeInput(object setter)
    {
        if (receivingInputSetter == setter)
        {
            receivingInputSetter = null;
            receivingInput = true;
        }
    }

    public void OverlayFreezeInput()
    {
        FreezeInput(this);
        //previousReceivingInput = receivingInput;
        //receivingInput = false;
    }

    public void OverlayUnfreezeInput()
    {   
        UnfreezeInput(this);
        //receivingInput = previousReceivingInput;
    }

    public void WaitForUseToUnfreeze()
    {
        StartCoroutine("WaitForUseToUnfreezeCoroutine");
    }

    private IEnumerator SlowFreezeCoroutine(float duration)
    {
        while (true)
        {
            slowFreezeTimer += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.SmoothStep(1, 0, slowFreezeTimer / duration);
            Time.fixedDeltaTime = (Time.timeScale != 0) ?  (1 / 50f) * Time.timeScale : (1 / 50f);
            if (Time.timeScale != 0)
            {
                yield return new WaitForEndOfFrame();
            }
            else
            {
                break;
            }
        }
    }

    private IEnumerator WaitForUseToUnfreezeCoroutine()
    {
        yield return new WaitUntil(() => Input.GetKeyDown(GameInfo.Settings.UseKey));

        UnfreezeGame();
    }

    public void FadeTeleport()
    {
        StartCoroutine(FadeTeleportCoroutine(1.5f, 1f));
    }

    private IEnumerator FadeTeleportCoroutine(float duration, float waitTime)
    {
        OverlayFreezeInput();

        //yield return new WaitForSecondsRealtime(2.25f);

        //if (onFadeOutroIn != null)
        //    onFadeOutroIn.Invoke();

        //Fade in
        yield return Fade(0.5f, 1);

        yield return new WaitForSecondsRealtime(waitTime);

        //if (onFadeOutroOut != null)
        //    onFadeOutroOut.Invoke();

        //Fade out
        yield return Fade(duration, 0);

        OverlayUnfreezeInput();
    }

    public void FadeOutro()
    {
        StartCoroutine(FadeOutroCoroutine(3.5f, 10f));
    }

    private IEnumerator FadeOutroCoroutine(float duration, float waitTime)
    {
        yield return new WaitForSecondsRealtime(2.25f);

        if (onFadeOutroIn != null)
            onFadeOutroIn.Invoke();

        //Fade in
        yield return Fade(duration, 1);

        yield return new WaitForSecondsRealtime(waitTime);

        if (onFadeOutroOut != null)
            onFadeOutroOut.Invoke();

        //Fade out
        yield return Fade(duration, 0);
    }

    private IEnumerator FadeRespawn(float duration)
    {
        Time.timeScale = 0;
        respawning = true;

        yield return new WaitForSecondsRealtime(2);

        //Fade in
        yield return Fade(duration, 1);

        if (GameInfo.CurrentLevel != null)
        {
            GameInfo.CurrentLevel.Reset();
            PlayerInfo.Player.transform.position =
                GameInfo.CurrentLevel.RespawnTransform.position;
            PlayerInfo.Player.transform.rotation = 
                GameInfo.CurrentLevel.RespawnTransform.rotation;
            GameInfo.CameraController.SetDirection(
                GameInfo.CurrentLevel.RespawnTransform.rotation);

            PlayerInfo.Manager.RestoreHealth();
            PlayerInfo.Manager.RestoreStamina();
        }
        else
        {
            PlayerInfo.Player.transform.position =
                GameInfo.RespawnTransformNoLevel.position;
            PlayerInfo.Player.transform.rotation = 
                GameInfo.RespawnTransformNoLevel.rotation;
            GameInfo.CameraController.SetDirection(
                GameInfo.RespawnTransformNoLevel.rotation);

            PlayerInfo.Manager.RestoreHealth();
            PlayerInfo.Manager.RestoreStamina();
        }
        PlayerInfo.Manager.Reset();
        if (OnRespawn != null)
            OnRespawn.Invoke(this, EventArgs.Empty);

        //Fade out
        yield return Fade(duration / 2, 0);

        PlayerInfo.Manager.Respawn();

        Time.timeScale = 1;
        respawning = false;
    }

    private IEnumerator Fade(float duration, float value)
    {
        float timer = 0;

        while (true)
        {
            timer += Time.unscaledDeltaTime;

            Color color = respawnImage.color;
            color.a = Mathf.SmoothStep(color.a, value, timer / (duration / 2));
            respawnImage.color = color;

            if (timer >= duration / 2)
            {
                break;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    //Called when player dies.
    public void Respawn()
    {
        StartCoroutine(FadeRespawn(4));
    }

    public void SetRespawnTransformNoLevel(Transform transform)
    {
        GameInfo.RespawnTransformNoLevel = transform;
    }
}