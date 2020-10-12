using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Always stays on the screen and faces the camera. Updates distance to player.
public class WaypointUI : MonoBehaviour
{
    [SerializeField]
    private Vector3 worldPosition;

    private Text distanceText;

    private void Awake()
    {
        distanceText = GetComponentInChildren<Text>();
    }

    private void Update()
    {
        if (Camera.current != null)
        {
            Vector3 waypointScreenPosition =
                Camera.current.WorldToScreenPoint(worldPosition);
            Vector3 waypointCameraPosition = Camera.current.worldToCameraMatrix.MultiplyPoint(worldPosition);

            float waypointPolarAngle =
                Matho.Angle(new Vector2(waypointCameraPosition.x, -waypointCameraPosition.z));
            Vector2 waypointClampOffset =
                new Vector2(Mathf.Cos(waypointPolarAngle * Mathf.Deg2Rad), Mathf.Sin(waypointPolarAngle * Mathf.Deg2Rad));

            /*
            Vector3 waypointClampPosition = 
                new Vector3(Camera.current.pixelWidth / 2, Camera.current.pixelHeight / 2, 0) + 
                new Vector3(waypointClampOffset.x * Camera.current.pixelHeight * 4f, waypointClampOffset.y * Camera.current.pixelHeight * 4f, 0);
            */

            Vector3 waypointCameraDirection = waypointCameraPosition.normalized;
            Vector2 projectedWaypointCameraDirection = new Vector2(waypointCameraDirection.x, waypointCameraDirection.y).normalized;
            Vector3 waypointClampDirection = 
                new Vector3(Camera.current.pixelWidth / 2, Camera.current.pixelHeight / 2, 0) + 
                new Vector3(projectedWaypointCameraDirection.x * Camera.current.pixelHeight * .4f, projectedWaypointCameraDirection.y * Camera.current.pixelHeight * .4f, 0);

            float radius = Camera.current.pixelHeight * .4f;

            //Debug.Log(waypointCameraPosition.z);

            if ((new Vector2(waypointScreenPosition.x, waypointScreenPosition.y) -
                new Vector2(Camera.current.pixelWidth / 2f, Camera.current.pixelHeight / 2f)).magnitude > radius ||
                waypointCameraPosition.z > 0)
            {
                waypointScreenPosition = waypointClampDirection;
            }



            /*
            waypointClampDirection.x =
                Mathf.Clamp(waypointClampDirection.x, Camera.current.pixelWidth * 0.1f, Camera.current.pixelWidth * 0.9f);

            waypointClampDirection.y =
                Mathf.Clamp(waypointClampDirection.y, Camera.current.pixelHeight * 0.1f, Camera.current.pixelHeight * 0.9f);*/

            /*
            if (waypointClampPosition.x > Camera.current.pixelWidth * 0.9f)
            {
                waypointClampPosition.x = Camera.current.pixelWidth * 0.9f;
            }
            else if (waypointClampPosition.x < Camera.current.pixelWidth * 0.1f)
            {
                waypointClampPosition.x = Camera.current.pixelWidth * 0.1f;
            }

            if (waypointClampPosition.y > Camera.current.pixelHeight * 0.9f)
            {
                waypointClampPosition.y = Camera.current.pixelHeight * 0.9f;
            }
            else if (waypointClampPosition.y < Camera.current.pixelHeight * 0.1f)
            {
                waypointClampPosition.y = Camera.current.pixelHeight * 0.1f;
            }*/


            //Vector2 centerScreenPositionOffset =
            //    new Vector2(waypointScreenPosition.x, waypointScreenPosition.y) - new Vector2(Camera.current.pixelWidth / 2, Camera.current.pixelHeight / 2);
            

            
            /*
            if (waypointScreenPosition.x < Camera.current.pixelWidth * 0.1f ||
                waypointScreenPosition.x > Camera.current.pixelWidth * 0.9f)
            {   
                waypointScreenPosition.x = waypointClampDirection.x;
            }

            if (waypointScreenPosition.y < Camera.current.pixelHeight * 0.1f ||
                waypointScreenPosition.y > Camera.current.pixelHeight * 0.9f)
            {   
                waypointScreenPosition.y = waypointClampDirection.y;
            }*/
                // * (1 - Matho.AngleBetween(new Vector3(0, 0, 1), waypointCameraPosition) / 180f)
            /*
            waypointScreenPosition = 
                new Vector3(Camera.current.pixelWidth / 2, Camera.current.pixelHeight / 2, 0) +
                (new Vector3(200 * Mathf.Sign(waypointCameraPosition.x), -200 * Mathf.Sign(waypointCameraPosition.z), 0));*/

                // * (1 - Matho.AngleBetween(new Vector3(0, 0, 1), waypointCameraPosition) / 180f)
            
            /*waypointScreenPosition =
                new Vector3(Camera.current.pixelWidth / 2, Camera.current.pixelHeight / 2, 0) +
                new Vector3(200 * Mathf.Sign(waypointCameraPosition.x), 0, 0) +
                new Vector3(0, -200 * Mathf.Sign(waypointCameraPosition.z), 0);*/
            //Debug.Log(Mathf.Cos(200 * waypointPolarAngle * Mathf.Deg2Rad));
            /*
            //waypointScreenPosition.x = waypointScreenPosition.x % Camera.current.pixelWidth;
            Debug.Log(waypointScreenPosition.x + ", " + waypointScreenPosition.y);
            if (waypointScreenPosition.x < Camera.current.pixelWidth * 0.1f ||
                waypointScreenPosition.x > Camera.current.pixelWidth * 0.9f)
            {
                Vector3 waypointCameraDisplacement = worldPosition - Camera.current.transform.position;
                float horizontalAngle = Matho.AngleBetween(waypointCameraDisplacement, Camera.current.transform.forward);
                waypointScreenPosition.x = Camera.current.pixelWidth * 0.1f;
            }
            */
            /*
            if (Mathf.Abs(waypointScreenPosition.x) < Camera.current.pixelWidth * 0.1f ||
                Mathf.Abs(waypointScreenPosition.x) > Camera.current.pixelWidth * 0.9f)
            {
                waypointScreenPosition.x = Camera.current.pixelWidth / 2f + (Camera.current.pixelWidth / 2f) * 0.95f * waypointClampPosition.x;
            }

            if (Mathf.Abs(waypointScreenPosition.y) < Camera.current.pixelHeight * 0.1f ||
                Mathf.Abs(waypointScreenPosition.y) > Camera.current.pixelHeight * 0.9f)
            {
                waypointScreenPosition.y = Camera.current.pixelHeight / 2f + (Camera.current.pixelHeight / 2f) * 0.95f * waypointClampPosition.y;
            }*/

            /*
            if (Mathf.Abs(waypointScreenPosition.y) < Camera.current.pixelHeight * 0.1f)
                waypointScreenPosition.y = Camera.current.pixelHeight * 0.1f;
            if (Mathf.Abs(waypointScreenPosition.y) > Camera.current.pixelHeight * 0.9f)
                waypointScreenPosition.y = Camera.current.pixelHeight * 0.9f;*/
            ((RectTransform) transform).anchoredPosition = waypointScreenPosition;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(worldPosition, Vector3.one);
    }
}
