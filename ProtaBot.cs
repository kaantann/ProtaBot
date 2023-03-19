using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ProtaBot_v1._0
{
    public class ProtaBot
    {

        #region Public Properties 
        public DiscordClient Client { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        #endregion

        public async Task RunAsync()
        {
            string json = string.Empty;
            string PATH = "config.json";

            using (var fs = File.OpenRead(PATH))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            var configJSON = JsonConvert.DeserializeObject<ConfigJSON>(json);


            var config = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = configJSON.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
            };

            Client = new DiscordClient(config);
            Client.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(2)
            });

            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { configJSON.Prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false,
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            Commands.RegisterCommands<GeneralCommands>();

            Commands.CommandErrored += OnCommandError;

            await Client.ConnectAsync();
            await Task.Delay(-1); //in case of connection lost situation
        }



        private Task OnClientReady(ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
        private async Task OnCommandError(CommandsNextExtension sender, CommandErrorEventArgs e)
        {

            //var channel = sender.Client.GetChannelAsync(1086758979847332001).Result;
            //await Client.SendMessageAsync(channel, e.Exception.Message);

            if (e.Exception is ChecksFailedException)
            {
                var castedException = e.Exception as ChecksFailedException;
                string cooldownTimer = string.Empty;

                foreach (var check in castedException.FailedChecks)
                {
                    var cooldown = (CooldownAttribute)check;
                    TimeSpan timeLeft = cooldown.GetRemainingCooldown(e.Context);
                    cooldownTimer = timeLeft.ToString(@"hh\:mm\:ss");
                }

                var cooldownMessage = new DiscordEmbedBuilder()
                {
                    Title = "I am in cooldown dude...",
                    Description = "Remaining time: " + cooldownTimer,
                    Color = DiscordColor.Red
                };

                await e.Context.Channel.SendMessageAsync(embed: cooldownMessage);


            }
        }
    }
}
