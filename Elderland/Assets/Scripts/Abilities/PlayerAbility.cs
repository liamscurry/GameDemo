using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Ability structure that supports any amount of states and substates. Has an optional cooldown period.
public abstract class PlayerAbility : Ability
{
    //Fields//
    protected bool continous;
    protected bool pressedOverloadFrame { get; private set; }
    protected bool pressedIndividualFrame { get; private set; }

    private bool fallUponFinish;

    private Slider slider;
    private List<Image> cooldownStaminaIcons;
    private Color cooldownReadyColor;
    protected float staminaCost = 0;

    public bool Continous { get { return continous; } }

    public Slider CooldownSlider { get { return slider; } }

    public virtual void Initialize(PlayerAbilityManager abilitySystem)
    {
        this.system = abilitySystem;
        state = AbilityState.Waiting;
    }

    //Runs methods based on state of the ability.
    public void UpdateAbility(bool pressedOverload, bool pressedIndividual)
    {
        pressedOverloadFrame = pressedOverload;
        pressedIndividualFrame = pressedIndividual;

        switch (state)
        {
            case AbilityState.Waiting:
                Wait(true);
                break;
            case AbilityState.CoolingDown:
                CoolDown();
                break;
        }

        CheckForConstantUpdate();
    }

    //Waits for valid input.
    public bool Wait(bool firstTimeCalling)
    {
        if (system.Ready() && system.CurrentAbility == null && pressedOverloadFrame && WaitCondition())
        {
            system.CurrentAbility = this;
            state = AbilityState.InProgress;

            GlobalStart();

            if (firstTimeCalling)
            {
                system.ResetSegmentIndex();
                system.Animator.SetTrigger("runAbility");
                system.Animator.SetBool("exitAbility", false);
            }

            ActiveSegment = segments.Start;
            system.SetNextSegmentClip(segments.Start.Clip);
            fallUponFinish = false;
   
            if (slider != null)
                ZeroCoolDownIcon();

            return true;
        }
        else
        {
            return false;
        }
    }

    protected virtual bool WaitCondition() { return true; }

    protected void CheckForConstantUpdate()
    {
        if (PlayerInfo.AbilityManager.CurrentAbility == this)
        {
            if (!ActiveSegment.Finished)
            {
                GlobalConstantUpdate();
            }
        }
    }

    protected override void AdvanceSegment()
    {
        if (ActiveSegment.Next != null)
        {
            ActiveSegment = ActiveSegment.Next;
            system.SetNextSegmentClip(ActiveSegment.Clip);

            system.Animator.SetTrigger("proceedAbility");
        }
        else
        {
            if (continous)
            {
                ContinousWait();
            }
            else
            {
                system.Animator.SetBool("exitAbility", true);
                ToCoolDown();
            }

            if (fallUponFinish)
            {
                //Check if fall is valid
                if (!PlayerInfo.PhysicsSystem.OverlappingGroundContact)
                {
                    PlayerInfo.Animator.SetBool("falling", true);
                    PlayerInfo.Animator.SetTrigger("fall");
                }
                else
                {
                    system.Animator.SetTrigger("proceedAbility");
                    PlayerInfo.Animator.SetBool("falling", false);
                }
            }
            else
            {
                system.Animator.SetTrigger("proceedAbility");
            }
        }
    }

    private void ContinousWait()
    {
        system.CurrentAbility = null;
        state = AbilityState.Waiting;

        bool replayed = Wait(false);

        if (!replayed)
        {
            ActiveSegment = null;
            system.Animator.SetBool("exitAbility", true);
        }
    }

    public sealed override void ShortCircuit(bool forceNoReuse = false)
    {
        //StopCoroutine("SegmentCoroutine");
        StopAllCoroutines();

        if (ActiveProcess.End != null)
            ActiveProcess.End();

        ResetAnimatorSettings();
        ActiveSegment.Finished = true;

        ShortCircuitLogic();

        if (continous && !forceNoReuse)
        {
            system.CurrentAbility = null;
            state = AbilityState.Waiting;

            ActiveSegment = null;
            system.Animator.SetBool("exitAbility", true);
        }
        else
        {
            system.Animator.SetBool("exitAbility", true);
            ToCoolDown();
        }
        system.Animator.SetTrigger("proceedAbility");
    }

