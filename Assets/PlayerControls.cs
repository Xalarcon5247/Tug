using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayerControls : NetworkBehaviour
{
    // Pull key assigned for each player to control their character, using A for P1 and L for P2
    // This is the only key players need to play
    public KeyCode pullKey; 

    // Variables to customize/change the tug of war gameplay
    public float pullStrength = 5f;
    public float staminaMax = 100f;
    public float staminaRecoveryRate = 10f;
    public float staminaDrainRate = 20f;
    
    // Network variable to keep track and communicate the stamina of each player
    private NetworkVariable<float> currentStamina = new NetworkVariable<float>(100f);
    
    //1 for P1 and 2 for P2
    public int playerId; 
    public bool isPulling = false;

    //Varibales for stamina and audio management 
    private bool outofstam;
    public AudioSource outofstamAudio;

    public Slider staminaBar;

    private void Start()
    {
        outofstamAudio = GetComponent<AudioSource>();
        outofstam = false;

        InitializeStaminaServerRpc();
    }

    private void Update()
    { 
        // Stamina management
        if (Input.GetKey(pullKey) && currentStamina.Value > 0)
        {
            isPulling = true;
            // Request to apply pull force
            ApplyPullServerRpc(pullStrength * Time.deltaTime, playerId);
            //  Drain stamina as the player pulls the rope
            ModifyStaminaServerRpc(-staminaDrainRate * Time.deltaTime);
        }
        else
        {
            // Recover stamina as the player stops pulling the rope
            isPulling = false;
            ModifyStaminaServerRpc(staminaRecoveryRate * Time.deltaTime);
        }

        //Audio management 
        //Play the out of stamina audio if it reaches 0 (<= 1f here, helps with audio spam)
        if (currentStamina.Value <= 1f){
            outofstam = true;
        }
        else {
            outofstam = false;
        }
        if (outofstam){
            if (!outofstamAudio.isPlaying) {
                outofstamAudio.Play();
            }
        }

        // Update stamina bar UI
        staminaBar.value = currentStamina.Value / staminaMax;
    }

    //Rpc to apply pull force
    [ServerRpc(RequireOwnership = false)]
    private void ApplyPullServerRpc(float pullAmount, int playerId)
    {
        Tug.Instance.ApplyPull(pullAmount, playerId);
    }

    //Rpc to modify stamina for each player
    [ServerRpc(RequireOwnership = false)]
    private void ModifyStaminaServerRpc(float amount)
    {
        currentStamina.Value = Mathf.Clamp(currentStamina.Value + amount, 0, staminaMax);
    }

    //Rpc to intialize stamina for each player at start
    [ServerRpc(RequireOwnership = false)]
    private void InitializeStaminaServerRpc()
    {
        currentStamina.Value = staminaMax;
    }

    //Rpc to freeze player controls for each player when one of them wins, called by tug class
    [ClientRpc]
    public void FreezeClientRpc()
    {
        this.enabled = false;
    }
}
