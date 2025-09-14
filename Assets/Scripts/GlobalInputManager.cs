using UnityEngine;

public interface IPlayerInput
{
    Vector2 movementInput { get; }
    Vector2 lookInput { get; }
}
//NEEDS TO BE ON AN OBJECT IN THE SCENE
public class GlobalInputManager : MonoBehaviour
{
    public InputActionModule controls;
    public static GlobalInputManager instance { get; private set; }

    private void Awake()
    {
        controls = new InputActionModule();

        if (instance != null && instance != this)
        {
            Debug.LogWarning("Multiple InputManager instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // Optional: persists across scenes
        
    }
}

