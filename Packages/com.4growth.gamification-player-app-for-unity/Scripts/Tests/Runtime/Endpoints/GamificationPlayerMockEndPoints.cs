using System.Collections;
using System;
using UnityEngine.Networking;
using NUnit.Framework;
using System.Text;

namespace GamificationPlayer.Tests
{
    public class GamificationPlayerMockEndPoints
    {
        private class ForceAcceptAll : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData)
            {
                return true;
            }
        }
        
        public static IEnumerator GetMockDTO(string url, Action<string> onDone)
        {
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            webRequest.SetRequestHeader("X-Api-Key", "123");
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.certificateHandler = new ForceAcceptAll();

            yield return webRequest.SendWebRequest();

            Assert.IsTrue(webRequest.result == UnityWebRequest.Result.Success);

            onDone?.Invoke(webRequest.downloadHandler.text);
        }

        public static IEnumerator GetMockDTOWithPost(string url, string data, Action<string> onDone)
        {
            using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(data);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("X-Api-Key", "123");
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.certificateHandler = new ForceAcceptAll();

                yield return webRequest.SendWebRequest();

                Assert.IsTrue(webRequest.result == UnityWebRequest.Result.Success);

                onDone?.Invoke(webRequest.downloadHandler.text);
            }
        }
    }
}
