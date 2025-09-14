using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(InterpolatedMove))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private AnimationCurve moveCurve;
    [SerializeField] private LayerMask Collidables;
    [SerializeField] private Transform orientator; //Transform used to calculate forward direction
    [SerializeField] private float maxCheckBeamDistance = 80;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float moveBufferTime = .8f; // Time window to buffer move inputs

    public bool canGoThroughPortals = false;
    private Queue<(int dirInt , float time)> movesBufferQueue = new Queue<(int, float)>();

    private InputActionModule controls;
    private InterpolatedMove moveScript;
    private Vector3 playerForward;
    private Vector3 gizmosDirection;
    public bool isMoving = false;

    void Start()
    {
        enablePlayerInputs(true);
        
    }

    // Update is called once per frame
    void Update()
    {
        playerForward = GetSnappedForward(orientator.eulerAngles);

    }

    private void FixedUpdate()
    {

    }

    private Vector3 GetSnappedForward(Vector3 rotation)
    {
        if(rotation.y >= 315 || rotation.y < 45)
            return Vector3.forward;
        else if(rotation.y >= 45 && rotation.y < 135)
            return Vector3.right;
        else if(rotation.y >= 135 && rotation.y < 225)
            return Vector3.back;
        else //if(rotation.y >= 225 && rotation.y < 315)
            return Vector3.left;

    }

    public Vector3 FarthestCollidablePosition(Vector3 direction)
    {
        gizmosDirection = direction;
        RaycastHit hit;

        if(Physics.Raycast(transform.position, direction, out hit,maxCheckBeamDistance , Collidables))
        {
            if (hit.collider.gameObject.TryGetComponent<Iinteractable>(out Iinteractable interactable))
            {
                Vector3 interactableResult = interactable.Interact(direction,hit,this.gameObject);
                if (!interactable.executeDefaultCast) return interactableResult;
            }

            Vector3 scale = transform.localScale;
            if (!(scale.x == scale.y && scale.y == scale.z)) { Debug.LogError("not a cube castCalculation will be wrong"); }
            return hit.point - (direction * transform.localScale.x / 2);
        }
        else
        {
            Debug.Log("no hit");
            return transform.position;
        }
    }
    
    public void MovePlayer(Vector3 position)
    {
        float duration = Vector3.Distance(transform.position, position) / moveSpeed;
        moveScript.MoveObject(position, moveCurve, duration);
    }

    private void handleInputs(InputAction.CallbackContext ctx)
    {
        int DirInt = ctx.action.name switch
        {
            "MoveUp" => 0,
            "MoveDown" => 1,
            "MoveRight" => 2,
            "MoveLeft" => 3,
            _ => -1
        };

        //Debug.Log("handleInputs called with DirInt: " + DirInt);

        if (isMoving)
        {
            movesBufferQueue.Enqueue((DirInt,Time.time));
            return;
        }

        Vector3 rotatedForward;
        switch (DirInt)
        {
            case 0:
                MovePlayer(FarthestCollidablePosition(playerForward));
                break;

            case 1:
                MovePlayer(FarthestCollidablePosition(-playerForward));
                break;

            case 2:
                rotatedForward = Quaternion.AngleAxis(90,Vector3.up) * playerForward;
                MovePlayer(FarthestCollidablePosition(rotatedForward));
                break;

            case 3:
                rotatedForward = Quaternion.AngleAxis(-90, Vector3.up) * playerForward;
                MovePlayer(FarthestCollidablePosition(rotatedForward));
                break;

            default:
                Debug.LogError("Invalid Direction Integer");
                break;
           ;
        }
    }

    private void onMovementStart()
    {
        Debug.Log("true :" + Time.time);
        isMoving = true;
    }

    private void onMovementEnd()
    {
        Debug.Log("false" + Time.time);
        isMoving = false;  // No valid buffered moves, set isMoving to false
        while (movesBufferQueue.Count > 0)
        {
            var (dirInt, time) = movesBufferQueue.Dequeue();

            if(Time.time - time < moveBufferTime)
            {   
                Vector3 rotatedForward;
                switch(dirInt)
                {
                    case 0:
                        MovePlayer(FarthestCollidablePosition(playerForward));
                        break;

                    case 1:
                        MovePlayer(FarthestCollidablePosition(-playerForward));
                        break;

                    case 2:
                        rotatedForward = Quaternion.AngleAxis(90, Vector3.up) * playerForward;
                        MovePlayer(FarthestCollidablePosition(rotatedForward));
                        break;

                    case 3:
                        rotatedForward = Quaternion.AngleAxis(-90, Vector3.up) * playerForward;
                        MovePlayer(FarthestCollidablePosition(rotatedForward));
                        break;

                    default:
                        Debug.LogError("Invalid Direction Integer");
                        break;
                        ;

                }
                return;   // Exit after processing one valid buffered move
            }
        }
        
    }

    private void enablePlayerInputs(bool enable = true)
    {
        if (GlobalInputManager.instance == null) return;
        
        if (enable)
        {
            if (controls == null) controls = GlobalInputManager.instance.controls;

            controls.Player.Enable();
            controls.Player.MoveUp.performed    += handleInputs;
            controls.Player.MoveDown.performed  += handleInputs;
            controls.Player.MoveRight.performed += handleInputs;
            controls.Player.MoveLeft.performed  += handleInputs;
        }                                                      
        else
        {
            controls?.Player.Disable();
            controls.Player.MoveUp.performed    -= handleInputs;
            controls.Player.MoveDown.performed  -= handleInputs;
            controls.Player.MoveRight.performed -= handleInputs;
            controls.Player.MoveLeft.performed  -= handleInputs;
        }
            
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, gizmosDirection * maxCheckBeamDistance);
    }

    #region OnEnable & OnDisable Methods

    private void OnEnable()
    {
        enablePlayerInputs(true);
        if (moveScript == null) moveScript = GetComponent<InterpolatedMove>();
        moveScript.dashStartEvent += onMovementStart;
        moveScript.dashFinishEvent += onMovementEnd;

    }
    private void OnDisable()
    {
        enablePlayerInputs(false);
        moveScript.dashStartEvent -= onMovementStart;
        moveScript.dashFinishEvent -= onMovementEnd;
    }
    
    #endregion
}
