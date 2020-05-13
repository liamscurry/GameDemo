using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AbilityMenuButton : MonoBehaviour, ISelectHandler
{
    [SerializeField]
    private VideoClip abilityVideo;
    [SerializeField]
    private string abilityDescription;
    [SerializeField]
    private Color acquiredColor;
    [SerializeField]
    private VideoPlayer previewPlayer;
    [SerializeField]
    private Text previewText;
    [SerializeField]
    private Text abilityStatus;
    [SerializeField]
    private int abilityCost;
    [SerializeField]
    private AbilityMenuButton prerequisite;
    [SerializeField]
    private bool acquiredInitially;

    private const float dimPercentage = 0.5f;

    public bool Acquired { get; set; }

    private void Awake()
    {
        if (acquiredInitially)
        {
            TryAcquire();
            
        }
        Acquired = acquiredInitially;
    }

    public virtual void OnSelect(BaseEventData eventData)
    {
        previewPlayer.clip = abilityVideo;
        previewPlayer.Stop();
        previewPlayer.Play();
        previewText.text = abilityDescription;
        UpdateAbilityStatus();
    }

    public void TryAcquire()
    {
        if (!Acquired &&
            (prerequisite == null || prerequisite.Acquired) &&
            PlayerInfo.StatsManager.UpgradePoints >= abilityCost)
        {
            Button button = GetComponent<Button>();
            ColorBlock colorBlock = button.colors;
            
            Color dimmedAcquiredColor = 
                new Color(acquiredColor.r * dimPercentage,
                          acquiredColor.g * dimPercentage,
                          acquiredColor.b * dimPercentage,
                          1);

            colorBlock.normalColor = dimmedAcquiredColor;
            colorBlock.pressedColor = acquiredColor;
            colorBlock.selectedColor = acquiredColor;
                
            button.colors = colorBlock;
            Acquired = true;

            PlayerInfo.StatsManager.UpgradePoints -= abilityCost;

            UpdateAbilityStatus();
        }
    }

    protected virtual void UpdateAbilityStatus()
    {
        if (Acquired)
        {
            abilityStatus.text = "Unlocked";
        }
        else
        {
            abilityStatus.text =
                PlayerInfo.StatsManager.UpgradePoints + " / " + abilityCost;
        }
    }
}
