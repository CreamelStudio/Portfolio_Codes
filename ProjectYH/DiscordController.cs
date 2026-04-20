using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif
using UnityEngine.SceneManagement;

#if UNITY_EDITOR 
[InitializeOnLoad]
#endif
public class DiscordController : MonoBehaviour
{
    private static bool isDiscordStarting = false;
    private static Discord.Discord discord;
    private static string projectName { get { return Application.productName; } }
    private static string version { get { return Application.unityVersion; } }
    private static RuntimePlatform platform { get { return Application.platform; } }
    private static string activateSceneName { get { return SceneManager.GetActiveScene().name; } }
    private static long lastTimestamp;

    private const string applicationID = "1364859784808890368";

    static DiscordController()
    {
        DelayInit();
    }

    private static async void DelayInit(int delay = 1000)
    {
        await Task.Delay(delay);
        SetupDiscord(); 
    }
    
    #if !UNITY_EDITOR
        private void Update()
        {
            if(isDiscordStarting) discord.RunCallbacks();
        }
    #endif

    private static void SetupDiscord()
    {
        discord = new Discord.Discord(long.Parse(applicationID), (ulong)default);
        lastTimestamp = GetTimestamp();
        #if UNITY_EDITOR
            EditorApplication.update += EditorUpdate;
        #endif
        UpdateActivity();
        isDiscordStarting = true;
    }
    #if UNITY_EDITOR
        private static void EditorUpdate()
        {
            discord.RunCallbacks();
        }
    #endif
    private static void UpdateActivity()
    {
        Discord.Activity activity = new Discord.Activity
        {
            Details = "Project YH 플레이중",
            Timestamps =
            {
                Start = lastTimestamp
            }
        };
        discord.GetActivityManager().UpdateActivity(activity, result =>
        {
            Debug.Log($"Discord Status : {result}");
        });
    }

    private static long GetTimestamp()
    {
        long unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        return unixTimestamp;
    } 
}
