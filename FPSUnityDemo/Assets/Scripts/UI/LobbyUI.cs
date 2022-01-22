using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private LobbyPlayerCard[] lobbyPlayerCards;
    [SerializeField] private Button startGameButton;
    [SerializeField] private bool bypassReady = false;

    private NetworkList<LobbyPlayerState> lobbyPlayers;

    private void Awake() {
        lobbyPlayers = new NetworkList<LobbyPlayerState>();
    }
    public override void OnNetworkSpawn(){

        //if we are a client (will only trigger for each player)
        if(IsClient){
            //do this when the list updates
            lobbyPlayers.OnListChanged += HandleLobbyPlayersStateChanged;
        }
        
        // the server (host) will also trigger this 
        if(IsServer){
            startGameButton.gameObject.SetActive(true);

            //run these when a player joins or leaves the game respectively
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        
            //run these for players that are already in the game 
            foreach(NetworkClient client in NetworkManager.Singleton.ConnectedClientsList){
                HandleClientConnected(client.ClientId);
            }
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        //remove from the lobby list
        lobbyPlayers.OnListChanged -= HandleLobbyPlayersStateChanged;

        //when exiting, do clean ups for memory leaks etc
        if(NetworkManager.Singleton){
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        }
    }

    public bool IsEveryoneReady(){
        if(bypassReady){
            return true;
        }
        //we need more players before starting
        if(lobbyPlayers.Count < 2){
            return false;
        }

        //all players must be ready
        foreach(var player in lobbyPlayers){
            if(!player.IsReady){
                return false;
            }
        }

        return true;
    }

    public void OnReadyClicked(){
        ToggleReadyServerRpc();
    }

    public void OnLeaveClicked(){
        GameNetPortal.Instance.RequestDisconnect();
    }

    public void OnStartClicked(){
        StartGameServerRpc();
    }

    //do not need to own the function to call the function, anyone can call
    [ServerRpc(RequireOwnership = false)]
    private void ToggleReadyServerRpc(ServerRpcParams serverRpcParams = default){
        
        for(int i = 0; i < lobbyPlayers.Count; i++){
            if(lobbyPlayers[i].ClientId == serverRpcParams.Receive.SenderClientId){
                lobbyPlayers[i] = new LobbyPlayerState(
                    lobbyPlayers[i].ClientId,
                    lobbyPlayers[i].PlayerName,
                    !lobbyPlayers[i].IsReady
                );
                break;
            }
        }
    }

    //anyone can call this in case we switch to dedicated servers
    [ServerRpc(RequireOwnership = false)]
    private void StartGameServerRpc(ServerRpcParams serverRpcParams = default){
       
        //if the person calling this is not the local player just return
        if(serverRpcParams.Receive.SenderClientId != NetworkManager.Singleton.LocalClientId){
            return;
        }

        if(!IsEveryoneReady()){ return;}

        ServerGameNetPortal.Instance.StartGame();
    }

    private void HandleClientDisconnect(ulong clientId){

        //remove players from lobby list when they leave
        for(int i = 0; i < lobbyPlayers.Count; i++){
            if(lobbyPlayers[i].ClientId == clientId){
                lobbyPlayers.RemoveAt(i);
                break;
            }
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        //pass in the client ID to get the correct player data
        var playerData = ServerGameNetPortal.Instance.GetPlayerData(clientId);

        //basically a null check using a c# nullables
        if(!playerData.HasValue){ return; }

        //add your data to the lobby player list
        lobbyPlayers.Add( new LobbyPlayerState(
            clientId,
            playerData.Value.PlayerName,
            false
        ));
    }

    private void HandleLobbyPlayersStateChanged(NetworkListEvent<LobbyPlayerState> lobbyState)
    {
        for(int i = 0; i < lobbyPlayerCards.Length; i++){
            if(lobbyPlayers.Count > i){
                lobbyPlayerCards[i].UpdateDisplay(lobbyPlayers[i]);
            } else {
                lobbyPlayerCards[i].DisableDisplay();
            }
        }

        if(IsHost){
            startGameButton.interactable = IsEveryoneReady();
        }
    }
}
