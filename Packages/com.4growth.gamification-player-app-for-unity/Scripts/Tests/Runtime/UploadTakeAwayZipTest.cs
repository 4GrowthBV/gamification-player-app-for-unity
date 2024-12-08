using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

namespace GamificationPlayer.Tests
{
    public class UploadTakeAwayZipTest
    {
        [SetUp]
        public void TestSetup()
        {
            //Use mock server for testing
            //GamificationPlayerManager.UseMockServer();
        }

        /*
        [UnityTest]
        public IEnumerator TestUploadTakeAwayZip()
        {
            var isMicroGameActive = false;
            var isTakeAwaySaveDone = false;
            UnityWebRequest.Result result = UnityWebRequest.Result.InProgress;

            string jsonString = "{\"data\":{\"type\":\"pageView\",\"attributes\":{\"organisation_id\":\"edb5e165-1c74-44f8-8d57-c24b82f2f5f2\",\"organisation_allow_upgrade_to_registered_user\":false,\"user_id\":\"5b411dd2-20c1-49dd-90a5-555dbaead5f8\",\"user_is_demo\":false,\"user_avatar\":\"https://user-assets.gamificationplayer.eu/avatars/df99e9f6-4f7c-48be-b983-81c2f542b8f0?placeholder=1&organisation=edb5e165-1c74-44f8-8d57-c24b82f2f5f2\",\"user_tags\":[],\"language\":\"nl\",\"user_score\":131439,\"user_score_bonus\":183500,\"user_score_battle\":76538,\"organisation_battle_active\":true,\"user_battle_invitations\":0}}}";

            GamificationPlayerManager.ProcessExternalMessage(jsonString);

            var guid = new Guid("d02c6584-93e5-4b25-a4f1-0bbf04dddd9b");

            GamificationPlayerManager.OnMicroGameOpened += (microGame) =>
            {
                isMicroGameActive = true;
            };

            GamificationPlayerManager.StartMicroGame(guid);

            yield return new WaitUntil(() => isMicroGameActive);

            GamificationPlayerManager.AddTakeAwayResultToActiveSession("test.zip", (r) =>
            {
                result = r;
                isTakeAwaySaveDone = true;
            });

            yield return new WaitUntil(() => isTakeAwaySaveDone);

            Assert.That(result == UnityWebRequest.Result.Success);
        }*/
    }
}
