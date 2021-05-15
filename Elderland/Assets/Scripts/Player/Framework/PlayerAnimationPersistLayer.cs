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
    private readonly float transitionDur;

    private readonly int layerIndex;
    private IEnumerator weightSmoothCoro;

    public PlayerAnimationPersistLayer(float transitionDur, string layerName)
    {
        this.transitionDur = transitionDur;
        layerIndex = PlayerInfo.Animator.GetLayerIndex(layerName);
        weightSmoothCoro = null;
    }

    public bool TryTurnOn()
    {
        if (weightSmoothCoro == null)
        {
            weightSmoothCoro = FadeWeight(0, 1);
            PlayerInfo.Manager.StartCoroutine(weightSmoothCoro);
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool TryTurnOff()
    {
        if (weightSmoothCoro == null)
        {
            weightSmoothCoro = FadeWeight(1, 0);
            PlayerInfo.Manager.StartCoroutine(weightSmoothCoro);
            return true;
        }
        else
        {
            return false;
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
}
