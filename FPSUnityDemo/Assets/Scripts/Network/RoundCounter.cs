using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoundCounter : MonoBehaviour
{
    [SerializeField]
    private Image[] playerOneWinIcons;
    [SerializeField]
    private Image[] playerTwoWinIcons;
    
    public void UpdateScore(RoundPlayerState player){
        if(player.Score > 0){
            if(player.PlayerNumber == 1){
                playerOneWinIcons[player.Score-1].fillCenter = true;
            } else if(player.PlayerNumber == 2){
                playerTwoWinIcons[player.Score-1].fillCenter = true;
            }
        }
    }
}
