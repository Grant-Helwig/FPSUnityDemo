using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Samples;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RoundSystem : NetworkBehaviour
{
    //public static RoundSystem Instance => instance;
    //private static RoundSystem instance;
    //server or host is spawning in a client prefab be the client is not spawning in the host prefab 
    [SerializeField]
    private NetworkObject playerPrefab;
    [SerializeField]
    private Transform[] SpawnPoints;

    private List<Transform> remainingSpawnPoints;
    private List<ulong> loadingClients = new List<ulong>(); // init plz 
    private List<NetworkObject> spawnedPlayers = new List<NetworkObject>();

    [SerializeField]
    [Tooltip("Time Remaining until the game starts")]
    public float m_DelayedStartTime = 5.0f;
    [Header("UI Settings")]
    public TextMeshProUGUI gameTimerText;
    public Image gameTimerIcon;
    public TMP_Text scoreText;
    private bool m_ReplicatedTimeSent = false;
    public float m_TimeRemaining;
    private bool m_ClientGameStarted;
    private bool m_ClientStartCountdown;
    public NetworkVariable<bool> hasGameStarted { get; } = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> m_CountdownStarted = new NetworkVariable<bool>(false);
    private Dictionary<ulong, Transform> assignedSpawns = new Dictionary<ulong, Transform>();
    private NetworkList<RoundPlayerState> roundPlayers;
    public int countdownTime;
    [SerializeField]
    private RoundCounter roundCounter;    
    private void Awake() {
        //list of player variables useful for our round system 
        roundPlayers = new NetworkList<RoundPlayerState>();

        if (IsServer)
        {
            hasGameStarted.Value = false;

            //Set our time remaining locally
            m_TimeRemaining = m_DelayedStartTime;

            //Set for server side
            m_ReplicatedTimeSent = false;
        }
        //ServerGameNetPortal.Instance.countdownTime = m_DelayedStartTime;
        // if (instance != null && instance != this)
        // {
        //     Destroy(gameObject);
        //     return;
        // }

        // //otherwise set the instance 
        // instance = this;
        // DontDestroyOnLoad(gameObject);
    }
    public override void OnNetworkSpawn(){
        
        //server needs to grab all active IDs
        if(IsServer){
            foreach(NetworkClient networkClient in NetworkManager.Singleton.ConnectedClientsList){
                loadingClients.Add(networkClient.ClientId);
            }
            remainingSpawnPoints = new List<Transform>(SpawnPoints);

            //DartPool.instance.InitializePool();
            // //DartPool.instance.
            // for(int i = 0; i < 20; i++){
            //     DartPool.instance.GetNetworkObject(DartPool.instance.dart).Spawn();
            // }
            //go = DartPool.instance.GetNetworkObject(projectilePrefabGameObject
        }
        
        
        if(IsClient){
            ClientIsReadyServerRpc();
            m_ClientGameStarted = false;
            roundPlayers.OnListChanged += HandleRoundPlayersStateChanged;
        }

        if(IsClient && !IsServer){
            hasGameStarted.OnValueChanged += (oldValue, newValue) =>
            {
                m_ClientGameStarted = newValue;
                //gameTimerText.gameObject.SetActive(!m_ClientGameStarted);
                Debug.LogFormat("Client side we were notified the game started state was {0}", newValue);
            };
            m_CountdownStarted.OnValueChanged += (oldValue, newValue) =>
            {
                m_ClientStartCountdown = newValue;
                //Debug.LogFormat("Client side we were notified the start count down state was {0}", newValue);
            };
        }
    }

    private void HandleRoundPlayersStateChanged(NetworkListEvent<RoundPlayerState> changeEvent)
    {
        roundCounter.UpdateScore(changeEvent.Value);
    }

    private void Update() {
        UpdateGameTimer();
    }

    private bool HasGameStarted()
    {
        if (IsServer)
            return hasGameStarted.Value;
        return m_ClientGameStarted;
    }

    private void UpdateGameTimer()
    {
        // check to see if everyone is loaded 
        if (!ShouldStartCountDown()) return;
        
        // check to see if the game already started or the timer is over 
        if (!HasGameStarted() && m_TimeRemaining > 0.0f)
        {
            //iterate timer
            m_TimeRemaining -= Time.deltaTime;

            // start the game 
            if (m_TimeRemaining <= 0.0f) // Only the server should be updating this
            {
                if(IsServer){
                    hasGameStarted.Value = true;
                    m_TimeRemaining = 0.0f;
                    OnGameStartedClientRpc();
                }
                
                //ServerGameNetPortal.Instance.countdownTime = m_TimeRemaining;
                
                
            }

            if (m_TimeRemaining > 0.1f)
                //ServerGameNetPortal.Instance.countdownTime = m_TimeRemaining;
                countdownTime = Mathf.FloorToInt(m_TimeRemaining);
            gameTimerText.text = countdownTime.ToString();
            gameTimerIcon.fillAmount = m_TimeRemaining / m_DelayedStartTime;
        }
    }

    [ClientRpc]
    private void OnGameStartedClientRpc()
    {
        if(IsServer){
            NetworkObject p = NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject;
            p.gameObject.gameObject.GetComponent<PlayerInput>().ActivateInput();
        } else {
            NetworkManager.LocalClient.PlayerObject.gameObject.GetComponent<PlayerInput>().ActivateInput();
        }
        
        //print(spawnedPlayers);
        //spawnedPlayers.ForEach(delegate(NetworkObject player) {
        //    player.gameObject.GetComponent<PlayerInput>().actions.Enable();
        //});
        //if(IsClient){
        //    NetworkObject p = NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject;
        //    p.gameObject.gameObject.GetComponent<PlayerInput>().actions.Enable();
        //}
    }

    private bool ShouldStartCountDown()
    {
        //If the game has started, then don't both with the rest of the count down checks.
        if (HasGameStarted()) return false;
        if (IsServer)
        {
            m_CountdownStarted.Value = ServerGameNetPortal.Instance.AllClientsAreLoaded();

            //While we are counting down, continually set the m_ReplicatedTimeRemaining.Value (client should only receive the update once)
            if (m_CountdownStarted.Value && !m_ReplicatedTimeSent)
            {
                SetReplicatedTimeRemainingClientRPC(m_DelayedStartTime);
                m_ReplicatedTimeSent = true;
            }

            return m_CountdownStarted.Value;
        }

        return m_ClientStartCountdown;
    }

    [ClientRpc]
    private void SetReplicatedTimeRemainingClientRPC(float delayedStartTime)
    {
        // See the ShouldStartCountDown method for when the server updates the value
        if (m_TimeRemaining == 0)
        {
            Debug.LogFormat("Client side our first timer update value is {0}", delayedStartTime);
            m_TimeRemaining = delayedStartTime;
        }
        else
        {
            Debug.LogFormat("Client side we got an update for a timer value of {0} when we shouldn't", delayedStartTime);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ClientIsReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        //if you are not a loading client go away
        if(!loadingClients.Contains(serverRpcParams.Receive.SenderClientId)){return;}

        //spawn them
        SpawnPlayer(serverRpcParams.Receive.SenderClientId);

        //remove them from the pending list
        loadingClients.Remove(serverRpcParams.Receive.SenderClientId);

        if(loadingClients.Count != 0){return;}

        Debug.Log("everyone is ready");
    }

    private void SpawnPlayer(ulong senderClientId)
    {
        //grab a random spawn point and remove that spawn point from list
        int spawnPointIndex = Random.Range(0, remainingSpawnPoints.Count);
        Transform spawnPoint = remainingSpawnPoints[spawnPointIndex];
        remainingSpawnPoints.RemoveAt(spawnPointIndex);

        //make a player object for the player 
        NetworkObject playerInstance = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        
        playerInstance.SpawnAsPlayerObject(senderClientId, true);
        //SetInputClientRPC(playerInstance);
        //playerInstance.gameObject.GetComponent<PlayerInput>().actions.Disable();
        
        assignedSpawns.Add(senderClientId, spawnPoint);
        
        //add your data to the lobby player list
        roundPlayers.Add( new RoundPlayerState(
            senderClientId,
            0,
            (Byte)(roundPlayers.Count+1),
            true
        ));
        MonoBehaviour.print(roundPlayers.Count);
    }

    public void playerDeath(){
        MonoBehaviour.print("death");
        
        //player death is called so updated the players round state to indicate that 
        for(int i = 0; i < roundPlayers.Count; i++){
            if(roundPlayers[i].ClientId == NetworkManager.Singleton.LocalClientId){
                roundPlayers[i] = new RoundPlayerState(
                    roundPlayers[i].ClientId,
                    roundPlayers[i].Score,
                    roundPlayers[i].PlayerNumber,
                    false
                );
            }
        }
    
        //now go through all of the players and iterate a count for each alive player
        ulong winner = 0;
        int count = 0;
        for(int i = 0; i < roundPlayers.Count; i++){
            if(roundPlayers[i].Alive){
                count++;
                winner = roundPlayers[i].ClientId;
            }
        }

        //if only player is alive the round is over, display the round update and iterate rounds 
        if(count ==1){
            for(int i = 0; i < roundPlayers.Count; i++){
                if(roundPlayers[i].ClientId == winner){
                        roundPlayers[i] = new RoundPlayerState(
                        roundPlayers[i].ClientId,
                        (Byte)(roundPlayers[i].Score + 1),
                        roundPlayers[i].PlayerNumber,
                        false
                    );
                    if(roundPlayers[i].Score == 2){
                        EndGame();
                    } else { NewRound();}
                }
            }
        }
    }

    //make everyone alive again and move them to their spawns, then restart the timer.
    // NOBODY IS BEING LOCKED AGAIN
    // CLIENT TIMER NOT RESTARTING, SERVER TIMER IS 
    private void NewRound()
    {
        for(int i = 0; i < roundPlayers.Count; i++){   
            roundPlayers[i] = new RoundPlayerState(
                roundPlayers[i].ClientId,
                roundPlayers[i].Score,
                roundPlayers[i].PlayerNumber,
                true
            );
            NewRoundServerRpc(roundPlayers[i].ClientId);
        }
        RestartTimerServerRpc();

    }

    [ServerRpc]
    private void RestartTimerServerRpc()
    {
        hasGameStarted.Value = false;

        //Set our time remaining locally
        m_TimeRemaining = m_DelayedStartTime;

        //Set for server side
        m_ReplicatedTimeSent = false;
    }

    [ClientRpc]
    private void NewRoundClientRpc(ulong clientId, Vector3 spawn, Quaternion dir){
        if(IsServer){
            NetworkObject p = NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject;
            p.gameObject.gameObject.GetComponent<PlayerInput>().DeactivateInput();
            p.gameObject.gameObject.transform.position = spawn;
            p.gameObject.gameObject.transform.rotation = dir;
            p.gameObject.gameObject.GetComponent<Character>().ResetStats();
        } else if(clientId == NetworkManager.LocalClientId){
            NetworkManager.LocalClient.PlayerObject.gameObject.GetComponent<PlayerInput>().DeactivateInput();
            NetworkManager.LocalClient.PlayerObject.gameObject.transform.position = spawn;
            NetworkManager.LocalClient.PlayerObject.gameObject.transform.rotation = dir;
            NetworkManager.LocalClient.PlayerObject.gameObject.GetComponent<Character>().ResetStats();
        }
        m_ClientGameStarted = false;
        m_TimeRemaining = m_DelayedStartTime;
    }

    [ServerRpc]
    private void NewRoundServerRpc(ulong clientId){
        NewRoundClientRpc(clientId, assignedSpawns[clientId].position, assignedSpawns[clientId].rotation);
    }
    //boot everyone back to the lobby somehow
    private void EndGame()
    {
        throw new NotImplementedException();
    }



    // public override void OnDestroy()
    // {

    // }
}
