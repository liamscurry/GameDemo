using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//Deals with initialization, loading and quiting of the game. Manager of all classes.

public class GameManager : MonoBehaviour 
{
    [SerializeField]
    private Image respawnImage;
    [SerializeField]
    private GameObject gameplayUI;
    [SerializeField]
    private EventSystem eventSystem;

    private IEnumerator slowEnumerator;
    private float slowFreezeTimer;

    private static bool receivingInput;
    private static bool previousReceivingInput;
    private static object receivingInputSetter;

    public bool ReceivingInput { get { return receivingInput; } }

    public event EventHandler OnRespawn;

    //Set up all components of the game.
	private void Awake() 
    {
        GetComponent<GameInitializer>().Initialize();
        receivingInput = false;
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

        if (Input.GetKeyDown(KeyCode.E))
        {
            GameInfo.PickupPool.Create<HealthPickup>(
                Resources.Load<GameObject>(ResourceConstants.Pickups.HealthPickup),
                new Vector3(0.26f, 1.24f, 7.59f));
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            GameInfo.PickupPool.ClearPickupPool();
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
        previousReceivingInput = receivingInput;
        receivingInput = false;
    }

    public void OverlayUnfreezeInput()
    {
        receivingInput = previousReceivingInput;
    }

    public void WaitForUseToUnfreeze()
    {
        StartCoroutine("WaitForUseToUnfreezeCoroutine");
    }

    private IEnumerator SlowFreezeCoroutine(float duration)
    {
        while (true)
        {
            slowFreezeTimer += Time.deltaTime;
            Time.timeScale = Mathf.SmoothStep(1, 0, slowFreezeTimer / duration);
            //Time.fixedDeltaTime = (Time.timeScale != 0) ?  (1 / 50f) * Time.timeScale : (1 / 50f);
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

    private IEnumerator FadeRespawn(float duration)
    {
        Time.timeScale = 0;

        yield return new WaitForSecondsRealtime(2);

        //Fade in
        yield return Fade(duration, 1);

        if (GameInfo.CurrentLevel != null)
            GameInfo.CurrentLevel.Reset();
        PlayerInfo.Manager.Reset();
        if (OnRespawn != null)
            OnRespawn.Invoke(this, EventArgs.Empty);

        //Fade out
        yield return Fade(duration / 2, 0);

        PlayerInfo.Manager.Respawn();

        Time.timeScale = 1;
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
}