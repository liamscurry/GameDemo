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

    protected bool replayed;
    public bool Continous { get { return continous; } }

    public Slider CooldownSlider { get { return slider; } }

    public virtual void Initialize(PlayerAbilityManager abilitySystem)
    {
        this.system = abilitySystem;
        state = AbilityState.Waiting;
        replayed = false;
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
    public virtual bool Wait(bool firstTimeCalling)
    {
        if (system.Ready() && system.CurrentAbility == null && pressedOverloadFrame && WaitCondition())
        {
            system.CurrentAbility = this;
            state = AbilityState.InProgress;

            replayed = true;

            GlobalStart();

            if (firstTimeCalling)
            {
                //system.AnimationLoop.ResetSegmentIndex();
                system.Animator.SetTrigger("runAbility");
                system.Animator.SetBool("exitAbility", false);
                system.Animator.ResetTrigger("proceedAbility");
            }

            ActiveSegment = segments.Start;
            SetNextAnimationClips();
            PlayerInfo.Animator.SetInteger(
                "choiceSeparator",
                PlayerInfo.AbilityManager.AnimationLoop.CurrentSegmentIndex);
            fallUponFinish = false;
   
            if (slider != null)
                ZeroCoolDownIcon();

            return true;
        }
        else
        {
            replayed = false;
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
            SetNextAnimationClips();
            //system.AnimationLoop.SetNextSegmentClip(ActiveSegment.Clip);

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

    private void SetNextAnimationClips()
    {
        AnimationClip[] lowerUpperClips = null;
        if (ActiveSegment.UpperClip != null)
        {
            lowerUpperClips =
                new AnimationClip[2] { ActiveSegment.Clip, ActiveSegment.UpperClip };
        }
        else
        {
            lowerUpperClips =
                new AnimationClip[2] { ActiveSegment.Clip, ActiveSegment.Clip };
        }
            
        string[] lowerUpperNames = 
            new string[2] { "Segment", "AbilityUpperSegment" };
        system.AnimationLoop.SetNextSegmentClip(lowerUpperClips, lowerUpperNames);
    }

    private void ContinousWait()
    {
        system.CurrentAbility = null;
        state = AbilityState.Waiting;

        bool replayedCheck = Wait(false);
        
        if (!replayedCheck)
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
                Resources.Load(ResourceConstants.Player.Abilities.CooldownUI),
                GameInfo.Menu.FightingUI.transform,
                false) as GameObject;

        int remainingStaminaInt = (int) staminaCost;
        float xStart = 9;
        float xDelta = 4;

        cooldownStaminaIcons = new List<Image>();

        float tolerance = 0.01f;
        if (staminaCost > tolerance)
        {
            for (int i = 0; i < remainingStaminaInt; i++)
            {
                GameObject cooldownStaminaUIObject =
                GameObject.Instantiate(
                    Resources.Load(ResourceConstants.Player.Abilities.CooldownStaminaUI),
                    cooldownUIObject.transform,
                    false) as GameObject;

                ((RectTransform) cooldownStaminaUIObject.transform).anchoredPosition =
                    new Vector2(xStart + xDelta * i, 0);

                cooldownStaminaIcons.Add(cooldownStaminaUIObject.GetComponent<Image>());
            }

            if (staminaCost - remainingStaminaInt > 0.0f + tolerance)
            {
                GameObject cooldownStaminaUIObject =
                GameObject.Instantiate(
                    Resources.Load(ResourceConstants.Player.Abilities.CooldownStaminaUI),
                    cooldownUIObject.transform,
                    false) as GameObject;

                ((RectTransform) cooldownStaminaUIObject.transform).anchoredPosition =
                    new Vector2(xStart + xDelta * (remainingStaminaInt), 0);

                cooldownStaminaUIObject.transform.localScale = new Vector2(staminaCost - remainingStaminaInt, 1);
                
                cooldownStaminaIcons.Add(cooldownStaminaUIObject.GetComponent<Image>());
            }

            cooldownReadyColor = cooldownStaminaIcons[0].color;
        }

        GameObject levelUIObject =
            GameObject.Instantiate(
                Resources.Load(ResourceConstants.Player.Abilities.CooldownLevelUI),
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

    protected AnimationClip GetDirAnim(string key, Vector2 direction)
    {
        float dirAngle;

        if (direction.x >= 0)
        {
            dirAngle = Matho.AngleBetween(Vector2.right, direction);
        }
        else
        {
            dirAngle = Matho.AngleBetween(-Vector2.right, direction);
        }

        if (direction.y >= 0 && dirAngle > 45 + 45f / 2)
        {
            return PlayerInfo.AnimationManager.GetAnim(
                key + "Forward");
        }
        else if (direction.y < 0 && dirAngle > 45 + 45f / 2)
        {
            return PlayerInfo.AnimationManager.GetAnim(
                key + "Backward");
        }
        else
        {   
            if (direction.x >= 0)
            {
                return PlayerInfo.AnimationManager.GetAnim(
                    key + "Right");
            }
            else
            {
                return PlayerInfo.AnimationManager.GetAnim(
                    key + "Left");
            }
        }
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