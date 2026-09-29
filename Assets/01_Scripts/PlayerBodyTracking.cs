using Unity.XR.CoreUtils;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(XROrigin), typeof(CharacterController))]
[DefaultExecutionOrder(-100)]
public class PlayerBodyTracking : MonoBehaviour
{
    private XROrigin xrOrigin;
    private CharacterController characterController;

    private void Awake()
    {
        xrOrigin = GetComponent<XROrigin>();
        characterController = GetComponent<CharacterController>();
    }

    private void Update() => UpdateBody();
    private void FixedUpdate() => UpdateBody();
    private void LateUpdate() => UpdateBody();

    private void UpdateBody()
    {
        if (xrOrigin.Camera == null || !characterController.enabled) return;

        Vector3 headPosition = transform.InverseTransformPoint(xrOrigin.Camera.transform.position);
        if (headPosition.y <= 0f) return;

        float minimumHeight = Mathf.Max(characterController.radius * 2f, characterController.stepOffset);
        float height = Mathf.Max(headPosition.y, minimumHeight);

        Vector3 center = new Vector3(headPosition.x, height * 0.5f + characterController.skinWidth, headPosition.z);
        if (!Mathf.Approximately(characterController.height, height)) characterController.height = height;
        if (characterController.center != center) characterController.center = center;
    }
}
