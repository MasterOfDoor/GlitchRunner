using System.Collections;
using UnityEngine;
using TMPro;

/// Sadece TMP Text objesine ekle.
/// Trigger'a değince glitch + fade ile yok olur.
public class AccessPanel : MonoBehaviour
{
    [Header("Referans")]
    [SerializeField] private TMP_Text label;

    [Header("Renkler")]
    [SerializeField] private Color colorA = new Color(1f, 0.06f, 0.25f);  // #FF0F40
    [SerializeField] private Color colorB = new Color(0f, 1f,   0.53f);   // #00FF88

    [Header("Efekt")]
    [SerializeField] private float glitchDuration = 0.5f;
    [SerializeField] private float fadeDuration   = 0.35f;

    static readonly string[] GlitchPool =
    {
        "ERİŞİM\nREDDEDİLDİ",
        "3Rİ$İM\nR3DD3D|LD|",
        "ER|Ş|M\n▓EDD▓D|LD|",
        "█RİŞ█M\n█EDDEDILDI",
        "E̷R̷İ̷Ş̷İ̷M̷\nR̷E̷D̷D̷E̷D̷İ̷L̷D̷İ̷",
        "҉ERİŞİM҉\nREDDEDİLDİ",
    };

    bool _done;
    float _t;

    void Awake()
    {
        if (label == null) label = GetComponent<TMP_Text>();
        if (label == null) label = GetComponentInChildren<TMP_Text>();
        if (label == null)
        {
            Debug.LogError("[AccessPanel] TMP_Text bulunamadı! Label alanını Inspector'dan bağla.");
            return;
        }
        label.text  = " // access\ndenied";
        label.color = colorA;
    }

    // sürekli nabız
    void Update()
    {
        if (_done || label == null) return;
        _t += Time.deltaTime * 2.5f;
        float b = 0.55f + 0.45f * Mathf.Sin(_t);
        label.color = Color.Lerp(colorA * b, colorA, 0.3f);
    }

    public void Dismiss()
    {
        if (_done) return;
        _done = true;
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        // 1 — glitch
        float t = 0f;
        while (t < glitchDuration)
        {
            label.text  = GlitchPool[Random.Range(0, GlitchPool.Length)];
            label.color = Random.value > 0.5f ? colorA : colorB;

            // yatay kayma
            label.transform.localPosition = new Vector3(
                Random.Range(-6f, 6f), Random.Range(-2f, 2f), 0f);

            t += Time.deltaTime;
            yield return new WaitForSeconds(0.035f);
        }

        label.transform.localPosition = Vector3.zero;

        // 2 — fade out
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            Color c = label.color;
            c.a = 1f - t;
            label.color = c;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}