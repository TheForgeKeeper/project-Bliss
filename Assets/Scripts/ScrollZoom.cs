using UnityEngine;

public class ScrollZoom : MonoBehaviour
{
    [SerializeField] private float zoomSensitivity;
    [SerializeField] private Vector2 zoomRange;
    [SerializeField] private float smoothingSpeed;

    private float scrollInput;
    private float unSmoothedZoom;
    private InputActionModule controls;

    void Start()
    {
        controls = GlobalInputManager.instance.controls;
        unSmoothedZoom = Camera.main.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        scrollInput = controls.Player.Scroll.ReadValue<float>();
        unSmoothedZoom -= scrollInput * zoomSensitivity * Time.deltaTime; 
        unSmoothedZoom = Mathf.Clamp(unSmoothedZoom, zoomRange.x, zoomRange.y);
        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, unSmoothedZoom, smoothingSpeed * Time.deltaTime);

    }
}
