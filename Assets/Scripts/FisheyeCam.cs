//from https://github.com/i-DAT/unity-fulldome-camera/blob/main/Runtime/FisheyeCam.cs

// using UnityEngine;
// using UnityEngine.Rendering.Universal;
// using UnityEngine.UI;

// [ExecuteAlways]
// public class FisheyeCam : MonoBehaviour
// {
//     public Shader shader;
//     public int resolution = 512;
//     public float fov = 180;

//     new Camera camera;
//     RenderTexture texture;
//     Material material;
//     GameObject canvas;

//     void OnEnable()
//     {
//         // To enable realtime preview in editor, we need to manage resources when the script is enabled.

//         // Grab the camera and disable its postprocessing - this ensures there are no visible seams 
//         // on the cubemap from vignette.
//         // TODO: this should specifically disable vignette to allow other postprocessing effects.
//         camera = GetComponent<Camera>();
//         gameObject.GetComponent<UniversalAdditionalCameraData>().renderPostProcessing = false;

//         // Create a new cubemap texture at the desired resolution.
//         texture = new(resolution, resolution, 24);
//         texture.dimension = UnityEngine.Rendering.TextureDimension.Cube;
//         texture.filterMode = FilterMode.Trilinear;
//         texture.anisoLevel = 4;
//         texture.Create();

//         // Create a material from the shader and assign the cubemap texture to its _MainTex slot.
//         material = new Material(shader);
//         material.SetTexture("_MainTex", texture);

//         if (GameObject.Find("FisheyeCanvas") is GameObject c)
//         {
//             canvas = c;
//         }
//         else
//         {
//             // If there is no canvas object yet, create it, and set it to display our material.
//             canvas = new GameObject("FisheyeCanvas");
//             canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
//             canvas.AddComponent<CanvasScaler>();
//             canvas.AddComponent<GraphicRaycaster>();
//             canvas.AddComponent<RawImage>();
//         }
//         canvas.GetComponent<RawImage>().material = material;
//     }

//     void OnDisable()
//     {
//         DestroyImmediate(texture);
//         DestroyImmediate(material);
//         DestroyImmediate(canvas);
//     }

//     void LateUpdate()
//     {
//         // After each Update() frame, update the shader with the camera rotation and FOV, then
//         // render to the cubemap texture on all faces (bitmap 63).
//         // This is an expensive operation as the scene is rendered 6 times!
//         // TODO: backface culling to reduce to 5?
//         material.SetMatrix("_CameraRotation", Matrix4x4.Rotate(camera.transform.rotation));
//         material.SetFloat("_FOV", fov);
//         camera.RenderToCubemap(texture, 63);
//     }

//     void OnValidate()
//     {
//         // When the texture resolution value is updated in editor, resize the texture.
//         // Unity doesn't really like this hack and sometimes throws errors, but it shouldn't
//         // affect a built version of the game.
//         if (texture != null && 0 < resolution && resolution <= 4096)
//         {
//             texture.Release();
//             texture.width = resolution;
//             texture.height = resolution;
//             texture.Create();
//         }
//     }
// }


using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

[ExecuteAlways]
public class FisheyeCam : MonoBehaviour
{
    public Shader shader;
    public int resolution = 512;
    public float fov = 180;

    new Camera camera;
    RenderTexture cubemapTexture;
    RenderTexture fisheyeOutput;
    Material material;
    GameObject canvas;

    void OnEnable()
    {
        camera = GetComponent<Camera>();

        // Disable vignette to avoid cubemap seams (disable all post by default)
        var camData = gameObject.GetComponent<UniversalAdditionalCameraData>();
        if (camData != null)
            camData.renderPostProcessing = false;

        CreateTextures();
        CreateMaterial();
        SetupCanvas();
    }

    void OnDisable()
    {
        CleanupResources();
    }

    void LateUpdate()
    {
        if (material == null || cubemapTexture == null || fisheyeOutput == null) return;

        // Update shader uniforms
        material.SetMatrix("_CameraRotation", Matrix4x4.Rotate(camera.transform.rotation));
        material.SetFloat("_FOV", fov);

        // Render the scene to a cubemap
        camera.RenderToCubemap(cubemapTexture, 63);

        // Apply fisheye projection via shader to 2D output
        Graphics.Blit(cubemapTexture, fisheyeOutput, material);
    }

    void OnValidate()
    {
        if (cubemapTexture != null && resolution > 0 && resolution <= 4096)
        {
            cubemapTexture.Release();
            fisheyeOutput.Release();

            cubemapTexture.width = resolution;
            cubemapTexture.height = resolution;
            cubemapTexture.Create();

            fisheyeOutput.width = resolution;
            fisheyeOutput.height = resolution;
            fisheyeOutput.Create();
        }
    }

    void CreateTextures()
    {
        cubemapTexture = new RenderTexture(resolution, resolution, 24);
        cubemapTexture.dimension = UnityEngine.Rendering.TextureDimension.Cube;
        cubemapTexture.filterMode = FilterMode.Trilinear;
        cubemapTexture.anisoLevel = 4;
        cubemapTexture.Create();

        fisheyeOutput = new RenderTexture(resolution, resolution, 0);
        fisheyeOutput.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
        fisheyeOutput.filterMode = FilterMode.Bilinear;
        fisheyeOutput.Create();
    }

    void CreateMaterial()
    {
        if (shader == null)
        {
            Debug.LogError("FisheyeCam: Shader not assigned.");
            return;
        }
        material = new Material(shader);
        material.SetTexture("_MainTex", cubemapTexture);
    }

    public RawImage targetImg;

    void SetupCanvas()
    {
        // RawImage rawImage;

        // if ((canvas = GameObject.Find("FisheyeCanvas")) == null)
        // {
        //     canvas = new GameObject("FisheyeCanvas");
        //     var c = canvas.AddComponent<Canvas>();
        //     c.renderMode = RenderMode.ScreenSpaceOverlay;
        //     canvas.AddComponent<CanvasScaler>();
        //     canvas.AddComponent<GraphicRaycaster>();
        //     rawImage = canvas.AddComponent<RawImage>();
        // }
        // else
        // {
        //     rawImage = canvas.GetComponent<RawImage>();
        //     if (rawImage == null)
        //         rawImage = canvas.AddComponent<RawImage>();
        // }

        targetImg.texture = fisheyeOutput;
        targetImg.material = null; // Important: avoid dimension conflict
    }

    void CleanupResources()
    {
        if (Application.isPlaying)
        {
            Destroy(cubemapTexture);
            Destroy(fisheyeOutput);
            Destroy(material);
        }
        else
        {
            DestroyImmediate(cubemapTexture);
            DestroyImmediate(fisheyeOutput);
            DestroyImmediate(material);
        }

        if (canvas != null && canvas.name == "FisheyeCanvas")
        {
            if (Application.isPlaying)
                Destroy(canvas);
            else
                DestroyImmediate(canvas);
        }
    }
}
