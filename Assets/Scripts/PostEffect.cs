using UnityEngine;

[ExecuteInEditMode]
public class PostEffect : MonoBehaviour
{
    [SerializeField]
    Material postprocessMaterial;

    Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.depthTextureMode = cam.depthTextureMode | DepthTextureMode.DepthNormals;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, postprocessMaterial);
    }
}