using UnityEngine;
using TMPro;

[ExecuteAlways]
public class BillboardManager : MonoBehaviour
{
    [Header("TMP внутри Mask, по порядку змейки")]
    [Tooltip("Например: Right → Top → Left")]
    public TextMeshProUGUI[] billboardTexts;

    [Header("Буферный TMP — тот же порядок, в том же Mask")]
    [Tooltip("Вторая копия строки сразу за основной.")]
    public TextMeshProUGUI[] bufferTexts;

    [Header("Ширина каждого окна (Mask). 0 = взять с родителя TMP")]
    public float[] panelWidths;

    [TextArea(2, 4)]
    public string fullText = "FOOTBALL ARKANOID - SCORE GOALS - DESTROY PLAYERS - COLLECT BONUSES - WIN THE MATCH! ";

    public float scrollSpeed = 100f;
    public string loopSeparator = "   ";

    [Header("Позиция прокрутки")]
    [Range(0f, 100f)]
    [Tooltip("В редакторе: 0–100% всего маршрута. В Play: стартовая позиция.")]
    public float scrollPercent;

    struct Panel
    {
        public float pathStart;
        public float width;
        public float segmentWidth;
        public float primaryX;
        public float bufferX;
        public RectTransform primary;
        public RectTransform buffer;
    }

    Panel[] panels;
    float scrollOffset;
    float segmentLength;
    float scrollRange;
    bool initialized;
    bool playPositionsReady;

    void OnValidate()
    {
        initialized = false;
        playPositionsReady = false;
        if (!TryInitialize())
            return;

        SyncPositionsFromOffset(GetPreviewOffset());
        ApplyPanelPositions();
    }

    void Start()
    {
        if (!TryInitialize())
            return;

        scrollOffset = GetPreviewOffset();
        SyncPositionsFromOffset(scrollOffset);
        ApplyPanelPositions();
        playPositionsReady = true;
    }

    void Update()
    {
        if (!initialized && !TryInitialize())
            return;

        if (Application.isPlaying)
        {
            if (!playPositionsReady)
            {
                SyncPositionsFromOffset(scrollOffset);
                playPositionsReady = true;
            }

            float delta = scrollSpeed * Time.deltaTime;
            scrollOffset += delta;

            for (int i = 0; i < panels.Length; i++)
            {
                if (panels[i].primary == null)
                    continue;

                panels[i].primaryX -= delta;

                if (panels[i].buffer != null)
                {
                    panels[i].bufferX -= delta;
                    RecyclePanel(ref panels[i]);
                }

                ApplyPanelPosition(i);
            }
        }
        else
        {
            SyncPositionsFromOffset(GetPreviewOffset());
            ApplyPanelPositions();
        }
    }

    bool TryInitialize()
    {
        if (billboardTexts == null || billboardTexts.Length == 0)
            return false;

        string segment = fullText + loopSeparator;
        panels = new Panel[billboardTexts.Length];

        float pathOffset = 0f;
        float maxSegmentWidth = 0f;
        bool hasPanel = false;

        for (int i = 0; i < billboardTexts.Length; i++)
        {
            TextMeshProUGUI primary = billboardTexts[i];
            if (primary == null)
                continue;

            float width = GetPanelWidth(primary, i);
            if (width <= 0f)
                continue;

            TextMeshProUGUI buffer = GetBuffer(i);
            SetupText(primary, segment);
            if (buffer != null)
                SetupText(buffer, segment);

            float segmentWidth = MeasureSegmentWidth(primary);

            panels[i] = new Panel
            {
                pathStart = pathOffset,
                width = width,
                segmentWidth = segmentWidth,
                primary = primary.rectTransform,
                buffer = buffer != null ? buffer.rectTransform : null
            };

            pathOffset += width;
            maxSegmentWidth = Mathf.Max(maxSegmentWidth, segmentWidth);
            hasPanel = true;
        }

        if (!hasPanel)
            return false;

        segmentLength = maxSegmentWidth > 0f ? maxSegmentWidth : 1f;
        scrollRange = pathOffset + segmentLength;

        initialized = true;
        return true;
    }

