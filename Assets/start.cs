using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class start : NetworkBehaviour
{
    public KeyCode startKey = KeyCode.Space;  

    // Start/Restart the game if space is pressed
    void Update()
    {
        if (Input.GetKeyDown(startKey))
        {
            NetworkManager.Singleton.SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
        }
    }
}
