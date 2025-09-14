using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [SerializeField] private float sensitivity = 10;
    [SerializeField] private bool moveSelf = false;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Vector2 limitXRotation = new Vector2(-80,80);
    [SerializeField] private bool lockXRotation = false;
    [SerializeField] private bool lockYRotation = false;

    private InputActionModule controls;
    private Vector2 lookVector;
    private Vector2 lookRotation;
    public bool isCursorLocked => Cursor.lockState == CursorLockMode.Locked;

    private void Start()
    {
        targetTransform = moveSelf ? transform : targetTransform;
        lookRotation = new Vector2(targetTransform.eulerAngles.x, targetTransform.eulerAngles.y);

        controls = GlobalInputManager.instance.controls;

        controls.Player.Look.Enable();
        controls.Player.FreeCursor.Enable();

        controls.Player.FreeCursor.performed += ctx => FreeCursor();
        controls.Player.FreeCursor.canceled += ctx => LockCursor();

        LockCursor();
    }

    private void LateUpdate()
    {

        if (controls == null) controls = GlobalInputManager.instance.controls;
        lookVector = controls.Player.Look.ReadValue<Vector2>();

        if(isCursorLocked) lookAround();
    }

    private void lookAround()
    {
        if(!lockYRotation) lookRotation.y += lookVector.x * (sensitivity / 500);
        if(!lockXRotation) lookRotation.x -= lookVector.y * (sensitivity / 500);

        lookRotation.x = Mathf.Clamp(lookRotation.x, limitXRotation.x, limitXRotation.y);

        if (targetTransform != null)
        {
            targetTransform.rotation = Quaternion.Euler(lookRotation.x, lookRotation.y, 0);           
        }
        else if(moveSelf)
        {
            transform.rotation = Quaternion.Euler(lookRotation.x, lookRotation.y, 0);
        }
        else
        {
            Debug.LogError("Target Transform is not assigned in rotateCameraMouse script.");
        }
    }

    private void FreeCursor()
    {
        Debug.Log("Free Cursor Activated");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    #region EnableDisables
    private void OnEnable()
    {
        if (controls != null)
        {
            controls.Player.Look.Enable();
            controls.Player.FreeCursor.Enable();

            controls.Player.FreeCursor.performed += ctx => FreeCursor();
            controls.Player.FreeCursor.canceled += ctx => LockCursor();
        }
    }

    private void OnDisable()
    {
        if (controls != null)
        {
            controls.Player.Look.Disable();
            controls.Player.FreeCursor.Disable();

            controls.Player.FreeCursor.performed -= ctx => FreeCursor();
            controls.Player.FreeCursor.canceled -= ctx => LockCursor();
        }
    }
        #endregion
}
