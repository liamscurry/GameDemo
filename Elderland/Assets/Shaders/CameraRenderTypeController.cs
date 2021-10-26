using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CameraRenderTypeController : MonoBehaviour
{
    private void Update()
    {
        if (Camera.current != null)
        {
            Camera.current.depthTextureMode = DepthTextureMode.DepthNormals;
        }
    }
}
