using Mirror;
using UnityEngine;

//We will be inheriting from NetworkBehaviour instead of MonoBehaviour 
//since we need the vars and functions that it provides us
public class PlayerController : NetworkBehaviour
{
    //Determines the movement speed modifier for the player, serialized for ease of access in inspector
    [SerializeField] private float _movementSpeed;
    //This holds the input of the player in a Vector3 structure
    private Vector3 _movementInput;
    //Reference to the rigidbody, so we can add force to it
    private Rigidbody _rigidbody;
    //Reference to the renderer on the sphere, so we can modify its color
    private Renderer _renderer;
    //Color of the player, which is changed on this object when the server spawns it.
    //We use SyncVar to synchronize the variable on all clients
    //We also use a hook on the SyncVar to call the SetPlayerColor method when the value changes
    [SyncVar(hook = nameof(SetPlayerColor))] private Color _playerColor = Color.white;

    private void Awake()
    {
        //Getting a reference to the rigidbody in the Sphere child object
        _rigidbody = transform.GetComponentInChildren<Rigidbody>();
        //Getting a reference to the renderer in the Sphere child object
        _renderer = transform.GetComponentInChildren<Renderer>();
    }

    public override void OnStartServer()
    {
        //Runs on the server when it spawns the current instance and sets the color
        _playerColor = Random.ColorHSV(0.5f, 1f, 0.5f, 1f, 0.5f, 1f, 1f, 1f);
    }

    private void Update()
    {
        //Check if this object is owned by the local player, if not, we do an early return
        //since we don't want this logic below to run on all object, just the one the player
        //should control
        if (!isLocalPlayer) return;

        //Getting horizontal and vertical input from the player and storing it into variables
        var hInput = Input.GetAxis("Horizontal");
        var vInput = Input.GetAxis("Vertical");

        //Here constantly set the _movementInput to a Vector3 which holds
        //the horizontal input on X and vertical input on Z.
        //We leave Y on 0 since we don't want the player to move up or down
        _movementInput = new Vector3(hInput, 0, vInput);

        //Check for input, jump and tell server you jumped
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
            CmdJump();
        }
    }

    private void FixedUpdate()
    {
        //We constantly add force to the rigidbody dependant on the input the player provides,
        //multiplied by the movement speed we set and the fixed delta time, to make sure it 
        //is frame independant no matter on how powerful the machine thats running
        //this game is
        _rigidbody.AddForce(_movementInput * _movementSpeed * Time.fixedDeltaTime, ForceMode.VelocityChange);
    }

    //This is the function that we set up in the SyncVar, it will fire off every time
    //the value of _playerColor changes
    private void SetPlayerColor(Color oldColor, Color newColor)
    {
        //Here we set the color of the material of the Sphere to the new color
        _renderer.material.color = newColor;
    }

    //Server side
    [Command]
    private void CmdJump()
    {
        Jump();
    }
    //Both client and serverside
    private void Jump()
    {
        _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
    }
}
