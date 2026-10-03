using UnityEngine;

// AI GENERATED: authored by claude (anthropic) opus 5.5
// locks the game to 16:9 with letterbox/pillarbox bars. zero setup: it spawns itself before the first scene loads.
// - orthographic cameras keep a full-screen viewport but get a projection that puts exactly their 16:9 view
//   (orthographicSize tall, aspect forced to 16:9) inside the centered 16:9 area. cam.rect isn't used because
//   URP drops the viewport offset when post-processing is on (the image slid down by one bar height)
// - black bars on a top-most overlay canvas cover the leftover space
// - overlay UI isn't affected by any of this, so keep it under an
//   AspectRatioFitter (Fit In Parent, 1.7778) like Canvas/MainContent in the UI scene
[DefaultExecutionOrder(10000)] // run after other scripts so newly enabled cameras get fixed the same frame
public class AspectRatioLock : MonoBehaviour
{
    public const float TargetAspect = 16f / 9f;
    public static bool Enabled = true; // set false to let cameras use the full screen again

    static AspectRatioLock instance;

    RectTransform barA; // left or bottom
    RectTransform barB; // right or top
    Camera[] cameraBuffer = new Camera[8];
    Rect currentRect = new Rect(-1, -1, -1, -1); // invalid so the first frame always applies

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (instance != null) return;
        Enabled = true;
        var go = new GameObject("[AspectRatioLock]", typeof(RectTransform));
        DontDestroyOnLoad(go);
        instance = go.AddComponent<AspectRatioLock>();
    }

    void Awake()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue; // draw above every other canvas
        barA = CreateBar("BarA");
        barB = CreateBar("BarB");
    }

    RectTransform CreateBar(string barName)
    {
        var go = new GameObject(barName, typeof(RectTransform), typeof(UnityEngine.UI.Image));
        go.transform.SetParent(transform, false);
        var image = go.GetComponent<UnityEngine.UI.Image>();
        image.color = Color.black;
        image.raycastTarget = false;
        var rt = (RectTransform)go.transform;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return rt;
    }

    void LateUpdate()
    {
        Rect rect = Enabled ? ComputeViewport() : new Rect(0, 0, 1, 1);
        if (rect != currentRect)
        {
            currentRect = rect;
            UpdateBars(rect);
        }
        ApplyToCameras(rect);
    }

    // normalized viewport rect for the largest centered 16:9 area, snapped to whole pixels so bars don't leave seams
    static Rect ComputeViewport()
    {
        float screenW = Screen.width;
        float screenH = Screen.height;
        if (screenW <= 0 || screenH <= 0) return new Rect(0, 0, 1, 1);

        float w = screenW;
        float h = screenH;
        if (screenW / screenH > TargetAspect) w = Mathf.Round(screenH * TargetAspect); // too wide: pillarbox
        else h = Mathf.Round(screenW / TargetAspect); // too tall: letterbox

        float x = Mathf.Floor((screenW - w) * 0.5f);
        float y = Mathf.Floor((screenH - h) * 0.5f);
        return new Rect(x / screenW, y / screenH, w / screenW, h / screenH);
    }

    void UpdateBars(Rect rect)
    {
        if (rect.width < 1f)
        {
            SetAnchors(barA, new Vector2(0, 0), new Vector2(rect.xMin, 1));
            SetAnchors(barB, new Vector2(rect.xMax, 0), new Vector2(1, 1));
        }
        else
        {
            SetAnchors(barA, new Vector2(0, 0), new Vector2(1, rect.yMin));
            SetAnchors(barB, new Vector2(0, rect.yMax), new Vector2(1, 1));
        }
    }

    static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
    }

    void ApplyToCameras(Rect rect)
    {
        int count = Camera.allCamerasCount;
        if (count > cameraBuffer.Length) cameraBuffer = new Camera[count * 2];
        count = Camera.GetAllCameras(cameraBuffer);

        bool full = rect.width >= 1f && rect.height >= 1f;
        for (int i = 0; i < count; i++)
        {
            Camera cam = cameraBuffer[i];
            // skip render-texture cameras (minimaps etc.), they don't draw to the screen
            if (cam.targetTexture != null) continue;

            if (!cam.orthographic)
            {
                if (cam.rect != rect) cam.rect = rect; // perspective fallback; has the URP post-processing offset bug
            }
            else if (full)
            {
                cam.ResetAspect();
                cam.ResetProjectionMatrix();
            }
            else
            {
                // aspect is forced so scripts reading orthographicSize * aspect (MasterCamera bounds) see the visible 16:9 area
                cam.aspect = TargetAspect;
                cam.projectionMatrix = LetterboxedOrtho(cam, rect);
            }
        }
    }

    // ortho projection over the whole screen where the normalized `band` shows exactly the camera's 16:9 view
    static Matrix4x4 LetterboxedOrtho(Camera cam, Rect band)
    {
        float halfH = cam.orthographicSize;
        float halfW = halfH * TargetAspect;
        float unitsPerX = 2f * halfW / band.width;  // world units per normalized screen width
        float unitsPerY = 2f * halfH / band.height;
        float left = -halfW - band.xMin * unitsPerX;
        float right = halfW + (1f - band.xMax) * unitsPerX;
        float bottom = -halfH - band.yMin * unitsPerY;
        float top = halfH + (1f - band.yMax) * unitsPerY;
        return Matrix4x4.Ortho(left, right, bottom, top, cam.nearClipPlane, cam.farClipPlane);
    }
}
