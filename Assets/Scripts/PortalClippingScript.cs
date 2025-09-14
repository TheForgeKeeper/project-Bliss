using UnityEngine;

public interface Iinteractable
{
    Vector3 Interact(Vector3 rayDirection ,RaycastHit hit, GameObject interactor);
    bool executeDefaultCast { get; set; }
}

public class PortalClippingScript : MonoBehaviour, Iinteractable
{
    [SerializeField] private Rigidbody fakePlayerRb; // visual pseudoplayer for clipping effect
    [SerializeField] private Transform pairedPortal;
    [SerializeField] private float forwardingOffset = 1;
   
    public bool executeDefaultCast { get; set; } = false;

    private void OnTriggerEnter(Collider collision)
    {
        GameObject collidedObject = collision.gameObject;
        //Debug.Log("Collision Enter");
        if (collidedObject.CompareTag("Player"))
        {
            // Return if the player is not allowed to go through portals
            if (!collidedObject.GetComponent<PlayerMovement>().canGoThroughPortals) return;

            fakePlayerRb.transform.position = GetPortalMirroredPosition(collidedObject.transform.position);
            fakePlayerRb.gameObject.SetActive(true);

            Material playerMat = collidedObject.GetComponent<MeshRenderer>().material;
            Material fakePlayerMat = fakePlayerRb.GetComponent<MeshRenderer>().material;

            playerMat.SetInt    ("_Clipping", 1);
            playerMat.SetVector ("_PlanePoint", transform.position);
            playerMat.SetVector ("_PlaneNormal", transform.up);

            fakePlayerMat.SetVector ("_PlanePoint", pairedPortal.position);
            fakePlayerMat.SetVector ("_PlaneNormal", pairedPortal.up);
        }
        
    }                              
    private void OnTriggerStay(Collider collision)
    {
        GameObject collidedObject = collision.gameObject;
        if (collidedObject.CompareTag("Player"))
        {
            // Return if the player is not allowed to go through portals
            if (!collidedObject.GetComponent<PlayerMovement>().canGoThroughPortals)
            {
                Debug.LogWarning("GoThruPortal disabled");
            }
            
            // positioning the fake on paired portal
            fakePlayerRb.MovePosition(GetPortalMirroredPosition(collidedObject.transform.position));
        }
    }
    private void OnTriggerExit(Collider collision)
    {
        GameObject collidedObject = collision.gameObject;
        // Debug.Log("Collision Exit");

        if (collidedObject.CompareTag("Player"))
        {
            // Return if the player is not allowed to go through portals
            if (!collidedObject.GetComponent<PlayerMovement>().canGoThroughPortals) return;


            // Disabling the fake player
            fakePlayerRb.gameObject.SetActive(false);

            // swaping the fake with the real player
            collidedObject.SetActive(false);
            collidedObject.transform.position = fakePlayerRb.transform.position;
            collidedObject.SetActive(true);

            // stoping the player's current movement routine
            PlayerMovement playerMovScript = collidedObject.GetComponent<PlayerMovement>();
            playerMovScript.GetComponent<InterpolatedMove>().StopMovement();

            // Disable portal passage ability after exit to avoid loops
            playerMovScript.canGoThroughPortals = false;

            // moving the player after the portal exit
            playerMovScript.MovePlayer(playerMovScript.FarthestCollidablePosition(pairedPortal.up));

            // Turning off clipping shader effect
            Material playerMat = collidedObject.GetComponent<MeshRenderer>().material;
            playerMat.SetInt("_Clipping", 0);

        }
    }

    private Vector3 GetPortalMirroredPosition(Vector3 subject)
    {
        Vector3 localPoint = transform.InverseTransformPoint(subject);
        Vector3 fakePlayerPosLocal = new Vector3(localPoint.x, -localPoint.y, localPoint.z);
        Vector3 fakePlayerPos = pairedPortal.TransformPoint(fakePlayerPosLocal);
        return fakePlayerPos;
    }

    public Vector3 Interact(Vector3 dir,RaycastHit hit, GameObject player)
    {
        if(Vector3.Dot(dir, transform.up) < 0) // Entering the portal
        {
            executeDefaultCast = false;
            player.GetComponent<PlayerMovement>().canGoThroughPortals = true;
            Debug.Log(dir * forwardingOffset);
            return  hit.point + (dir * forwardingOffset);
        }
        executeDefaultCast = true;
        return transform.position;
    }
}
