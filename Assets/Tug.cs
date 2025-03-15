using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class Tug : NetworkBehaviour
{
    public static Tug Instance;

    //Network variables for keeping track of position of players and tug rope
    public NetworkVariable<float> ropePosition = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> P1position = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> P2position = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    //Variables used as thresholds for when a player wins so the game will end
    public float winningThreshold = 5f;
    public float winningPlayerThreshold = 6f;
    public float deathY = 0f;

    //References to players to manipulate their position
    public Transform P1transform;
    public Transform P2transform;
    public Transform ropeTransform;

    //Reference to player 1 and 2 controls to access thier variables
    public PlayerControls P1Controls; 
    public PlayerControls P2Controls; 

    //Audio 
    public AudioSource freeze;
    public AudioSource tugSound;
    private bool hasPlayedTugSound = false;
    private bool gameisrunning;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        }
        freeze = GetComponent<AudioSource>();
        gameisrunning = true;
    }

    private void Update()
    {
        // Always be checking if either player has won
        if (IsServer) 
        {
        CheckWinCondition();
        }

        // Always update check player and rope position 
        UpdatePosition();

        // Audio management
        // Check if both players are pulling
        if (P1Controls.isPulling && P2Controls.isPulling)
        {
            //Play the tug audio if they are both pulling
            if (!hasPlayedTugSound){
                if (!tugSound.isPlaying) 
                {
                    tugSound.Play();
                    hasPlayedTugSound = true;
                }
            }
        }
        else
        {
            //Stop playing the tug audio if the players stop pulling
            if (tugSound.isPlaying) 
            {
                tugSound.Stop();
            }
            hasPlayedTugSound = false;
        }
    }

    //Function to apply a pull to the rope and update positions
    public void ApplyPull(float pullAmount, int playerId)
    {
        if (!IsServer) return;  

        //Apply pulling in the correct direction according to which player is pulling
        //Player 1
        if (playerId == 1)
        {
            ropePosition.Value -= pullAmount;
            P1position.Value -= pullAmount;
            P2position.Value -= pullAmount;
        }
        //Player 2
        else 
        {
            ropePosition.Value += pullAmount;
            P1position.Value += pullAmount;
            P2position.Value += pullAmount;
        }

        UpdatePosition();
        CheckWinCondition();
    }

    //Function to update position of players and rope
    private void UpdatePosition()
    {
        P1transform.position = new Vector3(P1position.Value, P1transform.position.y, P1transform.position.z);
        P2transform.position = new Vector3(P2position.Value, P2transform.position.y, P2transform.position.z);
        ropeTransform.position = new Vector3(ropePosition.Value, ropeTransform.position.y, ropeTransform.position.z);
    }

    // Function to check win condition based on the position of the players
    private void CheckWinCondition()
    {
        //If player1 falls bellow the threshold (reached the edge and is starting to fall)
        if (P1transform.position.y <= winningPlayerThreshold)
        {
            // Freeze all the playercontrols and load the game over scene with P2 as the winner after a delay for audio
            if (gameisrunning)
            {
                gameisrunning = false;
                freeze.Play();
                P1transform.GetComponent<PlayerControls>().FreezeClientRpc();
                P2transform.GetComponent<PlayerControls>().FreezeClientRpc();
            }
            if (P1transform.position.y <= deathY)
            {
                Invoke("EndGameP2", 1f); 
            }
        }
        //If player2 falls bellow the threshold (reached the edge and is starting to fall)
        else if (P2transform.position.y <= winningPlayerThreshold)
        {
            // Freeze all the playercontrols and load the game over scene with P1 as the winner after a delay for audio
            if (gameisrunning)
            {
                gameisrunning = false;
                P1transform.GetComponent<PlayerControls>().FreezeClientRpc();
                P2transform.GetComponent<PlayerControls>().FreezeClientRpc();
            }
            if (P2transform.position.y <= deathY)
            {
                Invoke("EndGameP1", 1f);
            }
        }
    }

    //Functions to load game over scenes
    private void EndGameP1()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("GameOverP1", LoadSceneMode.Single);
    }

    private void EndGameP2()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("GameOverP2", LoadSceneMode.Single);
    }
}
