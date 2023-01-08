using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using GamificationPlayer.DTO.ExternalEvents;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace GamificationPlayer.Tests
{
    public class GamificationPlayerManagerTest
    {
        [SetUp]
        public void TestSetup()
        {            
            //Use mock server for testing
            GamificationPlayerManager.UseMockServer();
        }

        [UnityTest]
        public IEnumerator TestGetServerTime()
        {
            var onServerTimeWasCalled = false;
            DateTime serverTime = default;
            GamificationPlayerManager.OnServerTime += (st) => {
                onServerTimeWasCalled = true;
                serverTime = st;
            };

            GamificationPlayerManager.GetServerTime();

            Assert.That(!GamificationPlayerManager.TryGetServerTime(out _));

            yield return new WaitUntil(() => onServerTimeWasCalled);

            Assert.That(GamificationPlayerManager.TryGetServerTime(out DateTime o1ServerTime));

            Assert.That(o1ServerTime.Date == serverTime.Date);

            yield return new WaitForSeconds(2.5f);

            Assert.That(GamificationPlayerManager.TryGetServerTime(out DateTime o2ServerTime));

            Assert.That(o2ServerTime > o1ServerTime);
            Assert.That((o2ServerTime - o1ServerTime).TotalSeconds >= 2f);
            Assert.That((o2ServerTime - o1ServerTime).TotalSeconds <= 3f);
        }

        [Test]
        public void TestOnPageView()
        {
            var obj = new PageViewDTO();

            var userId = System.Guid.NewGuid();
            obj.data.attributes.organisation_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.user_id = userId.ToString();

            obj.data.type = "pageView";

            var json = obj.ToJson();

            var onPageViewWasCalled = false;
            var onEventWasCalled = false;
            GamificationPlayerManager.OnPageView += () => onPageViewWasCalled = true;
            GamificationPlayerManager.OnEvent += (_) => onEventWasCalled = true;

            GamificationPlayerManager.ProcessExternalMessage(json);

            Assert.IsTrue(onPageViewWasCalled);
            Assert.IsTrue(onEventWasCalled);

            Assert.IsTrue(GamificationPlayerManager.TryGetActiveUserId(out _));
            if(GamificationPlayerManager.TryGetActiveUserId(out var id))
            {
                Assert.AreEqual(id, userId);
            }
        }

        [Test]
        public void TestOnMicroGameOpenend()
        {
            var obj = new MicroGameOpenedDTO();

            var microGameId = Guid.NewGuid();

            obj.data.type = "microGameOpened";
            obj.data.attributes.identifier = microGameId.ToString();

            var json = obj.ToJson();

            var OnMicroGameOpenedWasCalled = false;
            var onEventWasCalled = false;
            GamificationPlayerManager.OnMicroGameOpened += (id) => OnMicroGameOpenedWasCalled = true;
            GamificationPlayerManager.OnEvent += (_) => onEventWasCalled = true;

            GamificationPlayerManager.ProcessExternalMessage(json);

            Assert.IsTrue(OnMicroGameOpenedWasCalled);
            Assert.IsTrue(onEventWasCalled);

            Assert.IsTrue(GamificationPlayerManager.TryGetLatestMicroGameIdentifier(out _));
            if(GamificationPlayerManager.TryGetLatestMicroGameIdentifier(out var id))
            {
                Assert.AreEqual(obj.data.attributes.identifier, id);
            }
        }

        [Test]
        public void TestOnFitnessContentOpenend()
        {
            var obj = new FitnessContentOpenedDTO();

            var fitnessContentId = Guid.NewGuid();

            obj.data.type = "fitnessContentOpened";
            obj.data.attributes.identifier = fitnessContentId.ToString();

            var json = obj.ToJson();

            var OnFitnessContentOpenedWasCalled = false;
            var onEventWasCalled = false;
            GamificationPlayerManager.OnFitnessContentOpened += (id) => OnFitnessContentOpenedWasCalled = true;
            GamificationPlayerManager.OnEvent += (_) => onEventWasCalled = true;

            GamificationPlayerManager.ProcessExternalMessage(json);

            Assert.IsTrue(OnFitnessContentOpenedWasCalled);
            Assert.IsTrue(onEventWasCalled);

            Assert.IsTrue(GamificationPlayerManager.TryGetLatestFitnessContentIdentifier(out _));
            if(GamificationPlayerManager.TryGetLatestFitnessContentIdentifier(out var id))
            {
                Assert.AreEqual(obj.data.attributes.identifier, id);
            }
        }

        [Test]
        public void TestOnModuleSessionStarted()
        {
            var obj = new ModuleSessionStartedDTO();

            var moduleId = System.Guid.NewGuid();
            obj.data.attributes.organisation_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.user_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.campaign_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.challenge_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.challenge_session_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.module_id = moduleId.ToString();
            obj.data.attributes.module_session_id = System.Guid.NewGuid().ToString();

            obj.data.type = "moduleSessionStarted";

            var json = obj.ToJson();

            var onModuleStartWasCalled = false;
            var onEventWasCalled = false;
            GamificationPlayerManager.OnModuleStart += (_) => onModuleStartWasCalled = true;
            GamificationPlayerManager.OnEvent += (_) => onEventWasCalled = true;

            GamificationPlayerManager.ProcessExternalMessage(json);

            Assert.IsTrue(onModuleStartWasCalled);
            Assert.IsTrue(onEventWasCalled);

            Assert.IsTrue(GamificationPlayerManager.IsModuleSessionActive());

            Assert.IsTrue(GamificationPlayerManager.TryGetActiveModuleId(out _));
            if(GamificationPlayerManager.TryGetActiveModuleId(out var id))
            {
                Assert.AreEqual(id, moduleId);
            }
        }

        [UnityTest]
        public IEnumerator TestStartDeviceFlow()
        {
            var onLoginTokenWasCalled = false;
            var redirectURL = string.Empty;
            GamificationPlayerManager.OnUserLoggedIn += (url) => 
            {
                onLoginTokenWasCalled = true;
                redirectURL = url;
            };
            
            var link = string.Empty;
            GamificationPlayerManager.StartDeviceFlow((l) =>
            {
                link = l;
            });

            if(!onLoginTokenWasCalled)
                yield return new WaitForSeconds(4f);

            if(!onLoginTokenWasCalled)
                yield return new WaitForSeconds(1f);

            if(!onLoginTokenWasCalled)
                yield return new WaitForSeconds(2f);

            Assert.IsTrue(onLoginTokenWasCalled);
            
            Assert.That(!string.IsNullOrEmpty(link));
            Assert.That(!string.IsNullOrEmpty(redirectURL));
        }

        [UnityTest]
        public IEnumerator TestStopDeviceFlow()
        {
            var onLoginTokenWasCalled = false;
            GamificationPlayerManager.OnUserLoggedIn += (_) => onLoginTokenWasCalled = true;
            
            var link = string.Empty;
            GamificationPlayerManager.StartDeviceFlow((l) =>
            {
                link = l;
            });

            if(!onLoginTokenWasCalled)
                yield return new WaitForSeconds(3f);

            GamificationPlayerManager.StopDeviceFlow();

            if(!onLoginTokenWasCalled)
                yield return new WaitForSeconds(2f);

            if(!onLoginTokenWasCalled)
                yield return new WaitForSeconds(2f);

            Assert.IsFalse(onLoginTokenWasCalled);
        }

        [UnityTest]
        public IEnumerator EndLatestModuleSession()
        {
            var obj = new ModuleSessionStartedDTO();

            obj.data.attributes.organisation_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.user_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.campaign_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.challenge_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.challenge_session_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.module_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.module_session_id = System.Guid.NewGuid().ToString();

            obj.data.type = "moduleSessionStarted";

            var json = obj.ToJson();

            GamificationPlayerManager.ProcessExternalMessage(json);

            Assert.IsTrue(GamificationPlayerManager.IsModuleSessionActive());
            
            var isDone = false;
            GamificationPlayerManager.EndLatestModuleSession(777, true, () =>
            {
                isDone = true;
            });

            yield return new WaitUntil(() => isDone);

            Assert.IsFalse(GamificationPlayerManager.IsModuleSessionActive());
        }

        [UnityTest]
        public IEnumerator EndLatestModuleSessionAndStartNewOne()
        {
            var obj = new ModuleSessionStartedDTO();

            obj.data.attributes.organisation_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.user_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.campaign_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.challenge_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.challenge_session_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.module_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.module_session_id = System.Guid.NewGuid().ToString();

            obj.data.type = "moduleSessionStarted";

            var json = obj.ToJson();

            GamificationPlayerManager.ProcessExternalMessage(json);

            Assert.IsTrue(GamificationPlayerManager.IsModuleSessionActive());
            
            var isDone = false;
            GamificationPlayerManager.EndLatestModuleSession(777, true, () =>
            {
                isDone = true;
            });

            yield return new WaitUntil(() => isDone);

            Assert.IsFalse(GamificationPlayerManager.IsModuleSessionActive());

            obj = new ModuleSessionStartedDTO();

            var moduleId = System.Guid.NewGuid();
            obj.data.attributes.organisation_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.user_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.campaign_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.challenge_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.challenge_session_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.module_id = moduleId.ToString();
            obj.data.attributes.module_session_id = System.Guid.NewGuid().ToString();

            obj.data.type = "moduleSessionStarted";

            json = obj.ToJson();

            var onModuleStartWasCalled = false;
            var onEventWasCalled = false;
            GamificationPlayerManager.OnModuleStart += (_) => onModuleStartWasCalled = true;
            GamificationPlayerManager.OnEvent += (_) => onEventWasCalled = true;

            GamificationPlayerManager.ProcessExternalMessage(json);

            Assert.IsTrue(onModuleStartWasCalled);
            Assert.IsTrue(onEventWasCalled);

            Assert.IsTrue(GamificationPlayerManager.IsModuleSessionActive());

            Assert.IsTrue(GamificationPlayerManager.TryGetActiveModuleId(out _));
            if(GamificationPlayerManager.TryGetActiveModuleId(out var id))
            {
                Assert.AreEqual(id, moduleId);
            }
        }
    }
}
