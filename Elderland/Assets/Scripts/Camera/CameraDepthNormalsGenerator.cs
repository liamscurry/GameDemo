using UnityEngine;

// References: William Chyr. See References file for more details.
[ExecuteInEditMode]
public class CameraDepthNormalsGenerator : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.DepthNormals;
    }
}