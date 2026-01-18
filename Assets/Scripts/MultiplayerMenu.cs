using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerMenu : MonoBehaviour
{
    [SerializeField] private Button serverButton;
    [SerializeField] private Button clientButton;

    private void Start()
    {
        serverButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartServer())
            {
                Debug.Log("Server started...");
                HideMenu();
            }
            else
            {
                Debug.LogError("Server failed to start");
            }
        });

        clientButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Client started...");
                HideMenu();
            }
            else
            {
                Debug.LogError("Client failed to start");
            }
        });
    }

    private void HideMenu()
    {
        gameObject.SetActive(false);
    }
}
