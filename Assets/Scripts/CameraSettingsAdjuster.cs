using UnityEngine;

public class CameraSettingsAdjuster : MonoBehaviour
{

    private Camera cam;
    [SerializeField] private Color bgColor = Color.white;
    [SerializeField] private bool useSkybox = true;
    [SerializeField] private Material skybox;
    void Start()
    {
        cam = Camera.main;

        if (useSkybox)
        {
            cam.clearFlags = CameraClearFlags.Skybox;
            if (skybox)
            {
                RenderSettings.skybox = skybox;
            }
        }
        else
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = bgColor;
        }
    }

}
