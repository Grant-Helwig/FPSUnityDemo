using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Samples;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RoundSystem : NetworkBehaviour
{
    //server or host is spawning in a client prefab be the client is not spawning in the host prefab 
    [SerializeField]
    private NetworkObject playerPrefab;
    [SerializeField]
    private Transform[] SpawnPoints;

    private List<Transform> remainingSpawnPoints;
    private List<ulong> loadingClients = new List<ulong>(); // init plz 
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
    }

    // public override void OnDestroy()
    // {
        
    // }
}
