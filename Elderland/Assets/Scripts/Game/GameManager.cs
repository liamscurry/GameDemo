using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;

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

    private bool respawning;

    private float combatCheckTimer;
    private const float combatCheckDuration = 1f;
    private const float combatCheckRadius = 30f;

    // GameplayOverride Input
    // Can be overriden, yet do not override:
    // Player abilities
    // Sword take out and put away.

    // GameplayUnoverride Input
    // Cannot be overriden (except by respawn), yet do not override:
    // Player falling

    // None Input
    // Cannot be overriden (even by respawn), may override:
    // PS: respawn must implement short circuits for GameplayUnoverride input manually for each occurance
    // if player can take damage in that state.
    // Interactions
    // Cutscenes
    public StatLock<GameInput> ReceivingInput { get; private set; }
    public bool Respawning { get { return respawning; } }
    public bool InCombat { get; private set; }

    public event EventHandler OnRespawn;
    public event EventHandler OnLateRespawn;

    //Set up all components of the game.
	private void Awake() 
    {
        GetComponent<GameInitializer>().Initialize();
        ReceivingInput = new StatLock<GameInput>();
        Application.targetFrameRate = 300;
        respawning = false;
	}

    private void Update()
    {
        // Fix UI focus loss when clicking with mouse. There should be a better solution built in,
        // but this seems like the best solution at the moment annoyingly, as I have read from the forum.
        // Credit idea of logic goes to them.
        if (eventSystem.currentSelectedGameObject == null)
        {
            eventSystem.SetSelectedGameObject(eventSystem.firstSelectedGameObject);
        }

        CheckForCombat();

        if (Input.GetKeyDown(KeyCode.W))
        {
            Time.timeScale = 0.1f;
            PlayerInfo.Manager.MaxOutHealth();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Time.timeScale = 1;
            PlayerInfo.Manager.MaxOutHealth();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            PlayerInfo.Manager.ChangeHealth(-1);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            GameInfo.Settings.ChangeKeybindTest();
        }
    }

    /*
    Stream-lined method for use by None input actions. Overrides the input if overridable currently and
    also procs when simply input set to full currently.
    */
    public bool IsInputFreeOrOverridable()
    {
        return ReceivingInput.Value == GameInput.Full ||
               ReceivingInput.Value == GameInput.GameplayOverride;
    }

    public void CheckForCombat()
    {
        if (respawning)
        {
            InCombat = false;
            combatCheckTimer = 0;
            return;
        }

        combatCheckTimer += Time.deltaTime;
        if (combatCheckTimer > combatCheckDuration)
        {
            combatCheckTimer = 0;
            InCombat = false;

            // Will parse to only include enemies that are attacking player.
            Collider[] nearbyEnemies = 
                Physics.OverlapSphere(
                    PlayerInfo.Player.transform.position,
                    combatCheckRadius,
                    LayerConstants.Enemy);

            foreach (Collider collider in nearbyEnemies)
            {
                var enemyManager = collider.GetComponent<EnemyManager>();
                if (enemyManager.AttackingPlayer &&
                    enemyManager.Alive)
                {
                    InCombat = true;
                    break;
                }
            }
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
        yield return new WaitUntil(() => GameInfo.Settings.CurrentGamepad[GameInfo.Settings.UseKey].isPressed);

        UnfreezeGame();
    }

    public void FadeTeleport()
    {
        StartCoroutine(FadeTeleportCoroutine(1.5f, 1f));
    }

    private IEnumerator FadeTeleportCoroutine(float duration, float waitTime)
    {
        ReceivingInput.ClaimTempLock(GameInput.None);

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

        ReceivingInput.ReleaseTempLock();
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
        // Lock released in RespawnBehaviour.cs
        GameInfo.Manager.ReceivingInput.ClaimLock(this, GameInput.None);

        Time.timeScale = 0;
        respawning = true;

        yield return new WaitForSecondsRealtime(2);

        //Fade in
        yield return Fade(duration, 1);

        if (GameInfo.CurrentLevel != null)
            GameInfo.CurrentLevel.Reset();
        GameInfo.ProjectilePool.ClearProjectilePool();
        GameInfo.PickupPool.ClearPickupPool();
        
        if (OnRespawn != null)
            OnRespawn.Invoke(this, EventArgs.Empty); 

        GameInfo.SaveManager.LoadAutoSave();

        if (OnLateRespawn != null)
            OnLateRespawn.Invoke(this, EventArgs.Empty);

        //Fade out
        yield return Fade(duration / 2, 0);

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