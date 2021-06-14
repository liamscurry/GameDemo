using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* Class for interacting with player combat layer. Allows for ease of use to turn on/off a persistent layer
* with smoothing.
*/

// Solved: TODO: fix issue where when turning on layer, it stutters every few seconds after turning on layer.
// Sln: Don't have animator window open while playing, no stutter when closed.
// Tested initial implementation, passed 5/15/21.
public class PlayerAnimationPersistLayer
{
    // Fields
    private readonly string layerName;
    private readonly float transitionDur;

    private readonly int layerIndex;
    private IEnumerator weightSmoothCoro;

    private Object user;

    public PlayerAnimationPersistLayer(float transitionDur, string layerName)
    {
        this.transitionDur = transitionDur;
        layerIndex = PlayerInfo.Animator.GetLayerIndex(layerName);
        weightSmoothCoro = null;
        this.layerName = layerName;
    }

    public void TurnOn()
    {
        if (weightSmoothCoro != null)
        {
            PlayerInfo.Manager.StopCoroutine(weightSmoothCoro);
        }

        weightSmoothCoro = FadeWeight(0, 1);
        PlayerInfo.Manager.StartCoroutine(weightSmoothCoro);
    }

    public void TurnOff()
    {
        if (weightSmoothCoro != null)
        {
            PlayerInfo.Manager.StopCoroutine(weightSmoothCoro);
        }

        weightSmoothCoro = FadeWeight(1, 0);
        PlayerInfo.Manager.StartCoroutine(weightSmoothCoro);
    }

    public void ClaimTurnOn(Object user)
    {
        this.user = user;
        if (weightSmoothCoro != null)
        {
            PlayerInfo.Manager.StopCoroutine(weightSmoothCoro);
        }

        int layerIndex = PlayerInfo.Animator.GetLayerIndex(layerName);
        float layerWeight = PlayerInfo.Animator.GetLayerWeight(layerIndex);
        weightSmoothCoro = FadeWeight(layerWeight, 1);
        PlayerInfo.Manager.StartCoroutine(weightSmoothCoro);
    }

    public void ClaimTurnOff(Object user)
    {
        if (this.user = user)
        {
            user = null;
            if (weightSmoothCoro != null)
            {
                PlayerInfo.Manager.StopCoroutine(weightSmoothCoro);
            }

            int layerIndex = PlayerInfo.Animator.GetLayerIndex(layerName);
            float layerWeight = PlayerInfo.Animator.GetLayerWeight(layerIndex);
            weightSmoothCoro = FadeWeight(layerWeight, 0);
            PlayerInfo.Manager.StartCoroutine(weightSmoothCoro);
        }
    }

    private IEnumerator FadeWeight(float start, float target)
    {
        float timer = 0;
        while (timer < transitionDur)
        {
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
            float percentWeight = timer / transitionDur;
            PlayerInfo.Animator.SetLayerWeight(layerIndex, Mathf.Lerp(start, target, percentWeight));
        }
        PlayerInfo.Animator.SetLayerWeight(layerIndex, target);

        weightSmoothCoro = null;
    }

    public static void ClaimTurnOnUT()
    {
        Object obj1 = new Object();
        Object obj2 = new Object();
        var layer = new PlayerAnimationPersistLayer(1f, "test");
        UT.CheckEquality<bool>(layer.user == null, true);  
        layer.ClaimTurnOn(obj1);
        UT.CheckEquality<bool>(layer.user == obj1, true);  
        layer.ClaimTurnOn(obj2);
        UT.CheckEquality<bool>(layer.user == obj2, true);  
    }

    public static void ClaimTurnOffUT()
    {
        Object obj1 = new Object();
        Object obj2 = new Object();
        var layer = new PlayerAnimationPersistLayer(1f, "test");
        UT.CheckEquality<bool>(layer.user == null, true);  
        layer.ClaimTurnOn(obj1);
        UT.CheckEquality<bool>(layer.user == obj1, true);  
        layer.ClaimTurnOff(obj2);
        UT.CheckEquality<bool>(layer.user == obj1, true);  
        layer.ClaimTurnOn(obj2);
        layer.ClaimTurnOff(obj2);
        UT.CheckEquality<bool>(layer.user == null, true);  
    }
}
