using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* IK manager to update each IK system at the correct time and adjust the vertical position of the model.
* This ensures the model's feet aren't floating or bending when they shouldn't. This is generally
* placed on the armature object that is a parent of all the bone hierarchy.
*/
public class IKSystemManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private IKSystem[] IKSystems;
    [Header("Optional")]
    // Capsule of character this is attached to. If used, will adjust this models y coordinate
    // so leg IK targets are touching the floor.
    [SerializeField]
    private CapsuleCollider clampCapsule;
    // Top level parent of mesh/IK setup. Generally a child of the manager object.
    [SerializeField]
    private GameObject clampModelParent;
    // Distance for which the model is offset vertically down when clamping.
    [SerializeField]
    private float overClamp;
    [SerializeField]
    private float maxClampAngle;
    [SerializeField]
    private float clampSpeed;

    private float clampOffset;
    private float currentClampPerc;

    private void Start()
    {     
        Initialize();
    }

    private void LateUpdate()
    {
        if (clampCapsule != null)
            ClampSystem();
        
        foreach (var system in IKSystems)
        {
            system.UpdateSystem();
        }
    }

    private void Initialize()
    {
        clampOffset = 
            clampModelParent.transform.position.y - clampCapsule.transform.position.y;
        currentClampPerc = 0;

        foreach (var system in IKSystems)
        {
            system.InitializeSystem();
        }
    }

    /*
    * Clamps system model parent to ground so leg limbs targets are near the ground. 
    * This reduces floating leg targets or craped leg targets.
    */
    private void ClampSystem()
    {
        RaycastHit clampHit;
        bool collided =
            Physics.Raycast(
                clampCapsule.transform.position,
                Vector3.down,
                out clampHit,
                clampCapsule.height,
                LayerConstants.GroundCollision);
        
        if (collided)
        {
            float distanceToMoveModel =
                clampHit.distance - (clampCapsule.height / 2);

            float overClampPercentage = 
                Mathf.Clamp01(Matho.AngleBetween(Vector3.up, clampHit.normal) / maxClampAngle);
            currentClampPerc =
                Mathf.MoveTowards(currentClampPerc, overClampPercentage, clampSpeed * Time.deltaTime);
            distanceToMoveModel += overClamp * currentClampPerc;

            clampModelParent.transform.position = 
                new Vector3(
                    clampModelParent.transform.position.x,
                    clampCapsule.transform.position.y + clampOffset - distanceToMoveModel,
                    clampModelParent.transform.position.z);
        }
    }
}