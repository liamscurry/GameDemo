using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* Monobehaviour needed so that the animator can call function events in animations.
*/
public class PlayerAnimEventConnector : MonoBehaviour
{
    [SerializeField]
    private GameObject meleeWeapon;
    [SerializeField]
    private GameObject meleeHand;

    private Transform meleeWeaponStartParent;
    private Vector3 startLocalMeleePos;
    private Quaternion startLocalMeleeRot;

    private void Start()
    {
        meleeWeaponStartParent = meleeWeapon.transform.parent;
        startLocalMeleePos = meleeWeapon.transform.localPosition;
        startLocalMeleeRot = meleeWeapon.transform.localRotation;
    }

    public void GraspMelee()
    {
        meleeWeapon.transform.parent = meleeHand.transform;
        meleeWeapon.GetComponent<CharacterProp>().enabled = false;
    }

    public void PutAwayMelee()
    {
        meleeWeapon.transform.parent = meleeWeaponStartParent;
        meleeWeapon.GetComponent<CharacterProp>().enabled = true;
        meleeWeapon.transform.localPosition = startLocalMeleePos;
        meleeWeapon.transform.localRotation = startLocalMeleeRot;
    }
}
