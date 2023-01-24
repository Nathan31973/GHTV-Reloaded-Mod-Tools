using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Discord;
using System;

public class DiscordController : MonoBehaviour
{
    public static DiscordController instance { get; private set; }
    public Discord.Discord discord;
    private bool isRunningDiscord = false;
    private long DiscordTOKEN = 0000000000000000000;
    private void Awake()
    {

        //check if a instance already exisit
        if (instance != null)
        {
            Debug.LogError("[Discord] DiscordController is alread loaded on " + gameObject.name);
            Destroy(gameObject);
            return;
        }
        instance = this;
        try
        {
            discord = new Discord.Discord(DiscordTOKEN, (System.UInt64)(Discord.CreateFlags.NoRequireDiscord));
        }
        catch (Exception e)
        {
            Debug.LogError("[DiscordController] " + e.Message);
        }
        if (discord == null)
        {
            Debug.LogWarning("[Discord] Discord Isn't running on the client Device");
            isRunningDiscord = false;
            return;
        }
        else
        {
            Debug.LogWarning("[Discord] Discord is running on the client Device");
            isRunningDiscord = true;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (isRunningDiscord)
        {
            discord.RunCallbacks();
        }
    }

    private void OnApplicationQuit()
    {
        if (isRunningDiscord)
        {
            discord.Dispose();
        }
    }
    public void UpdateDiscordActivity(string detail, string state, string largeImage, string largeText)
    {
        //we want to make sure if discord is running so we don't break our game
        if (isRunningDiscord == true)
        {
            //we are grabing the user activity
            var activityManager = discord.GetActivityManager();
            activityManager.ClearActivity((result) =>
            {
                if (result == Discord.Result.Ok)
                {
                    Debug.Log("[Discord] Connected to Client Discord");
                }
                else
                {
                    Debug.Log("[Discord] Another Discord App is connected to the Client Discord");
                }
            });

            //let update the user activity
            var activity = new Discord.Activity
            {
                Details = detail,
                State = state,
                Instance = true,
                Assets =
                {
                    LargeImage = largeImage,
                    LargeText = largeText,
                    SmallImage = "logo",
                    SmallText = "GHTV Reloaded Team",

                }
            };
            activity.Timestamps.Start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            activityManager.UpdateActivity(activity, (res) =>
            {
                if (res == Discord.Result.Ok)
                {
                    Debug.Log("[Discord] Discord status Set!");
                }
                else
                {
                    Debug.LogError("[Discord] Discord status Failed!");
                }
            });
        }
    }
}
