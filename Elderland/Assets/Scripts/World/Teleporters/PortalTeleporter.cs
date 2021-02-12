using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Construct for portal teleporters to and from the safe haven. Only renders
// costly preview plane image when in range of trigger.
public class PortalTeleporter : MonoBehaviour
{
    [SerializeField]
    private Camera renderCamera;
    [SerializeField]
    private SkinnedMeshRenderer playerRendererCopy;
    [SerializeField]
    private SkinnedMeshRenderer playerRenderer;
    [SerializeField]
    private PortalTeleporter targetTeleporter;

    // Fields
    private bool drawPlane;

    public void Update()
    { 
        if (drawPlane && targetTeleporter != null)
        {
            // neeed to have initial global rotaion of camera to be local to camera. rotate based on current teleporter (this)
            Vector3 globalCameraPosition =
                GameInfo.CameraController.transform.position;
            Vector3 localCameraPosition = 
                transform.worldToLocalMatrix.MultiplyPoint(globalCameraPosition);
            Matrix4x4 targetMatrix =
                targetTeleporter.transform.localToWorldMatrix;
            Matrix4x4 rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 180, 0));
            Quaternion globalCameraRotation = 
                targetTeleporter.transform.rotation * Quaternion.Inverse(transform.rotation) * Quaternion.Euler(0, 180, 0) * GameInfo.CameraController.transform.rotation;
            Vector3 targetGlobalPosition = 
                rotationMatrix.MultiplyPoint(localCameraPosition);
            targetGlobalPosition = 
                targetMatrix.MultiplyPoint(targetGlobalPosition);
            renderCamera.transform.position = targetGlobalPosition;
            renderCamera.transform.rotation = globalCameraRotation;

            renderCamera.fieldOfView = GameInfo.CameraController.Camera.fieldOfView;
            renderCamera.Render();

            // assume created from before (will add create and destory system on touching the teleporter)
            playerRendererCopy.materials = playerRenderer.materials;
            Vector3 globalPlayerPosition =
                PlayerInfo.Player.transform.position;
            Vector3 localPlayerPosition = 
                transform.worldToLocalMatrix.MultiplyPoint(globalPlayerPosition);
            localPlayerPosition.x *= -1;
            localPlayerPosition.z *= -1;
            Vector3 targetGlobalPlayerPosition = 
                targetMatrix.MultiplyPoint(localPlayerPosition);
            playerRendererCopy.transform.parent.position = targetGlobalPlayerPosition;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == TagConstants.PlayerHitbox)
        {
            drawPlane = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == TagConstants.PlayerHitbox)
        {
            drawPlane = false;
        }
    }
}
