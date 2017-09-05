using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.Input;
using UnityEngine.Serialization;

public class MultiplayerManager : MonoBehaviour
{
    [Serializable]
    public class StartGameEvent : UnityEvent {}

    public PlayerInput playerPrefab;

    [Space]

    public ButtonAction joinAction;
    public ButtonAction leaveAction;

    [Space]

    public List<Transform> spawnPositions = new List<Transform>();

    [Space]

    public StartGameEvent onStartGame;

    class PlayerInfo
    {
        public PlayerHandle playerHandle;
        public bool ready = false;
        public ButtonControl joinControl;
        public ButtonControl leaveControl;

        public PlayerInfo(PlayerHandle playerHandle, ButtonAction joinAction, ButtonAction leaveAction)
        {
            this.playerHandle = playerHandle;
            joinControl = playerHandle.GetActions(joinAction.action.actionMap).GetControl(joinAction.action.actionIndex) as ButtonControl;
            leaveControl = playerHandle.GetActions(leaveAction.action.actionMap).GetControl(leaveAction.action.actionIndex) as ButtonControl;
        }
    }

    PlayerHandle globalHandle;
    List<PlayerInfo> players = new List<PlayerInfo>();

    public void Start()
    {
        // Create a global player handle that listen to all relevant devices not already used
        // by other player handles.
        globalHandle = PlayerHandleManager.GetNewPlayerHandle();
        globalHandle.global = true;
        playerPrefab.SetPlayerHandleMaps(globalHandle);
        joinAction.Bind(globalHandle);
        leaveAction.Bind(globalHandle);
    }

    public void Update()
    {
        // Listen to if the join button was pressed on a yet unassigned device.
        if (joinAction.control.wasJustPressed)
        {
            // These are the devices currently active in the global player handle.
            List<InputDevice> devices = globalHandle.GetActions(joinAction.action.actionMap).GetCurrentlyUsedDevices();

            PlayerHandle handle = PlayerHandleManager.GetNewPlayerHandle();
            for (int i = 0; i < devices.Count; i++)
                handle.AssignDevice(devices[i]);

            playerPrefab.SetPlayerHandleMaps(handle);
            for (int i = 0; i < handle.maps.Count; i++)
            {
                // Activate the ActionMap that is used to join,
                // disregard active state from ActionMapSlots for now (wait until instantiating player).
                var map = handle.maps[i];
                map.active = (map.actionMap == joinAction.action.actionMap);
            }

            players.Add(new PlayerInfo(handle, joinAction, leaveAction));
        }

        int readyCount = 0;
        for (int i = players.Count - 1; i >= 0; i--)
        {
            var player = players[i];
            if (!player.ready)
            {
                if (player.joinControl.wasJustPressed)
                    player.ready = true;
                if (player.leaveControl.wasJustPressed)
                {
                    player.playerHandle.Destroy();
                    players.Remove(player);
                    continue;
                }
            }
            else
            {
                if (player.joinControl.wasJustPressed || player.leaveControl.wasJustPressed)
                    player.ready = false;
            }
            if (player.ready)
                readyCount++;
        }

        if (readyCount >= 1 && (players.Count - readyCount) == 0)
            StartGame();
    }

    public void OnGUI()
    {
        float width = 200;
        float height = 300;
        int playerNum = 0;
        for (int i = 0; i < players.Count; i++)
        {
            PlayerInfo player = players[i];

            Rect rect = new Rect(20 + (width + 20) * playerNum, (Screen.height - height) * 0.5f, width, height);
            GUILayout.BeginArea(rect, "Player", "box");
            GUILayout.Space(20);

            GUILayout.BeginVertical();
            GUILayout.Space(10);
            if (!player.ready)
            {
                GUILayout.Label(string.Format("Press {0} when ready\n\n(Press {1} to leave)",
                        player.joinControl.sourceName,
                        player.leaveControl.sourceName));
            }
            else
            {
                GUILayout.Label(string.Format("READY\n\n(Press {0} to cancel)",
                        player.leaveControl.sourceName));
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();

            playerNum++;
        }
    }

    void StartGame()
    {
        for (int i = 0; i < players.Count; i++)
        {
            PlayerInfo playerInfo = players[i];

            // Activate ActionMaps according to settings in ActionMapSlots.
            for (int j = 0; j < playerPrefab.actionMaps.Count; j++)
            {
                ActionMapSlot actionMapSlot = playerPrefab.actionMaps[j];
                var map = playerInfo.playerHandle.GetActions(actionMapSlot.actionMap);
                map.active = actionMapSlot.active;
            }

            Transform spawnTransform = spawnPositions[i % spawnPositions.Count];
            var player = (PlayerInput)Instantiate(playerPrefab, spawnTransform.position, spawnTransform.rotation);
            player.handle = playerInfo.playerHandle;
        }

        if (onStartGame != null)
            onStartGame.Invoke();

        gameObject.SetActive(false);
    }
}
