using DiegoG.TelegramBot;
using DiegoG.TelegramBot.Types;
using DiegoG.TelegramBot.Types.ChatSequenceTypes;
using DiegoG.Utilities.Settings;
using DiegoG.WebWatcher;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace MediaWatchers
{
    [BotCommand]
    public class EraiRawsSettingsCommand : IBotCommand
    {
        public TelegramBotCommandClient Processor { get; set; }

        public string HelpExplanation { get; } = "Manipulates the settings for EraiRawsWatcher";

        public string HelpUsage { get; } = "/set_erai (option) (value)";

        public IEnumerable<OptionDescription> HelpOptions { get; } = new OptionDescription[]
        {
            new("option", "The name of the setting"),
            new("value", "The value given to the setting"),
            new("settings", "ChatId, `long` | MatchPatterns, `\"string\" \"string\" ...`")
        };

        public string Trigger { get; } = "/set_erai";

        public string Alias { get; } = "/ser";

        public async Task<CommandResponse> Action(BotCommandArguments args)
        {
            var m = args.Message;
            var userright = OutBot.GetAdminLevel(args.User);

            if (userright is < AdminRights.Admin)
            {
                Log.Debug($"User {args.User} attempted to change Erai Raws Watcher settings, which is beyond their right of {userright}");
                return new(m, false, "You don't have the right to do that");
            }

            try
            {
                if (args.Arguments.Length is < 3)
                    return new CommandResponse(args.Message, false, "Not enough arguments");

                var op = args.Arguments[1].ToLower();
                if(op is "chatid")
                {
                    if(long.TryParse(args.Arguments[2], out var result))
                    {
                        Log.Debug($"Changing EraiRaws output chat to {result}");
                        Settings<EraiRawsWatcherSettings>.Current.ChatId = result;
                        return new(m, false, $"Changed Erai Raws output chat to {result}");
                    }
                    return new(m, false, $"\"{args.Arguments[2]}\" is not a valid number of type `long`");
                }

                if(op is "matchpatterns" or "matchpattern")
                {
                    var list = Settings<EraiRawsWatcherSettings>.Current.MatchPatterns;
                    var plist = list.ToArray();
                    var coll = args.Arguments.Skip(2).Distinct();
                    foreach (var s in coll)
                        if (!plist.Contains(s))
                            list.Add(s);
                    Log.Debug($"Added new patterns to list: {string.Join(", ", coll)}");
                    return new(m, false, $"Added new patterns");
                }

                return new(m, false, "Unrecognized argument");
            }
            finally
            {
                await Settings<EraiRawsWatcherSettings>.SaveSettingsAsync();
            }
        }

        public Task<CommandResponse> ActionReply(BotCommandArguments args)
        {
            throw new NotImplementedException();
        }

        public void Cancel(User user)
        {
            throw new NotImplementedException();
        }
    }
}