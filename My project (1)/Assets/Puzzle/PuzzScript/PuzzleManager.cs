using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PuzzleManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject backgroundDim;
    public GameObject puzzlePanel;
    public RectTransform codeContainer;
    public GameObject codeLinePrefab;
    public TMP_Text timerText;
    public MatrixBitWriter matrixWriter;

    [Header("Target Area")]
    public GameObject targetSlotPrefab;
    public Transform targetArea;

    [Header("Puzzle Data")]
    public List<string> correctOrder = new List<string>();
    private List<CodeLine> currentLines = new List<CodeLine>();

    [Header("Debug / Test")]
    public bool autoStartOnPlay = true;

    [Header("Timer")]
    public float timeLimit = 10f;
    private float timer;
    private bool timerRunning;

    private bool puzzleActive = false;

    void Start()
    {
        ClosePuzzle();

        if (timerText != null)
            timerText.gameObject.SetActive(false);

        if (autoStartOnPlay)
            OpenPuzzle();
    }

    void Update()
    {
        if (!puzzleActive || !timerRunning)
            return;

        timer -= Time.unscaledDeltaTime;
        timerText.text = Mathf.Ceil(timer).ToString();

        if (timer <= 0f)
        {
            timerRunning = false;
            TriggerFail();
            ClosePuzzle();
        }
    }

    // --------------------------------------------------
    // SAFE SPAWN SYSTEM
    // --------------------------------------------------

    Rect GetLocalRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        Vector2 min = codeContainer.InverseTransformPoint(corners[0]);
        Vector2 max = codeContainer.InverseTransformPoint(corners[2]);

        return new Rect(min, max - min);
    }

    bool Overlaps(Rect a, Rect b)
    {
        return a.Overlaps(b);
    }

    Vector2 GetSafeSpawnPosition(
        RectTransform matrixArea,
        RectTransform timerArea,
        List<Rect> occupiedRects,
        Vector2 itemSize,
        int maxAttempts = 60)
    {
        Rect containerRect = codeContainer.rect;
        Rect matrixRect = GetLocalRect(matrixArea);
        Rect timerRect = GetLocalRect(timerArea);

        for (int i = 0; i < maxAttempts; i++)
        {
            float x = Random.Range(containerRect.xMin, containerRect.xMax - itemSize.x);
            float y = Random.Range(containerRect.yMin, containerRect.yMax - itemSize.y);

            Rect candidate = new Rect(new Vector2(x, y), itemSize);

            if (Overlaps(candidate, matrixRect)) continue;
            if (Overlaps(candidate, timerRect)) continue;

            bool overlap = false;
            foreach (Rect r in occupiedRects)
            {
                if (Overlaps(candidate, r))
                {
                    overlap = true;
                    break;
                }
            }

            if (!overlap)
                return candidate.position;
        }

        // Çok nadir fallback
        return Vector2.zero;
    }

    // --------------------------------------------------
    // PUZZLE FLOW
    // --------------------------------------------------

    public void OpenPuzzle()
    {
        puzzleActive = true;
        timerRunning = false;

        Time.timeScale = 0f;
        timerText.gameObject.SetActive(false);

        matrixWriter.OnFinished = StartTimer;

        backgroundDim.SetActive(true);
        puzzlePanel.SetActive(true);

        GeneratePuzzle();
    }

    public void ClosePuzzle()
    {
        puzzleActive = false;
        timerRunning = false;

        Time.timeScale = 1f;

        backgroundDim.SetActive(false);
        puzzlePanel.SetActive(false);
        timerText.gameObject.SetActive(false);

        foreach (Transform child in codeContainer)
            Destroy(child.gameObject);

        foreach (Transform child in targetArea)
            Destroy(child.gameObject);

        currentLines.Clear();
    }

    void StartTimer()
    {
        timer = timeLimit;
        timerRunning = true;
        timerText.gameObject.SetActive(true);
    }

    void GeneratePuzzle()
    {
        List<string> shuffled = new List<string>(correctOrder);
        Shuffle(shuffled);

        for (int i = 0; i < correctOrder.Count; i++)
            Instantiate(targetSlotPrefab, targetArea);

        RectTransform matrixRect = matrixWriter.GetComponent<RectTransform>();
        RectTransform timerRect = timerText.GetComponent<RectTransform>();

        List<Rect> occupiedRects = new List<Rect>();

        foreach (string line in shuffled)
        {
            GameObject obj = Instantiate(codeLinePrefab, codeContainer);
            RectTransform rt = obj.GetComponent<RectTransform>();

            Vector2 size = rt.rect.size;

            Vector2 pos = GetSafeSpawnPosition(
                matrixRect,
                timerRect,
                occupiedRects,
                size
            );

            rt.anchoredPosition = pos;
            occupiedRects.Add(new Rect(pos, size));

            CodeLine code = obj.GetComponent<CodeLine>();
            code.Setup(line, this);
            currentLines.Add(code);
        }
    }

    public void CheckTargetSlots()
    {
        for (int i = 0; i < targetArea.childCount; i++)
            if (targetArea.GetChild(i).childCount == 0)
                return;

        for (int i = 0; i < correctOrder.Count; i++)
        {
            CodeLine line = targetArea.GetChild(i).GetChild(0).GetComponent<CodeLine>();
            if (line.text != correctOrder[i])
                return;
        }

        TriggerSuccess();
    }

    void TriggerSuccess()
    {
        ClosePuzzle();
    }

    void TriggerFail()
    {
        Debug.Log("<color=red>Puzzle başarısız</color>");
    }

    void Shuffle(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}
