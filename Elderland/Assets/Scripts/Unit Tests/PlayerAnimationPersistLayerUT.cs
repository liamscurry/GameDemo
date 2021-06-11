using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationPersistLayerUT : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(TestCoroutine());
    }

    private IEnumerator TestCoroutine()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        try
        {
            PlayerAnimationPersistLayer.ClaimTurnOnUT();
            PlayerAnimationPersistLayer.ClaimTurnOffUT();

            Debug.Log("PlayerAnimationPersistLayerUT: Success");
        } 
        catch (Exception e)
        {
            Debug.Log("PlayerAnimationPersistLayerUT: Failed. " + e.Message + " " + e.StackTrace);
        }
    }
}