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
    private PortalTeleporter targetTeleporter;

    // Fields
    private bool drawPlane;
    private PortalObjectManager objectManager;
    
    private void Start()
    {
        objectManager = 
            transform.parent.GetComponentInChildren<PortalObjectManager>();
    }

    public void Update()
    { 
        UpdateMirroredCamera();
    }

    public void RootMirror(Transform root, Transform target)
    {
        Matrix4x4 targetMatrix =
            targetTeleporter.transform.localToWorldMatrix;
        Vector3 globalPlayerPosition =
            target.position;
        Vector3 localPlayerPosition = 
            transform.worldToLocalMatrix.MultiplyPoint(globalPlayerPosition);
        localPlayerPosition.x *= -1;
        localPlayerPosition.z *= -1;
        Vector3 targetGlobalPlayerPosition = 
            targetMatrix.MultiplyPoint(localPlayerPosition);
        root.position = targetGlobalPlayerPosition;

        Quaternion teleporterRotation = 
            targetTeleporter.transform.rotation *
            Quaternion.Inverse(transform.rotation) *
            Quaternion.Euler(0, 180, 0) *
            target.rotation;
        root.rotation = teleporterRotation;
    }

    public void TeleportPlayer()
    {
        UpdateMirroredCamera();
        Vector3 modelOffset = 
        PlayerInfo.Player.transform.Find("Model").Find("Armature").position - 
            PlayerInfo.Player.transform.position;
        Transform armature = 
            objectManager.PlayerCopy.transform.Find("Armature");
        PlayerInfo.Player.transform.position = 
            armature.position;
        PlayerInfo.Player.transform.position += modelOffset;
        PlayerInfo.Player.transform.rotation =
            Quaternion.LookRotation(targetTeleporter.transform.forward, Vector3.up);
        
        GameInfo.CameraController.SetDirection(renderCamera.transform);
        GameInfo.CameraController.TargetDirection = Vector3.zero;
    }

    /*
	* Needed to update transform backings in cutscene yet not move player to retain trigger copy on
	* real teleport.
	*/
    public void PreTeleportPlayer(
        out Vector3 position,
        out Quaternion rotation,
        out Vector3 cameraForward)
    {
        UpdateMirroredCamera();
        position = PlayerInfo.Player.transform.position;
        rotation = PlayerInfo.Player.transform.rotation;
        cameraForward = renderCamera.transform.forward;

        Vector3 modelOffset = 
        PlayerInfo.Player.transform.Find("Model").Find("Armature").position - 
            position;
        Transform armature = 
            objectManager.PlayerCopy.transform.Find("Armature");
        position = 
            armature.position;
        position += modelOffset;
        rotation =
            Quaternion.LookRotation(targetTeleporter.transform.forward, Vector3.up);
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

    /*
    * Needed to update target teleporter camera transform and force update before teleporting.
    */
    private void UpdateMirroredCamera()
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
        }
    }
}
