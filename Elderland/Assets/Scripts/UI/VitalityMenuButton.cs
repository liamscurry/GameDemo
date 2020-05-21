using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class VitalityMenuButton : MonoBehaviour, ISelectHandler
{
    [SerializeField]
    private VideoClip vitalityVideo;
    [SerializeField]
    private string vitalityDescription;
    [SerializeField]
    private Color maxTierColor;
    [SerializeField]
    private VideoPlayer previewPlayer;
    [SerializeField]
    private Text previewText;
    [SerializeField]
    private Text vitalityAvailableText;
    [SerializeField]
    private Text abilityCostText;
    [SerializeField]
    private GameObject abilityCostIcon;
    [SerializeField]
    private Text vitalityCostText;
    [SerializeField]
    private GameObject vitalityCostIcon;
    [SerializeField]
    private int vitalityCost;
    [SerializeField]
    private int maxTier;
    [SerializeField]
    private Image[] tierIndicators;
    [SerializeField]
    private UnityEvent onUpgrade;

    private const float dimPercentage = 0.5f;

    private int tier;

    private void Start()
    {
        tier = 0;

        vitalityAvailableText.text =
            "Available: " + PlayerInfo.StatsManager.VitalityPoints;

        vitalityCostText.text = "";
        vitalityCostIcon.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        vitalityAvailableText.text =
            "Available: " + PlayerInfo.StatsManager.VitalityPoints;

        vitalityCostText.text = "";
        vitalityCostIcon.gameObject.SetActive(false);
    }

    public virtual void OnSelect(BaseEventData eventData)
    {
        previewPlayer.clip = vitalityVideo;
        previewPlayer.Stop();
        previewPlayer.Play();
        previewText.text = vitalityDescription;
        UpdateVitalityStatus();
        abilityCostIcon.SetActive(false);
        abilityCostText.text = "";
    }

    public void TryAcquireIteration()
    {
        //Debug.Log("called");
        if (tier < maxTier &&
            PlayerInfo.StatsManager.VitalityPoints >= vitalityCost)
        {
            Button button = GetComponent<Button>();

            if (tier == maxTier - 1)
            {
                ColorBlock colorBlock = button.colors;
                
                Color dimmedMaxTierColor = 
                    new Color(maxTierColor.r * dimPercentage,
                            maxTierColor.g * dimPercentage,
                            maxTierColor.b * dimPercentage,
                            1);

                colorBlock.normalColor = dimmedMaxTierColor;
                colorBlock.pressedColor = maxTierColor;
                colorBlock.selectedColor = maxTierColor;
                    
                button.colors = colorBlock;
            }

            tierIndicators[tier].color = maxTierColor;

            PlayerInfo.StatsManager.VitalityPoints -= vitalityCost;
            tier++;

            if (onUpgrade != null)
                onUpgrade.Invoke();

            UpdateVitalityStatus();
        }
    }

    protected virtual void UpdateVitalityStatus()
    {
        vitalityAvailableText.text =
            "Available: " + PlayerInfo.StatsManager.VitalityPoints;

        if (tier < maxTier)
        {
            vitalityCostText.text =
                "Cost: " + vitalityCost;
            vitalityCostIcon.gameObject.SetActive(true);
        }
        else
        {
            vitalityCostText.text = "";
            vitalityCostIcon.gameObject.SetActive(false);
        }
    }
}
