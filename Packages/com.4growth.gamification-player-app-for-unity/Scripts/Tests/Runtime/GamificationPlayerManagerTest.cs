using System;
using System.Collections;
using GamificationPlayer.DTO.ExternalEvents;
using NUnit.Framework;
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
        public void TestOnPageViewLogOut()
        {
            var obj = new PageViewDTO();

            var userId = System.Guid.NewGuid();
            obj.data.attributes.organisation_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.user_id = userId.ToString();

            obj.data.type = "pageView";

            var json = obj.ToJson();

            GamificationPlayerManager.ProcessExternalMessage(json);

            obj = new PageViewDTO();

            obj.data.attributes.organisation_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.user_id = null;

            obj.data.type = "pageView";

            json = obj.ToJson();

            GamificationPlayerManager.ProcessExternalMessage(json);

            //Assert.IsFalse(GamificationPlayerManager.IsUserActive());
            if(GamificationPlayerManager.TryGetActiveUserId(out var id))
            {
                Debug.Log(id);
                Assert.AreEqual(id, null);
            }
        }

        [Test]
        public void TestOnMicroGameOpenend()
        {
            var moduleSessionStartedDTO = new ModuleSessionStartedDTO();

            var moduleId = System.Guid.NewGuid();
            moduleSessionStartedDTO.data.attributes.campaign_id = System.Guid.NewGuid().ToString();
            moduleSessionStartedDTO.data.attributes.challenge_id = System.Guid.NewGuid().ToString();
            moduleSessionStartedDTO.data.attributes.challenge_session_id = System.Guid.NewGuid().ToString();
            moduleSessionStartedDTO.data.attributes.module_id = moduleId.ToString();
            moduleSessionStartedDTO.data.attributes.module_session_id = System.Guid.NewGuid().ToString();

            moduleSessionStartedDTO.data.type = "moduleSessionStarted";

            GamificationPlayerManager.ProcessExternalMessage(moduleSessionStartedDTO.ToJson());

            var obj = new MicroGameOpenedDTO();

            var microGameId = Guid.NewGuid();

            obj.data.type = "microGameOpened";
            obj.data.attributes.identifier = microGameId.ToString();
            //obj.data.attributes.module_data = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJwbGF5ZXIiOnsib3JnYW5pc2F0aW9uX2lkIjoiNDBiMDQxOTMtOGNhOC00ODY2LThlMWEtYTgyNmVjMGNhNjUzIiwidXNlcl9pZCI6IjdhNzY0ZjA5LTcxNjctNDc3NC04NWMwLWJkODVjMjM5MTdjMSJ9LCJzZXNzaW9uIjp7ImNoYWxsZW5nZV9zZXNzaW9uX2lkIjoiZTU2MWQ1N2QtNTg5Mi00NWY5LThjYTAtY2Y2MDY3YTNhMTljIiwibW9kdWxlX3Nlc3Npb25faWQiOiJmZmUxZDg2Ny0yMjM5LTQ4NzktODNlOS0zMWM1MzRmMzJmMTUifSwibWljcm9fZ2FtZSI6eyJuYW1lIjoiVGVzdCBNaWNyb0dhbWVzIiwiaWRlbnRpZmllciI6IlBVWjQxTWlzc2lvbiIsInN0YXJzIjp7ImZpdmUiOjkwMDAsImZvdXIiOjcwMDAsInRocmVlIjo1MDAwLCJ0d28iOjQwMDAsIm9uZSI6MH19LCJtb2R1bGUiOnsibXVsdGlwbGllciI6MTAwLCJtYXhfc2NvcmUiOjEwMDAwLCJjdXJyZW50X3Njb3JlIjowLCJjdXJyZW50X2JvbnVzIjowLCJjdXJyZW50X3RvdGFsIjowfX0.Oeimoy7u1Hg25ck3svVLr246Q0WFjshEKAOeHQ2mBgI";
            //obj.data.attributes.module_data = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJwbGF5ZXIiOnsib3JnYW5pc2F0aW9uX2lkIjoiNDBiMDQxOTMtOGNhOC00ODY2LThlMWEtYTgyNmVjMGNhNjUzIiwidXNlcl9pZCI6IjdhNzY0ZjA5LTcxNjctNDc3NC04NWMwLWJkODVjMjM5MTdjMSJ9LCJzZXNzaW9uIjp7ImNoYWxsZW5nZV9zZXNzaW9uX2lkIjoiYTQ3Y2JjMzItYTQyOC00NGVhLTliYjItYWNhNzk3MmRmNDkzIiwibW9kdWxlX3Nlc3Npb25faWQiOiJhNjE3ZGJhMy1lYjA5LTQ0NzUtOWUxOC1iYTRjZTYwNzBiYzkifSwibWljcm9fZ2FtZSI6eyJuYW1lIjoiVGVzdCBNaWNyb0dhbWVzIiwiaWRlbnRpZmllciI6InRlc3QtbWljcm9nYW1lcyIsInN0YXJzIjp7ImZpdmUiOjkwMDAsImZvdXIiOjcwMDAsInRocmVlIjo1MDAwLCJ0d28iOjQwMDAsIm9uZSI6MH19LCJtb2R1bGUiOnsibXVsdGlwbGllciI6MTAwLCJtYXhfc2NvcmUiOjEwMDAwLCJjdXJyZW50X3Njb3JlIjowLCJjdXJyZW50X2JvbnVzIjowLCJjdXJyZW50X3RvdGFsIjowfX0.C4uZkUpqXragKH7-x-SFEud9Pttv9aR_CG_cKEy_gjE";
            obj.data.attributes.module_data = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJwbGF5ZXIiOnsib3JnYW5pc2F0aW9uX2lkIjoiZWRiNWUxNjUtMWM3NC00NGY4LThkNTctYzI0YjgyZjJmNWYyIiwidXNlcl9pZCI6IjViNDExZGQyLTIwYzEtNDlkZC05MGE1LTU1NWRiYWVhZDVmOCIsImxhbmd1YWdlIjoibmwifSwic2Vzc2lvbiI6eyJjaGFsbGVuZ2Vfc2Vzc2lvbl9pZCI6ImU5YzExOGNlLWU5ZjYtNGE5OS04MGUyLTY1MDg1NjI0ODg2OCIsIm1vZHVsZV9zZXNzaW9uX2lkIjoiN2MxZDJhMWEtZmFkOS00YjZlLThkYWYtNTliYWEwYTYzZGNmIn0sIm1pY3JvX2dhbWUiOnsibmFtZSI6IlBVWjUxIEtlbm5pcyBBbGdlbWVuZSB2b29yem9yZ3NtYWF0cmVnZWxlbiIsImlkZW50aWZpZXIiOiJQVVo1MSIsInN0YXJzIjp7ImZpdmUiOjkwMDAsImZvdXIiOjcwMDAsInRocmVlIjo1MDAwLCJ0d28iOjQwMDAsIm9uZSI6MH19LCJtb2R1bGUiOnsibXVsdGlwbGllciI6MTAwLCJtYXhfc2NvcmUiOjEwMDAwLCJjdXJyZW50X3Njb3JlIjowLCJjdXJyZW50X2JvbnVzIjowLCJjdXJyZW50X3RvdGFsIjowfX0.drHJ7fZxYDQwBY1ntPZEV2tEHzOuyp84nmJkdnlhYJA";

            var json = obj.ToJson();

            var OnMicroGameOpenedWasCalled = false;
            var onEventWasCalled = false;
            GamificationPlayerManager.OnMicroGameOpened += (id) => OnMicroGameOpenedWasCalled = true;
            GamificationPlayerManager.OnEvent += (_) => onEventWasCalled = true;

            GamificationPlayerManager.ProcessExternalMessage(json);

            Assert.IsTrue(OnMicroGameOpenedWasCalled);
            Assert.IsTrue(onEventWasCalled);

            Assert.IsTrue(GamificationPlayerManager.TryGetLatestMicroGamePayload(out _));

            Assert.IsTrue(GamificationPlayerManager.IsUserActive());
        }

        [Test]
        public void TestOnFitnessContentOpenend()
        {
            var moduleSessionStartedDTO = new ModuleSessionStartedDTO();

            var moduleId = System.Guid.NewGuid();
            moduleSessionStartedDTO.data.attributes.campaign_id = System.Guid.NewGuid().ToString();
            moduleSessionStartedDTO.data.attributes.challenge_id = System.Guid.NewGuid().ToString();
            moduleSessionStartedDTO.data.attributes.challenge_session_id = System.Guid.NewGuid().ToString();
            moduleSessionStartedDTO.data.attributes.module_id = moduleId.ToString();
            moduleSessionStartedDTO.data.attributes.module_session_id = System.Guid.NewGuid().ToString();

            moduleSessionStartedDTO.data.type = "moduleSessionStarted";

            GamificationPlayerManager.ProcessExternalMessage(moduleSessionStartedDTO.ToJson());

            var obj = new MicroGameOpenedDTO();

            var fitnessContentId = Guid.NewGuid();

            obj.data.type = "fitnessContentOpened";
            obj.data.attributes.identifier = fitnessContentId.ToString();
            obj.data.attributes.module_data = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJwbGF5ZXIiOnsib3JnYW5pc2F0aW9uX2lkIjoiZWRiNWUxNjUtMWM3NC00NGY4LThkNTctYzI0YjgyZjJmNWYyIiwidXNlcl9pZCI6IjViNDExZGQyLTIwYzEtNDlkZC05MGE1LTU1NWRiYWVhZDVmOCIsImxhbmd1YWdlIjoibmwifSwic2Vzc2lvbiI6eyJjaGFsbGVuZ2Vfc2Vzc2lvbl9pZCI6ImU5YzExOGNlLWU5ZjYtNGE5OS04MGUyLTY1MDg1NjI0ODg2OCIsIm1vZHVsZV9zZXNzaW9uX2lkIjoiN2MxZDJhMWEtZmFkOS00YjZlLThkYWYtNTliYWEwYTYzZGNmIn0sIm1pY3JvX2dhbWUiOnsibmFtZSI6IlBVWjUxIEtlbm5pcyBBbGdlbWVuZSB2b29yem9yZ3NtYWF0cmVnZWxlbiIsImlkZW50aWZpZXIiOiJQVVo1MSIsInN0YXJzIjp7ImZpdmUiOjkwMDAsImZvdXIiOjcwMDAsInRocmVlIjo1MDAwLCJ0d28iOjQwMDAsIm9uZSI6MH19LCJtb2R1bGUiOnsibXVsdGlwbGllciI6MTAwLCJtYXhfc2NvcmUiOjEwMDAwLCJjdXJyZW50X3Njb3JlIjowLCJjdXJyZW50X2JvbnVzIjowLCJjdXJyZW50X3RvdGFsIjowfX0.drHJ7fZxYDQwBY1ntPZEV2tEHzOuyp84nmJkdnlhYJA";

            var json = obj.ToJson();

            var OnFitnessContentOpenedWasCalled = false;
            var onEventWasCalled = false;
            GamificationPlayerManager.OnMicroGameOpened += (id) => OnFitnessContentOpenedWasCalled = true;
            GamificationPlayerManager.OnEvent += (_) => onEventWasCalled = true;

            GamificationPlayerManager.ProcessExternalMessage(json);

            Assert.IsTrue(OnFitnessContentOpenedWasCalled);
            Assert.IsTrue(onEventWasCalled);

            Assert.IsTrue(GamificationPlayerManager.TryGetLatestMicroGamePayload(out _));
        }

        [Test]
        public void TestOnModuleSessionStarted()
        {
            var obj = new ModuleSessionStartedDTO();

            var moduleId = System.Guid.NewGuid();
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

            var fitnessContentOpenedDTO = new MicroGameOpenedDTO();
            var fitnessContentId = Guid.NewGuid();
            fitnessContentOpenedDTO.data.type = "fitnessContentOpened";
            fitnessContentOpenedDTO.data.attributes.identifier = fitnessContentId.ToString();
            fitnessContentOpenedDTO.data.attributes.module_data = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJwbGF5ZXIiOnsib3JnYW5pc2F0aW9uX2lkIjoiZWRiNWUxNjUtMWM3NC00NGY4LThkNTctYzI0YjgyZjJmNWYyIiwidXNlcl9pZCI6IjViNDExZGQyLTIwYzEtNDlkZC05MGE1LTU1NWRiYWVhZDVmOCIsImxhbmd1YWdlIjoibmwifSwic2Vzc2lvbiI6eyJjaGFsbGVuZ2Vfc2Vzc2lvbl9pZCI6ImU5YzExOGNlLWU5ZjYtNGE5OS04MGUyLTY1MDg1NjI0ODg2OCIsIm1vZHVsZV9zZXNzaW9uX2lkIjoiN2MxZDJhMWEtZmFkOS00YjZlLThkYWYtNTliYWEwYTYzZGNmIn0sIm1pY3JvX2dhbWUiOnsibmFtZSI6IlBVWjUxIEtlbm5pcyBBbGdlbWVuZSB2b29yem9yZ3NtYWF0cmVnZWxlbiIsImlkZW50aWZpZXIiOiJQVVo1MSIsInN0YXJzIjp7ImZpdmUiOjkwMDAsImZvdXIiOjcwMDAsInRocmVlIjo1MDAwLCJ0d28iOjQwMDAsIm9uZSI6MH19LCJtb2R1bGUiOnsibXVsdGlwbGllciI6MTAwLCJtYXhfc2NvcmUiOjEwMDAwLCJjdXJyZW50X3Njb3JlIjowLCJjdXJyZW50X2JvbnVzIjowLCJjdXJyZW50X3RvdGFsIjowfX0.drHJ7fZxYDQwBY1ntPZEV2tEHzOuyp84nmJkdnlhYJA";
            GamificationPlayerManager.ProcessExternalMessage(fitnessContentOpenedDTO.ToJson());

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

            Assert.IsTrue(GamificationPlayerManager.IsDeviceFlowActive());

            if(!onLoginTokenWasCalled)
            {
                Assert.IsTrue(GamificationPlayerManager.IsDeviceFlowActive());

                yield return new WaitForSeconds(4f);
            }

            if(!onLoginTokenWasCalled)
            {
                Assert.IsTrue(GamificationPlayerManager.IsDeviceFlowActive());
                
                yield return new WaitForSeconds(1f);
            }

            if(!onLoginTokenWasCalled)
            {
                Assert.IsTrue(GamificationPlayerManager.IsDeviceFlowActive());

                yield return new WaitForSeconds(2f);
            }

            Assert.IsFalse(GamificationPlayerManager.IsDeviceFlowActive());

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
            {
                Assert.IsTrue(GamificationPlayerManager.IsDeviceFlowActive());

                yield return new WaitForSeconds(3f);
            }
            
            GamificationPlayerManager.StopDeviceFlow();

            Assert.IsFalse(GamificationPlayerManager.IsDeviceFlowActive());

            if(!onLoginTokenWasCalled)
                yield return new WaitForSeconds(2f);

            if(!onLoginTokenWasCalled)
                yield return new WaitForSeconds(2f);

            Assert.IsFalse(onLoginTokenWasCalled);
        }

        [UnityTest]
        public IEnumerator EndLatestModuleSession()
        {
            var pageViewDTO = new PageViewDTO();

            var userId = System.Guid.NewGuid();
            pageViewDTO.data.attributes.organisation_id = System.Guid.NewGuid().ToString();
            pageViewDTO.data.attributes.user_id = userId.ToString();

            pageViewDTO.data.type = "pageView";

            var json = pageViewDTO.ToJson();

            GamificationPlayerManager.ProcessExternalMessage(json);

            Assert.IsTrue(GamificationPlayerManager.IsUserActive());

            var obj = new ModuleSessionStartedDTO();

            obj.data.attributes.campaign_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.challenge_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.challenge_session_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.module_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.module_session_id = System.Guid.NewGuid().ToString();

            obj.data.type = "moduleSessionStarted";

            json = obj.ToJson();

            GamificationPlayerManager.ProcessExternalMessage(json);

            Assert.IsTrue(GamificationPlayerManager.IsUserActive());

            var fitnessContentOpenedDTO = new MicroGameOpenedDTO();
            var fitnessContentId = Guid.NewGuid();
            fitnessContentOpenedDTO.data.type = "fitnessContentOpened";
            fitnessContentOpenedDTO.data.attributes.identifier = fitnessContentId.ToString();
            fitnessContentOpenedDTO.data.attributes.module_data = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJwbGF5ZXIiOnsib3JnYW5pc2F0aW9uX2lkIjoiZWRiNWUxNjUtMWM3NC00NGY4LThkNTctYzI0YjgyZjJmNWYyIiwidXNlcl9pZCI6IjViNDExZGQyLTIwYzEtNDlkZC05MGE1LTU1NWRiYWVhZDVmOCIsImxhbmd1YWdlIjoibmwifSwic2Vzc2lvbiI6eyJjaGFsbGVuZ2Vfc2Vzc2lvbl9pZCI6ImU5YzExOGNlLWU5ZjYtNGE5OS04MGUyLTY1MDg1NjI0ODg2OCIsIm1vZHVsZV9zZXNzaW9uX2lkIjoiN2MxZDJhMWEtZmFkOS00YjZlLThkYWYtNTliYWEwYTYzZGNmIn0sIm1pY3JvX2dhbWUiOnsibmFtZSI6IlBVWjUxIEtlbm5pcyBBbGdlbWVuZSB2b29yem9yZ3NtYWF0cmVnZWxlbiIsImlkZW50aWZpZXIiOiJQVVo1MSIsInN0YXJzIjp7ImZpdmUiOjkwMDAsImZvdXIiOjcwMDAsInRocmVlIjo1MDAwLCJ0d28iOjQwMDAsIm9uZSI6MH19LCJtb2R1bGUiOnsibXVsdGlwbGllciI6MTAwLCJtYXhfc2NvcmUiOjEwMDAwLCJjdXJyZW50X3Njb3JlIjowLCJjdXJyZW50X2JvbnVzIjowLCJjdXJyZW50X3RvdGFsIjowfX0.drHJ7fZxYDQwBY1ntPZEV2tEHzOuyp84nmJkdnlhYJA";

            GamificationPlayerManager.ProcessExternalMessage(fitnessContentOpenedDTO.ToJson());

            Assert.IsTrue(GamificationPlayerManager.IsModuleSessionActive());

            Assert.IsTrue(GamificationPlayerManager.IsUserActive());
            
            var isDone = false;
            GamificationPlayerManager.EndLatestModuleSession(777, true, () =>
            {
                isDone = true;
            });

            yield return new WaitUntil(() => isDone);

            Assert.IsTrue(GamificationPlayerManager.IsUserActive());

            Assert.IsFalse(GamificationPlayerManager.IsModuleSessionActive());

            GamificationPlayerManager.ProcessExternalMessage(json);
            GamificationPlayerManager.ProcessExternalMessage(fitnessContentOpenedDTO.ToJson());

            Assert.IsFalse(GamificationPlayerManager.IsModuleSessionActive());

            Assert.IsTrue(GamificationPlayerManager.IsUserActive());
        }

        [UnityTest]
        public IEnumerator EndLatestModuleSessionAndStartNewOne()
        {
            var obj = new ModuleSessionStartedDTO();

            obj.data.attributes.campaign_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.challenge_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.challenge_session_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.module_id = System.Guid.NewGuid().ToString();
            obj.data.attributes.module_session_id = System.Guid.NewGuid().ToString();

            obj.data.type = "moduleSessionStarted";

            var json = obj.ToJson();

            GamificationPlayerManager.ProcessExternalMessage(json);

            var fitnessContentOpenedDTO = new MicroGameOpenedDTO();
            var fitnessContentId = Guid.NewGuid();
            fitnessContentOpenedDTO.data.type = "fitnessContentOpened";
            fitnessContentOpenedDTO.data.attributes.identifier = fitnessContentId.ToString();
            fitnessContentOpenedDTO.data.attributes.module_data = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJwbGF5ZXIiOnsib3JnYW5pc2F0aW9uX2lkIjoiZWRiNWUxNjUtMWM3NC00NGY4LThkNTctYzI0YjgyZjJmNWYyIiwidXNlcl9pZCI6IjViNDExZGQyLTIwYzEtNDlkZC05MGE1LTU1NWRiYWVhZDVmOCIsImxhbmd1YWdlIjoibmwifSwic2Vzc2lvbiI6eyJjaGFsbGVuZ2Vfc2Vzc2lvbl9pZCI6ImU5YzExOGNlLWU5ZjYtNGE5OS04MGUyLTY1MDg1NjI0ODg2OCIsIm1vZHVsZV9zZXNzaW9uX2lkIjoiN2MxZDJhMWEtZmFkOS00YjZlLThkYWYtNTliYWEwYTYzZGNmIn0sIm1pY3JvX2dhbWUiOnsibmFtZSI6IlBVWjUxIEtlbm5pcyBBbGdlbWVuZSB2b29yem9yZ3NtYWF0cmVnZWxlbiIsImlkZW50aWZpZXIiOiJQVVo1MSIsInN0YXJzIjp7ImZpdmUiOjkwMDAsImZvdXIiOjcwMDAsInRocmVlIjo1MDAwLCJ0d28iOjQwMDAsIm9uZSI6MH19LCJtb2R1bGUiOnsibXVsdGlwbGllciI6MTAwLCJtYXhfc2NvcmUiOjEwMDAwLCJjdXJyZW50X3Njb3JlIjowLCJjdXJyZW50X2JvbnVzIjowLCJjdXJyZW50X3RvdGFsIjowfX0.drHJ7fZxYDQwBY1ntPZEV2tEHzOuyp84nmJkdnlhYJA";
            GamificationPlayerManager.ProcessExternalMessage(fitnessContentOpenedDTO.ToJson());

            Assert.IsTrue(GamificationPlayerManager.IsModuleSessionActive());
            
            var isDone = false;
            GamificationPlayerManager.EndLatestModuleSession(777, true, () =>
            {
                isDone = true;
            });

            yield return new WaitUntil(() => isDone);

            Assert.IsFalse(GamificationPlayerManager.IsModuleSessionActive());

            GamificationPlayerManager.ProcessExternalMessage(json);
            GamificationPlayerManager.ProcessExternalMessage(fitnessContentOpenedDTO.ToJson());

            Assert.IsFalse(GamificationPlayerManager.IsModuleSessionActive());

            obj = new ModuleSessionStartedDTO();

            var moduleId = System.Guid.NewGuid();
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

            fitnessContentOpenedDTO = new MicroGameOpenedDTO();
            fitnessContentId = Guid.NewGuid();
            fitnessContentOpenedDTO.data.type = "fitnessContentOpened";
            fitnessContentOpenedDTO.data.attributes.identifier = fitnessContentId.ToString();
            fitnessContentOpenedDTO.data.attributes.module_data = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJwbGF5ZXIiOnsib3JnYW5pc2F0aW9uX2lkIjoiZWRiNWUxNjUtMWM3NC00NGY4LThkNTctYzI0YjgyZjJmNWYyIiwidXNlcl9pZCI6IjViNDExZGQyLTIwYzEtNDlkZC05MGE1LTU1NWRiYWVhZDVmOCIsImxhbmd1YWdlIjoibmwifSwic2Vzc2lvbiI6eyJjaGFsbGVuZ2Vfc2Vzc2lvbl9pZCI6ImU5YzExOGNlLWU5ZjYtNGE5OS04MGUyLTY1MDg1NjI0ODg2OCIsIm1vZHVsZV9zZXNzaW9uX2lkIjoiN2MxZDJhMWEtZmFkOS00YjZlLThkYWYtNTliYWEwYTYzZGNmIn0sIm1pY3JvX2dhbWUiOnsibmFtZSI6IlBVWjUxIEtlbm5pcyBBbGdlbWVuZSB2b29yem9yZ3NtYWF0cmVnZWxlbiIsImlkZW50aWZpZXIiOiJQVVo1MSIsInN0YXJzIjp7ImZpdmUiOjkwMDAsImZvdXIiOjcwMDAsInRocmVlIjo1MDAwLCJ0d28iOjQwMDAsIm9uZSI6MH19LCJtb2R1bGUiOnsibXVsdGlwbGllciI6MTAwLCJtYXhfc2NvcmUiOjEwMDAwLCJjdXJyZW50X3Njb3JlIjowLCJjdXJyZW50X2JvbnVzIjowLCJjdXJyZW50X3RvdGFsIjowfX0.drHJ7fZxYDQwBY1ntPZEV2tEHzOuyp84nmJkdnlhYJA";
            GamificationPlayerManager.ProcessExternalMessage(fitnessContentOpenedDTO.ToJson());

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