    public virtual void FallUponFinish()
    {
        fallUponFinish = true;
    }

    public virtual void ResetCooldown()
    {
        state = AbilityState.Waiting;
        coolDownTimer = 0;
    }

    protected sealed override void CoolDown()
    {
        coolDownTimer += Time.deltaTime;

        if (slider != null)
            UpdateCoolDownIcon();

        if (coolDownTimer >= coolDownDuration)
		{			
            if (slider != null)
                ReadyCoolDownIcon();
            state = AbilityState.Waiting;
		}
    }

    public virtual void GlobalConstantUpdate() { }

    protected void GenerateCoolDownIcon(float staminaCost, Sprite icon, string level)
    {
        GameObject cooldownUIObject =
            GameObject.Instantiate(
                Resources.Load(ResourceConstants.Player.UI.CooldownUI),
                GameInfo.Menu.FightingUI.transform,
                false) as GameObject;

        int remainingStaminaInt = (int) staminaCost;
        float xStart = 9;
        float xDelta = 4;

        cooldownStaminaIcons = new List<Image>();

        for (int i = 0; i < remainingStaminaInt; i++)
        {
            GameObject cooldownStaminaUIObject =
            GameObject.Instantiate(
                Resources.Load(ResourceConstants.Player.UI.CooldownStaminaUI),
                cooldownUIObject.transform,
                false) as GameObject;

            ((RectTransform) cooldownStaminaUIObject.transform).anchoredPosition =
                new Vector2(xStart + xDelta * i, 0);

            cooldownStaminaIcons.Add(cooldownStaminaUIObject.GetComponent<Image>());
        }

        float tolerance = 0.01f;
        if (staminaCost - remainingStaminaInt > 0.0f + tolerance)
        {
            GameObject cooldownStaminaUIObject =
            GameObject.Instantiate(
                Resources.Load(ResourceConstants.Player.UI.CooldownStaminaUI),
                cooldownUIObject.transform,
                false) as GameObject;

            ((RectTransform) cooldownStaminaUIObject.transform).anchoredPosition =
                new Vector2(xStart + xDelta * (remainingStaminaInt), 0);

            cooldownStaminaUIObject.transform.localScale = new Vector2(staminaCost - remainingStaminaInt, 1);
            
            cooldownStaminaIcons.Add(cooldownStaminaUIObject.GetComponent<Image>());
        }

        cooldownReadyColor = cooldownStaminaIcons[0].color;

        GameObject levelUIObject =
            GameObject.Instantiate(
                Resources.Load(ResourceConstants.Player.UI.CooldownLevelUI),
                cooldownUIObject.transform,
                false) as GameObject;

        float levelHeightOffset = -5.35f;

        ((RectTransform) levelUIObject.transform).anchoredPosition =
                new Vector2(0, levelHeightOffset);

        levelUIObject.GetComponentInChildren<Text>().text = level;
        
        slider =
            cooldownUIObject.GetComponentInChildren<Slider>();
        slider.fillRect.GetComponent<Image>().sprite = icon;

        UpdateStaminaCostIcons();
    }

    protected void DeleteAbilityIcon()
    {
        GameObject.Destroy(slider.transform.parent.gameObject);
    }

    private void UpdateCoolDownIcon()
    {
        if (!continous)
            slider.value = coolDownTimer / coolDownDuration;
    }

    private void ReadyCoolDownIcon()
    {
        if (!continous)
            slider.value = 1;
    }

    private void ZeroCoolDownIcon()
    {
        if (!continous)
            slider.value = 0;
    }

    public void UpdateStaminaCostIcons()
    {
        if (slider != null)
        {
            Color colorIndicator =
            ((system as PlayerAbilityManager).Stamina >= staminaCost) ?
            cooldownReadyColor : new Color(0.25f, 0.25f, 0.25f, 1);

            foreach (Image i in cooldownStaminaIcons)
            {
                i.color = colorIndicator;
            }
        }
    }
}