using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>World-space VR briefing shared by the introduction and encounter warnings.</summary>
[DisallowMultipleComponent]
public class LevelBriefingUI : MonoBehaviour
{
    [SerializeField] private AutomaticMovementVR movement;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text countdownText;
    [Tooltip("Objetos decorativos del briefing completo que se ocultan en modo advertencia compacta.")]
    [SerializeField] private GameObject[] fullBriefingOnlyObjects;
    [SerializeField] private bool showIntroduction = true;
    [SerializeField] private bool showFirstAmbushPanel = true;
    [Min(1f)] [SerializeField] private float introductionDuration = 14f;
    [TextArea(3, 8)] [SerializeField] private string introductionMessage =
        "Apunta a los enemigos y pulsa el gatillo para disparar.\n\n" +
        "Para RECARGAR, apunta el arma hacia abajo un momento y vuelve a levantarla.\n\n" +
        "El avance es automático. En las paradas, elimina a todos los enemigos para continuar.";

    private Object messageOwner;
    private float expiresAt;
    private bool showingIntroduction;
    private bool hasShownFirstAmbush;
    private bool compactWarning;
    private float messageStartedAt;
    private RectTransform panelRect;
    private Vector2 fullPanelSize;
    private Vector2 fullTitlePosition;
    private Vector2 fullCountdownPosition;
    private float fullTitleAlpha;

    [Header("Advertencias posteriores")]
    [SerializeField] private Vector3 warningViewOffset = new Vector3(0f, 0.85f, 2.4f);
    [Range(0.25f, 2f)] [SerializeField] private float blinkFrequency = 1f;

    private void Awake()
    {
        panelRect = panel != null ? panel.GetComponent<RectTransform>() : null;
        if (panelRect != null) fullPanelSize = panelRect.sizeDelta;
        if (titleText != null)
        {
            fullTitlePosition = titleText.rectTransform.anchoredPosition;
            fullTitleAlpha = titleText.alpha;
        }
        if (countdownText != null) fullCountdownPosition = countdownText.rectTransform.anchoredPosition;
        if (panel != null) panel.SetActive(false);
        if (showIntroduction && movement != null) movement.PauseMovement(this);
    }

    private IEnumerator Start()
    {
        if (!showIntroduction) yield break;
        // Allow XR tracking to supply the initial headset pose.
        yield return null;
        showingIntroduction = true;
        ShowMessage(this, "PREPÁRATE PARA EL COMBATE", introductionMessage, introductionDuration);
        yield return new WaitForSeconds(introductionDuration);
        HideMessage(this);
        showingIntroduction = false;
        if (movement != null) movement.ResumeMovement(this);
    }

    public void ShowMessage(Object owner, string title, string message, float duration)
    {
        if (!isActiveAndEnabled || panel == null || playerCamera == null) return;
        SetCompactWarning(false);
        messageOwner = owner;
        messageStartedAt = Time.time;
        expiresAt = Time.time + duration;
        titleText.text = title;
        bodyText.text = message;
        PositionPanel();
        panel.SetActive(true);
        UpdateCountdown();
    }

    public void ShowAmbushWarning(Object owner, string title, string message, float duration)
    {
        if (!isActiveAndEnabled || panel == null || playerCamera == null) return;
        ShowMessage(owner, title, message, duration);
        SetCompactWarning(!showFirstAmbushPanel || hasShownFirstAmbush);
        hasShownFirstAmbush = true;
        PositionPanel();
    }

    private void SetCompactWarning(bool compact)
    {
        compactWarning = compact;
        if (panelRect != null)
            panelRect.sizeDelta = compact ? new Vector2(fullPanelSize.x, 110f) : fullPanelSize;
        if (bodyText != null) bodyText.gameObject.SetActive(!compact);
        if (titleText != null)
        {
            titleText.rectTransform.anchoredPosition = compact ? Vector2.zero : fullTitlePosition;
            titleText.alpha = fullTitleAlpha;
        }
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(!compact);
            countdownText.rectTransform.anchoredPosition = fullCountdownPosition;
        }
        if (fullBriefingOnlyObjects != null)
        {
            for (int i = 0; i < fullBriefingOnlyObjects.Length; i++)
            {
                if (fullBriefingOnlyObjects[i] != null)
                    fullBriefingOnlyObjects[i].SetActive(!compact);
            }
        }
    }

    public void HideMessage(Object owner)
    {
        if (messageOwner != owner) return;
        messageOwner = null;
        if (panel != null) panel.SetActive(false);
    }

    private void LateUpdate()
    {
        if (panel == null || !panel.activeSelf) return;
        // Follow position without attaching the panel's tilt to the headset.
        PositionPanel();
        UpdateCountdown();
        if (compactWarning && titleText != null)
        {
            // A slow pulse keeps the warning readable without fully hiding it.
            float pulse = 0.5f + 0.5f * Mathf.Cos((Time.time - messageStartedAt) * blinkFrequency * 2f * Mathf.PI);
            titleText.alpha = fullTitleAlpha * Mathf.Lerp(0.55f, 1f, pulse);
        }
        if (Time.time >= expiresAt) HideMessage(messageOwner);
    }

    private void PositionPanel()
    {
        if (compactWarning)
        {
            // World-space canvas held above the centre of the headset's view, including pitch.
            panel.transform.SetPositionAndRotation(
                playerCamera.position + playerCamera.rotation * warningViewOffset,
                playerCamera.rotation);
            return;
        }
        Vector3 forward = Vector3.ProjectOnPlane(playerCamera.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.01f) forward = movement != null ? movement.transform.forward : Vector3.forward;
        forward.Normalize();
        panel.transform.SetPositionAndRotation(playerCamera.position + forward * 2.4f,
            Quaternion.LookRotation(forward, Vector3.up));
    }

    private void UpdateCountdown()
    {
        if (countdownText == null || compactWarning) return;
        int remaining = Mathf.Max(0, Mathf.CeilToInt(expiresAt - Time.time));
        countdownText.text = showingIntroduction && messageOwner == this
            ? $"El recorrido comienza en {remaining} s"
            : $"Enemigos en {remaining} s · Mira a tu alrededor";
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        messageOwner = null;
        if (panel != null) panel.SetActive(false);
        if (movement != null) movement.ResumeMovement(this);
    }
}