    void SyncPositionsFromOffset(float offset)
    {
        if (!initialized || panels == null)
            return;

        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i].primary == null)
                continue;

            float local = offset - panels[i].pathStart;
            panels[i].primaryX = panels[i].width - local;
            panels[i].bufferX = panels[i].primaryX + panels[i].segmentWidth;
        }
    }

    static void RecyclePanel(ref Panel panel)
    {
        for (int pass = 0; pass < 4; pass++)
        {
            bool moved = false;

            if (panel.primaryX + panel.segmentWidth <= 0f)
            {
                panel.primaryX = panel.bufferX + panel.segmentWidth;
                moved = true;
            }

            if (panel.bufferX + panel.segmentWidth <= 0f)
            {
                panel.bufferX = panel.primaryX + panel.segmentWidth;
                moved = true;
            }

            if (!moved)
                break;
        }
    }

    TextMeshProUGUI GetBuffer(int index)
    {
        if (bufferTexts == null || index >= bufferTexts.Length)
            return null;

        return bufferTexts[index];
    }

    static void SetupText(TextMeshProUGUI tmp, string segment)
    {
        tmp.text = segment;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.horizontalAlignment = HorizontalAlignmentOptions.Left;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;

        var rt = tmp.rectTransform;
        rt.sizeDelta = new Vector2(Mathf.Max(rt.sizeDelta.x, 4096f), rt.sizeDelta.y);
    }

    float GetPreviewOffset()
    {
        if (scrollRange <= 0f)
            return 0f;

        return Mathf.Clamp01(scrollPercent / 100f) * scrollRange;
    }

    float GetPanelWidth(TextMeshProUGUI tmp, int index)
    {
        if (panelWidths != null && index < panelWidths.Length && panelWidths[index] > 0f)
            return panelWidths[index];

        RectTransform mask = tmp.rectTransform.parent as RectTransform;
        if (mask == null)
            return 0f;

        Canvas.ForceUpdateCanvases();
        float width = mask.rect.width;
        return width > 0f ? width : mask.sizeDelta.x;
    }

    void ApplyPanelPositions()
    {
        if (!initialized || panels == null)
            return;

        for (int i = 0; i < panels.Length; i++)
            ApplyPanelPosition(i);
    }

    void ApplyPanelPosition(int index)
    {
        if (panels == null || index >= panels.Length || panels[index].primary == null)
            return;

        SetTextX(panels[index].primary, panels[index].primaryX);

        if (panels[index].buffer != null)
            SetTextX(panels[index].buffer, panels[index].bufferX);
    }

    static float MeasureSegmentWidth(TextMeshProUGUI tmp)
    {
        tmp.ForceMeshUpdate(true);
        Canvas.ForceUpdateCanvases();

        Bounds bounds = tmp.textBounds;
        if (bounds.size.x > 0.001f)
            return bounds.size.x;

        Vector2 rendered = tmp.GetRenderedValues(false);
        if (rendered.x > 0.001f)
            return rendered.x;

        TMP_TextInfo info = tmp.textInfo;
        if (info != null && info.characterCount > 0)
        {
            TMP_CharacterInfo first = info.characterInfo[0];
            TMP_CharacterInfo last = info.characterInfo[info.characterCount - 1];
            float charWidth = last.topRight.x - first.bottomLeft.x;
            if (charWidth > 0.001f)
                return charWidth;
        }

        return tmp.GetPreferredValues(tmp.text).x;
    }

    static void SetTextX(RectTransform textRect, float x)
    {
        Vector2 pos = textRect.anchoredPosition;
        pos.x = x;
        textRect.anchoredPosition = pos;
    }
}
