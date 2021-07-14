using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for bug testing steep hill climbing issue on character movement controller.
// Character controller seems to have built in bug that it can climb slopes higher than the slope limit.
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
