using System.Collections;
using UnityEngine;
using TMPro;

public class AuraFloaterUI : MonoBehaviour
{
    [Header("Canvas Container")]
    [SerializeField] private RectTransform container; // defaults to self
    [Header("Camera (for world -> screen)")]
    [SerializeField] private Camera playerCamera;

    [Header("Look")]
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private int fontSize = 52;
    [SerializeField] private Color color = Color.yellow;

    [Header("Motion")]
    [SerializeField] private Vector2 drift = new Vector2(0f, 60f);
    [SerializeField] private float lifetime = 0.8f;
    [SerializeField] private float fadeTime = 0.25f;

    public static AuraFloaterUI Instance;

    private void Awake()
    {
        if (container == null) container = GetComponent<RectTransform>();
        if (playerCamera == null) playerCamera = Camera.main;
        Instance = this;
    }

    // Allow other systems to spawn a custom text at a world position
    public static void SpawnStatic(Vector3 worldPos, string text)
    {
        if (Instance == null) return;
        Instance.SpawnCustom(text, worldPos);
    }

    private void SpawnCustom(string text, Vector3 worldPos)
    {
        GameObject go = new GameObject("AuraFloater", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(container, false);

        var rt = go.GetComponent<RectTransform>();
        var tmp = go.GetComponent<TextMeshProUGUI>();

        if (font != null) tmp.font = font;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        tmp.text = text;

        // keep on one line + don’t shrink
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.enableAutoSizing = false;

        // size rect to text
        tmp.ForceMeshUpdate();
        float w = Mathf.Ceil(tmp.preferredWidth) + 8f;
        float h = Mathf.Ceil(tmp.preferredHeight);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);


        Vector2 anchored = WorldToCanvasPoint(worldPos);

        rt.anchoredPosition += new Vector2(0f, 20f);

        StartCoroutine(Life(rt, tmp, lifetime, fadeTime, drift));
    }

    private void OnEnable()
    {
        AuraPointsManager.OnPointsAdded += SpawnFloater;
    }

    private void OnDisable()
    {
        AuraPointsManager.OnPointsAdded -= SpawnFloater;
    }

    private void SpawnFloater(int amount, Vector3 worldPos)
    {
        // Build UI object
        GameObject go = new GameObject("AuraFloater", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(container, false);

        var rt = go.GetComponent<RectTransform>();
        var tmp = go.GetComponent<TextMeshProUGUI>();

        if (font != null) tmp.font = font;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        tmp.text = "+" + amount;

        // Convert world -> local canvas position
        Vector2 anchored = WorldToCanvasPoint(worldPos);
        rt.anchoredPosition = anchored;

        StartCoroutine(Life(rt, tmp, lifetime, fadeTime, drift));
    }

    private Vector2 WorldToCanvasPoint(Vector3 world)
    {
        if (playerCamera == null) return Vector2.zero;
        Vector3 screen = playerCamera.WorldToScreenPoint(world);

        // if behind camera, force onto screen center so floater ALWAYS appears
        if (screen.z < 0f)
            screen = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(container, screen, null, out var local);
        return local;
    }


    private IEnumerator Life(RectTransform rt, TextMeshProUGUI tmp, float life, float fade, Vector2 driftPixels)
    {
        float solid = Mathf.Max(0f, life - fade);
        float t = 0f;

        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos = startPos + driftPixels;

        while (t < solid)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / life);
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, a);
            yield return null;
        }

        float ft = 0f;
        Color start = tmp.color;
        Color end = start; end.a = 0f;

        while (ft < fade)
        {
            ft += Time.deltaTime;
            rt.anchoredPosition = endPos;
            tmp.color = Color.Lerp(start, end, Mathf.Clamp01(ft / fade));
            yield return null;
        }

        Destroy(rt.gameObject);
    }
}
