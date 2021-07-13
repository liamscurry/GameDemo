using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for bug testing steep hill climbing issue on character movement controller.
public class CharacterMovementSystemBasicControllerUT : MonoBehaviour
{
    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        controller.Move(Vector3.forward * 5 * Time.deltaTime);
    }
}
