using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.Assertions;

//server should just have to get network objects and return network objects now 
//NetworkObject no = NetworkObjectPool.Singleton.GetNetworkObject(projectileInfo.ProjectilePrefab, projectileInfo.ProjectilePrefab.transform.position, projectileInfo.ProjectilePrefab.transform.rotation);
public class DartPool : NetworkBehaviour
{
    [SerializeField]
    public GameObject dart;
    public static DartPool instance;
    private Queue<NetworkObject> pool;
    private bool hasInitialized = false;
    
    //this makes a reference to itself as 'instance'
    private void Awake() {
        if(instance == null){
            instance = this;
        } else {
            Destroy(this);
        }
    }

    //this will make a pool of 20 gameobjects
    public override void OnNetworkSpawn(){
        InitializePool();
    }

    //unregister objects from the cache
    public override void OnNetworkDespawn(){
        ClearPool();
    }

    //function that calls the initialize and does a dupe check 
    public void InitializePool()
    {
        if (hasInitialized) return;
        RegisterPrefabInternal();
        hasInitialized = true;
    }

    //dont think this is needed (from the docs) can remove later
     public void AddPrefab(GameObject prefab)
    {
        var networkObject = prefab.GetComponent<NetworkObject>();

        Assert.IsNotNull(networkObject, $"{nameof(prefab)} must have {nameof(networkObject)} component.");

        RegisterPrefabInternal();
    }

    //process for adding to the pool and then adding a handler
    private void RegisterPrefabInternal()
    {
        pool = new Queue<NetworkObject>();
        for(int i = 0; i < 20; i++){
            GameObject poolItem = Instantiate(dart);
            ReturnNetworkObject(poolItem);
        }
        NetworkManager.Singleton.PrefabHandler.AddHandler(dart, new PooledPrefabInstanceHandler(dart, this));
    }

    /// <summary>
    /// Unregisters all objects in <see cref="PooledPrefabsList"/> from the cache.
    /// </summary>
    public void ClearPool()
    {
        
        NetworkManager.Singleton.PrefabHandler.RemoveHandler(dart);
        pool.Clear();
    }

    //turn off the object and add it back to the queue so it can be used again 
    public void ReturnNetworkObject(GameObject obj){
        obj.SetActive(false);
        pool.Enqueue(obj.GetComponent<NetworkObject>());
    }

    //grabs the pool object by removing it from the queue and turning it on
    public NetworkObject GetNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return GetNetworkObjectInternal(prefab, position, rotation);
    }

    //Also grabs the object but does not need location or rotation 
    public NetworkObject GetNetworkObject(GameObject prefab)
    {
        return GetNetworkObjectInternal(prefab, Vector3.zero, Quaternion.identity);
    }

    //function that actually dequeues / creates new objects and turns them on 
    private NetworkObject GetNetworkObjectInternal(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        NetworkObject networkObject;
        if (pool.Count > 0)
        {
            networkObject = pool.Dequeue();
        }
        else
        {
            networkObject = Instantiate(dart).GetComponent<NetworkObject>();
        }

        // Here we must reverse the logic in ReturnNetworkObject.
        var go = networkObject.gameObject;
        go.SetActive(true);

        go.transform.position = position;
        go.transform.rotation = rotation;

        return networkObject;
    }
}

//this is needed for the prefabhandler instance
class PooledPrefabInstanceHandler : INetworkPrefabInstanceHandler
    {
        //this the 
        GameObject m_Prefab;
        DartPool m_Pool;

        //sets the instance handler vars
        public PooledPrefabInstanceHandler(GameObject prefab, DartPool pool)
        {
            m_Prefab = prefab;
            m_Pool = pool;
        }

        //instantiation will remove objects from the queue
        NetworkObject INetworkPrefabInstanceHandler.Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            var netObject = m_Pool.GetNetworkObject(m_Prefab, position, rotation);
            return netObject;
        }

        //this brings it back to the queue
        void INetworkPrefabInstanceHandler.Destroy(NetworkObject networkObject)
        {
            m_Pool.ReturnNetworkObject(m_Prefab);
        }
    }