using System;
using System.Collections;
using System.Collections.Generic;
using GamificationPlayer;
using GamificationPlayer.DTO.ExternalEvents;
using TMPro;
using UnityEngine;

public class GamificationTestWebGL : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI text;

    public void Awake()
    {
        GamificationPlayerManager.OnMicroGameOpened += GamificationPlayerManager_OnMicroGameOpened;
    }

    private void GamificationPlayerManager_OnMicroGameOpened(MicroGamePayload microGame)
    {
        text.text = microGame.ToJson(true);
    }
}
