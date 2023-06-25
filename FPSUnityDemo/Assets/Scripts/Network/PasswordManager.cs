using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System;
using System.Text;

public class PasswordManager : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField password;

    [SerializeField]
    private GameObject passwordPopup;

    [SerializeField]
    private GameObject leaveButton;

    [SerializeField]
    private GameObject MenuCamera;

    private void Start()
    {

        passwordPopup = GetComponent<UltimateClean.PopupOpener>().m_popup;
        //called when server is started
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;

        //called when clients connect
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;

        //called when clients leave
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleCleintDisconnected;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null)
        {
            return;
        }

        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleCleintDisconnected;
    }
    public void host()
    {
        //set the approval check
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;

        //start host
        NetworkManager.Singleton.StartHost();
    }

    public void join()
    {
        //send in the password input as bytes
        NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(password.text);

        //start client
        NetworkManager.Singleton.StartClient();
    }

    public void leave()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.DisconnectClient(NetworkManager.Singleton.LocalClientId);
        }
    }

    private void ApprovalCheck(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback)
    {
        //convert byte to string
        string pass = Encoding.ASCII.GetString(connectionData);

        //verify the password is correct
        bool approve = pass == password.text;

        // create player object, null to use default object, password matche, pos, rot
        callback(true, null, approve, new Vector3(21.7f, .001f, 20.47f), null);
    }

    private void HandleServerStarted()
    {
        //if we are hosting then we need to update UI too
        if (NetworkManager.Singleton.IsHost)
        {
            HandleClientConnected(NetworkManager.Singleton.LocalClientId);
        }
    }

    private void HandleCleintDisconnected(ulong clientID)
    {
        //check if we are the host 
        if (clientID == NetworkManager.Singleton.LocalClientId)
        {
            print("we have left");
            //passwordPopup.SetActive(true);
            leaveButton.SetActive(false);
            MenuCamera.SetActive(true);
        }
    }

    private void HandleClientConnected(ulong clientID)
    {
        //check if we are the host 
        if (clientID == NetworkManager.Singleton.LocalClientId)
        {
            print("we have joined");
            //passwordPopup.SetActive(false);
            passwordPopup = GetComponent<UltimateClean.PopupOpener>().m_popup;
            if (passwordPopup != null)
            {
                passwordPopup.GetComponent<UltimateClean.Popup>().Close();
            }
            else
            {
                print("no popup");
            }
            leaveButton.SetActive(true);
            MenuCamera.SetActive(false);
        }
    }
}
