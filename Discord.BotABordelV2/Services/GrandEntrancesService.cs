﻿using Discord.BotABordelV2.Configuration;
using Discord.BotABordelV2.Interfaces;
using Discord.BotABordelV2.Services.Media;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Options;

namespace Discord.BotABordelV2.Services;

public class GrandEntrancesService : IGrandEntranceService
{
    private const int _MINIMUM_CONNECTED_MEMBERS = 1;

    private const int _SCENARIO_TRIGGER_PERCENT_CHANCE = 50;

    private readonly IMediaService _localMediaService;

    private readonly ILogger<GrandEntrancesService> _logger;

    private readonly List<EntrancesEvent> _entranceEventsOptions;

    public GrandEntrancesService(IOptionsSnapshot<DiscordBot> options,
                            ILogger<GrandEntrancesService> logger,
                            LocalMediaService mediaService)
    {
        _localMediaService = mediaService;
        _logger = logger;
        _entranceEventsOptions = options.Value.EntrancesEvents;
    }

    public async Task TriggerCustomEntranceScenarioAsync(VoiceStateUpdateEventArgs args)
    {
        if (args is null)
            throw new ArgumentNullException(nameof(args));

        if (!ShouldCustomEntrancesScenarioTrigger(args))
            return;

        var eventForUser = _entranceEventsOptions.FirstOrDefault(evnOpt => evnOpt.UserId == args.User.Id);
        if (eventForUser is not null)
        {
            try
            {
                _logger.LogDebug("Playing entrance {entrance}", eventForUser.Name);
                _logger.LogDebug("Playing entrance {path} path", eventForUser.TrackFilePath);
                await _localMediaService.PlayTrackAsync(eventForUser.TrackFilePath, args.After.Channel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while triggering entrance scenario for {entrance}", eventForUser.Name);
            }
        }
    }

    private static bool ShouldCustomEntrancesScenarioTrigger(VoiceStateUpdateEventArgs args)
    {
        if (args.After.Channel is null) // If the user did not connected to a channel
            return false;

        if (args.After.Channel.Users.Count < _MINIMUM_CONNECTED_MEMBERS)
            return false;

        if (Random.Shared.Next(((int)Math.Ceiling(100.0 / _SCENARIO_TRIGGER_PERCENT_CHANCE))) != 1)
            return false;

        return true;
    }
}