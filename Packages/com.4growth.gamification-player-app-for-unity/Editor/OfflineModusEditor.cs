using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace GamificationPlayer.Editor
{
    [ExecuteInEditMode]
    public class EditorCoroutineRunner : MonoBehaviour
    {
        public void GetLoginToken(IEnumerator enumerator)
        {
            StartCoroutine(enumerator);
        }
    }

    public class OfflineModusEditor : EditorWindow
    {
        private EnvironmentConfig environmentConfig;
        private EditorCoroutineRunner coroutineHelper;

        private string organisationId = "";
        private string userId = "";
        private string loginToken = "";
        private string subdomain = "";

        [MenuItem("Gamification Player/Offline Modus")]
        public static void ShowWindow()
        {
            GetWindow<OfflineModusEditor>("Offline Modus");
        }

        private string GetPackageRootPath()
        {
            // Replace 'YourScriptClass' with the actual class name of your editor script
            MonoScript monoScript = MonoScript.FromScriptableObject(CreateInstance<OfflineModusEditor>());
            string scriptPath = AssetDatabase.GetAssetPath(monoScript);
            // Trimming the path to get the root folder of the package
            string packageRootPath = System.IO.Path.GetDirectoryName(scriptPath);
            // Additional trimming may be required based on your folder structure

            // remove folder Editor from path
            packageRootPath = packageRootPath.Replace("Editor", "");

            packageRootPath.Replace("\\", "/");

            return packageRootPath;
        }

        private void OnGUI()
        {
            var sessionData = new SessionLogData();
            environmentConfig = (EnvironmentConfig)EditorGUILayout.ObjectField("Environment Config", environmentConfig, typeof(EnvironmentConfig), false);

            GUILayout.Label("Organisation ID:", EditorStyles.boldLabel);
            organisationId = GUILayout.TextField(organisationId);

            GUILayout.Label("User ID:", EditorStyles.boldLabel);
            userId = GUILayout.TextField(userId);

            if (GUILayout.Button("Get login token"))
            {
                var gamificationPlayerEndpoints = new GamificationPlayerEndpoints(environmentConfig, sessionData);

                if (FindObjectOfType<EditorCoroutineRunner>() == null)
                {
                    coroutineHelper = new GameObject("Editor Coroutine Runner").AddComponent<EditorCoroutineRunner>();
                } else
                {
                    coroutineHelper = FindObjectOfType<EditorCoroutineRunner>();
                }

                coroutineHelper.GetLoginToken(gamificationPlayerEndpoints.CoGetLoginToken(Guid.Parse(organisationId), Guid.Parse(userId), (result, loginToken) =>
                    {
                        Debug.Log("Result: " + result);
                        this.loginToken = loginToken;
                        Debug.Log("Login token: " + loginToken);

                        DestroyImmediate(coroutineHelper.gameObject);
                    }
                ));
            }

            GUILayout.Label("Login token:", EditorStyles.boldLabel);
            loginToken = GUILayout.TextField(loginToken);

            GUILayout.Label("Subdomain:", EditorStyles.boldLabel);
            subdomain = GUILayout.TextField(subdomain);

            if (GUILayout.Button("Download Gamification Player App for Offline Modus"))
            {
                var url = string.Format("https://{0}.{1}login?otlToken={2}", subdomain, environmentConfig.Webpage, loginToken);
                RunShellScript(url);
            }
        }

        // Method to execute the shell script
        public void RunShellScript(string url = "https://csm.learnstrike.app/anonymous")
        {
            var arguments = $"--mirror --directory-prefix=Assets/StreamingAssets --exclude-directories=profile --content-disposition --page-requisites --convert-links --adjust-extension --compression=auto --reject-regex \"/search|/rss\" --no-if-modified-since --no-check-certificate --user-agent=\"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36 learnstrike-mobile-app\" {url}";
            var path = GetPackageRootPath();

            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = path + "Tools/wget.exe",
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            var process = System.Diagnostics.Process.Start(processInfo);
            process.WaitForExit();

            string output = process.StandardOutput.ReadToEnd();

            Debug.Log(output);
        }
    }
}
