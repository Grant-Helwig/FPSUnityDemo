using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct RoundPlayerState : INetworkSerializable, IEquatable<RoundPlayerState>
{
    public ulong ClientId;
    public Byte Score;
    public Byte PlayerNumber;
    //public ulong X;
    //public ulong Y;
    //public ulong Z;
    public bool Alive;
    public RoundPlayerState(ulong clientId, Byte score, Byte playerNumber,  bool alive){ //ulong x,ulong y,ulong z,
        ClientId = clientId;
        Score = score;
        PlayerNumber = playerNumber;
        //X = x;
        //Y = y;
        //Z = z;
        Alive = alive;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref Score);
        serializer.SerializeValue(ref PlayerNumber);
        //serializer.SerializeValue(ref X);
        //serializer.SerializeValue(ref Y);
        //serializer.SerializeValue(ref Z);
        serializer.SerializeValue(ref Alive);
    }

    public bool Equals(RoundPlayerState other){
        return ClientId == other.ClientId &&
            Score == other.Score;
    }
}