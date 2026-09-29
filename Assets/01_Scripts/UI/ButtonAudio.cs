using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAudio : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, ISubmitHandler
{
    [Header("Sonidos del botón")]
    public AudioClip hoverSound;
    public AudioClip clickSound;
    [Range(0f, 1f)] public float volume = 0.5f;
    private Button button;
    private float lastHover = -1f;

    private void Awake() => button = GetComponent<Button>();

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.IsInteractable() || Time.unscaledTime - lastHover < 0.15f) return;
        lastHover = Time.unscaledTime;
        OneShotAudio.Play(hoverSound, transform.position, volume, spatial: false, gameplay: false);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) PlayClick();
    }

    public void OnSubmit(BaseEventData eventData) => PlayClick();

    private void PlayClick()
    {
        if (button.IsInteractable())
            OneShotAudio.Play(clickSound, transform.position, volume, spatial: false, persist: true, gameplay: false);
    }
}
