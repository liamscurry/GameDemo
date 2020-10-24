using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class AbilityMenuButton : MonoBehaviour, ISelectHandler
{
    [SerializeField]
    private VideoClip abilityVideo;
    [SerializeField]
    private string abilityDescription;
    [SerializeField]
    private Color acquiredColor;
    [SerializeField]
    private Color lockedColor;
    [SerializeField]
    private VideoPlayer previewPlayer;
    [SerializeField]
    private Text previewText;
    [SerializeField]
    private Text abilityAvailableText;
    [SerializeField]
    private Text vitalityCostText;
    [SerializeField]
    private GameObject vitalityCostIcon;
    [SerializeField]
    private Text abilityCostText;
    [SerializeField]
    private GameObject abilityCostIcon;
    [SerializeField]
    private int abilityCost;
    [SerializeField]
    private AbilityMenuButton prerequisite;
    [SerializeField]
    private AbilityMenuButton preceding;
    [SerializeField]
    private bool acquiredInitially;
    [SerializeField]
    private bool unlocked;
    [SerializeField]
    private UnityEvent onAcquire;

    private const float dimPercentage = 1f;

    public bool Acquired { get; set; }

    private ColorBlock unlockedColorBlock;
    private Button button;
    private bool initialized;

    private void Awake()
    {
        TryInitialize();
    }

    private void TryInitialize()
    {
        if (!initialized)
        {
            button = GetComponent<Button>();
            unlockedColorBlock = button.colors;
            initialized = true;

            if (!unlocked)
            {
                acquiredInitially = false;
                SetToLockedColor();
            }

            if (acquiredInitially)
            {
                TryAcquire();
            }
            Acquired = acquiredInitially;
        }
    }

    public virtual void OnSelect(BaseEventData eventData)
    {
        previewPlayer.clip = abilityVideo;
        previewPlayer.Stop();
        previewPlayer.Play();
        previewText.text = abilityDescription;
        UpdateAbilityStatus();
        vitalityCostIcon.SetActive(false);
        vitalityCostText.text = "";
    }

    public void Unlock(bool setColors = true)
    {
        if (setColors)
            SetToUnlockedColor();
        unlocked = true;
    }

    public void TryAcquire()
    {
        if (!Acquired &&
            (prerequisite == null || prerequisite.Acquired) &&
            PlayerInfo.StatsManager.UpgradePoints >= abilityCost &&
            unlocked)
        {
            SetToAcquiredColor();

            Acquired = true;

            PlayerInfo.StatsManager.UpgradePoints -= abilityCost;

            if (onAcquire != null)
                onAcquire.Invoke();

            UpdateAbilityStatus();

            if (preceding != null)
                preceding.Unlock();
        }
    }

    private void SetToLockedColor()
    {
        TryInitialize();

        ColorBlock colorBlock = unlockedColorBlock;
            
        Color dimmedLockedColor = 
            new Color(lockedColor.r * dimPercentage,
                      lockedColor.g * dimPercentage,
                      lockedColor.b * dimPercentage,
                      lockedColor.a);

        colorBlock.normalColor = dimmedLockedColor;
        colorBlock.pressedColor = lockedColor;
        colorBlock.selectedColor = lockedColor;
        button.colors = colorBlock;
    }

    private void SetToUnlockedColor()
    {
        TryInitialize();
        button.colors = unlockedColorBlock;
    }

    private void SetToAcquiredColor()
    {
        TryInitialize();

        ColorBlock colorBlock = unlockedColorBlock;
            
        Color dimmedAcquiredColor = 
            new Color(acquiredColor.r * dimPercentage,
                      acquiredColor.g * dimPercentage,
                      acquiredColor.b * dimPercentage,
                      0);

        colorBlock.normalColor = dimmedAcquiredColor;
        colorBlock.pressedColor = acquiredColor;
        colorBlock.selectedColor = acquiredColor;

        button.colors = colorBlock;
    }

    protected virtual void UpdateAbilityStatus()
    {
        abilityAvailableText.text =
            "Available:      " + PlayerInfo.StatsManager.UpgradePoints;

        if (Acquired)
        {
            abilityCostText.text = "Acquired";
            abilityCostIcon.gameObject.SetActive(false);
        }
        else if (!unlocked)
        {
            abilityCostText.text = "Not unlocked";
            abilityCostIcon.gameObject.SetActive(false);
        }
        else
        {
            abilityCostText.text =
                "Cost:      " + abilityCost;
            abilityCostIcon.gameObject.SetActive(true);
        }
    }
}
