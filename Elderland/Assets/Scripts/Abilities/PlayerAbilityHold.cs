using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Structure helper used in player abilities to monitor hold ability segments.
// A full hold is used when the ability short circuits when there is an early release.
// A truncated hold is when the ability would rather do another action when released, in which the player
// can release as earlier as they like.
public class PlayerAbilityHold
{   
    // Fields
    private float holdTimer;
    private float minimumDuration;
    private float longDuration;
    private bool letGo;

    private GameObject holdBar;
    private GameObject holdBarFill;
    private float holdBarScaleXMax;

    private AbilityProcess process;

    private Func<bool> letGoPredicate;

    private bool fullHold;

    // Properties
    public bool Held { get { return held; } }
    private bool held;

    public PlayerAbilityHold(
        GameObject holdBar,
        AbilityProcess process,
        float minimumDuration,
        float longDuration,
        Func<bool> letGoPredicate,
        bool fullHold)
    {
        this.holdBar = holdBar;
        this.process = process;
        this.minimumDuration = minimumDuration;
        this.longDuration = longDuration;
        this.letGoPredicate = letGoPredicate;
        this.fullHold = fullHold;

        holdBarFill = holdBar.transform.Find("Hold Bar Fill").gameObject;
        holdBarScaleXMax = holdBarFill.transform.localScale.x;
    }

    public void Start()
    {
        holdTimer = 0;
        letGo = false;
        if (fullHold)
        {
            holdBar.SetActive(true);
            holdBarFill.transform.localScale =
                new Vector3(
                    0,
                    holdBarFill.transform.localScale.y,
                    holdBarFill.transform.localScale.z);
        }
        else
        {
            holdBar.SetActive(false);
        }
    }

    public void Update()
    {
        if (fullHold)
        {
            UpdateFullHold();
        }
        else
        {
            UpdateTruncatedHold();
        }
    }

    private void UpdateFullHold()
    {
        holdTimer += Time.deltaTime;
        if (letGoPredicate())
        {
            letGo = true;
        }
        
        float holdPercentage = 
            Mathf.Clamp01(holdTimer / longDuration);
        holdBarFill.transform.localScale =
            new Vector3(
                holdPercentage * holdBarScaleXMax,
                holdBarFill.transform.localScale.y,
                holdBarFill.transform.localScale.z);

        if (letGo)
        {
            if (holdTimer >= longDuration)
            {
                held = true;
                process.IndefiniteFinished = true;
            }
            else if (holdTimer >= minimumDuration)
            {
                held = false;
                process.IndefiniteFinished = true;
            }
        }
    }

    private void UpdateTruncatedHold()
    {
        holdTimer += Time.deltaTime;
        if (letGoPredicate())
        {
            letGo = true;
        }
        
        if (letGo)
        {
            if (holdTimer >= longDuration)
            {
                held = true;
                process.IndefiniteFinished = true;
                return;
            }
            else
            {
                held = false;
                process.IndefiniteFinished = true;
                return;
            }
        }

        if (holdTimer > minimumDuration && !holdBar.activeSelf)
        {
            holdBar.SetActive(true);
            holdBarFill.transform.localScale =
                new Vector3(
                    0,
                    holdBarFill.transform.localScale.y,
                    holdBarFill.transform.localScale.z);
        }

        if (holdBar.activeSelf)
        {
            float holdPercentage = 
                Mathf.Clamp01((holdTimer - minimumDuration) / (longDuration - minimumDuration));
            holdBarFill.transform.localScale =
                new Vector3(
                    holdPercentage * holdBarScaleXMax,
                    holdBarFill.transform.localScale.y,
                    holdBarFill.transform.localScale.z);
        }
    }

    public void End()
    {
        holdBar.SetActive(false);
    }
}