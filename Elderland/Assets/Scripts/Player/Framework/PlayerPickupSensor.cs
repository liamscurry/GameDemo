using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickupSensor : MonoBehaviour 
{
	private void OnTriggerStay(Collider other)
	{
		if (other.tag == TagConstants.Pickup)
        {
            Pickup pickup = other.GetComponent<Pickup>();
            if (pickup.IsSeekValid())
            {
                pickup.SeekPlayer();
            }
        }
	}
}
