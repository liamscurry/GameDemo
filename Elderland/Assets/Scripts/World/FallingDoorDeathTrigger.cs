using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingDoorDeathTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
        //&&
        //    transform.position.y > PlayerInfo.Player.transform.position.y + PlayerInfo.Capsule.height / 4f
        if (other.tag == "PlayerHealth" )
        {
            
        }
    }
}
