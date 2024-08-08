﻿using BepInEx.Logging;
using Fika.Core.Coop.Components;
using Fika.Core.Coop.Players;
using UnityEngine;

namespace Fika.Dedicated.Classes
{
    public class DedicatedRaidController : MonoBehaviour
    {
        public CoopPlayer MainPlayer { get; set; }
        private CoopPlayer targetPlayer;
        private ManualLogSource logger;
        private float counter;
        private bool paused;

        private void Start()
        {
            counter = 0;
            logger = BepInEx.Logging.Logger.CreateLogSource(nameof(DedicatedRaidController));
            paused = false;
        }

        private void Pause(bool state)
        {
            paused = state;
        }

        private void Update()
        {
            if (paused)
            {
                return;
            }

            counter += Time.deltaTime;

            if (counter > 10)
            {
                counter = 0;

                if (targetPlayer != null)
                {
                    if (!targetPlayer.HealthController.IsAlive)
                    {
                        FindNewPlayer();
                        return;
                    }

                    Vector3 currentPosition = targetPlayer.Position;
                    MainPlayer.Teleport(new(currentPosition.x, currentPosition.y - 50, currentPosition.z));
                    MainPlayer.MovementContext.SetPitchSmoothly(new Vector2(-90, -90));
                }
                else
                {
                    FindNewPlayer();
                }
            }
        }

        public void FindNewPlayer()
        {
            if (CoopHandler.TryGetCoopHandler(out CoopHandler coopHandler))
            {
                foreach (CoopPlayer player in coopHandler.HumanPlayers)
                {
                    if (player.HealthController.IsAlive && !player.IsYourPlayer)
                    {
                        targetPlayer = player;
                        logger.LogInfo("DedicatedRaidController: New player: " + player.Profile.Info.MainProfileNickname);
                        return;
                    }
                }
            }

            targetPlayer = null;
            logger.LogWarning("No more players found as targets");
            Destroy(this);
        }
    }
}
