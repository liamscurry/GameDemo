using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSensor : MonoBehaviour 
{
	//Properties//
	public GameObject LadderTop { get; private set; }
	public GameObject LadderBottom { get; private set; }
	public Mantle MantleTop { get; private set; }
	public Mantle MantleBottom { get; private set; }
	public StandardInteraction Interaction { get; private set; }

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == TagConstants.LadderTop)
			LadderTop = other.gameObject;

		if (other.tag == TagConstants.LadderBottom)
			LadderBottom = other.gameObject;

		if (other.tag == TagConstants.MantleTop)
			MantleTop = other.transform.parent.GetComponent<Mantle>();

		if (other.tag == TagConstants.MantleBottom)
			MantleBottom = other.transform.parent.GetComponent<Mantle>();

		if (other.tag == TagConstants.Interactive && other.gameObject.activeInHierarchy)
			Interaction = other.transform.parent.GetComponent<StandardInteraction>();

		if (other.tag == "FallingDoorDeathTrigger")
		{
			PlayerInfo.Manager.ChangeHealth(-100f);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (other.tag == TagConstants.MantleTop && MantleTop == null)
			MantleTop = other.transform.parent.GetComponent<Mantle>();

		if (other.tag == TagConstants.MantleBottom && MantleBottom == null)
			MantleBottom = other.transform.parent.GetComponent<Mantle>();

		if (other.tag == TagConstants.Interactive && other.gameObject.activeInHierarchy && Interaction == null)
			Interaction = other.transform.parent.GetComponent<StandardInteraction>();
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.tag == TagConstants.LadderTop)
			LadderTop = null;

		if (other.tag == TagConstants.LadderBottom)
			LadderBottom = null;

		if (other.tag == TagConstants.MantleTop)
			MantleTop = null;

		if (other.tag == TagConstants.MantleBottom)
			MantleBottom = null;

		if (other.tag == TagConstants.Interactive) 
			Interaction = null;
	}
}
