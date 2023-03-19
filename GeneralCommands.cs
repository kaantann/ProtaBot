using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ProtaBot_v1._0
{
    public class GeneralCommands : BaseCommandModule
    {

        [Command("hello")]
        public async Task TestCommand(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync($"Hello");
        }

        [Command("formatcode")]
        public async Task FormatCodeCommand(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync(Formatter.BlockCode(ctx.RawArgumentString));
        }

        [Command("meme")]
        [Cooldown(1,30,CooldownBucketType.Channel)]
        public async Task MemeCommand(CommandContext ctx)
        {
            ulong id_forMemesOnly = 1086640856771076197;

            if (ctx.Channel.Id != id_forMemesOnly)
            {
                await ctx.Channel.SendMessageAsync("I am not allowed to post memes in this channel.");
                return;
            }

            var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync("https://www.reddit.com/r/memes/random.json");

            var json = JArray.Parse(response);
            var memeUrl = json[0]["data"]["children"][0]["data"]["url"].ToString();

            var embed = new DiscordEmbedBuilder()
                                           .WithTitle("Daily Dose of Meme!")
                                           .WithImageUrl(memeUrl)
                                           .Build();

            await ctx.Channel.SendMessageAsync(embed);
        }
        [Command("poll")]
        [Cooldown(2, 200, CooldownBucketType.User)]
        public async Task PollCommand(CommandContext ctx, string Option1, string Option2, string Option3, string Option4,int timeLimitSec ,params string[] Question)
        {
            var interactivity = ctx.Client.GetInteractivity();
            TimeSpan timer = TimeSpan.FromSeconds(timeLimitSec);

            DiscordEmoji[] optionEmojis =
            {
                DiscordEmoji.FromName(ctx.Client, ":one:",false ),
                DiscordEmoji.FromName(ctx.Client, ":two:",false ),
                DiscordEmoji.FromName(ctx.Client, ":three:",false ),
                DiscordEmoji.FromName(ctx.Client, ":four:",false )
            };

            string optionsString = optionEmojis[0] + " | " + Option1 + "\n" +
                                                   optionEmojis[1] + " | " + Option2 + "\n" +
                                                   optionEmojis[2] + " | " + Option3 + "\n" +
                                                   optionEmojis[3] + " | " + Option4;

            var pollMessage = new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder()

                .WithColor(DiscordColor.Azure)
                .WithTitle(string.Join(" ", Question))
                .WithDescription(optionsString)
                );

            var putReaction = await ctx.Channel.SendMessageAsync(pollMessage);

            foreach (var emoji in optionEmojis)
            {
                await putReaction.CreateReactionAsync(emoji);
            }

            var result = await interactivity.CollectReactionsAsync(putReaction, timer);

            int count1 = 0;
            int count2 = 0;
            int count3 = 0;
            int count4 = 0;

            foreach (var emoji in result)
            {
                if (emoji.Emoji == optionEmojis[0])
                    count1++;
                if (emoji.Emoji == optionEmojis[1])
                    count2++;
                if (emoji.Emoji == optionEmojis[2])
                    count3++;
                if (emoji.Emoji == optionEmojis[3])
                    count4++;
            }

            int totalVotes = count1 + count2 + count3 + count4;

            string resultString = optionEmojis[0] + ": " + count1 + " Votes \n" +
                optionEmojis[1] + ": " + count2 + " Votes \n" +
                optionEmojis[2] + ": " + count3 + " Votes \n" +
                optionEmojis[3] + ": " + count4 + " Votes \n\n" +
                "The total number of votes is : " + totalVotes;

            var resultsMessage = new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder()

                .WithColor(DiscordColor.Green)
                .WithTitle("Results of the Poll")
                .WithDescription(resultString)
                );

            await ctx.Channel.SendMessageAsync(resultsMessage);

        }


    }
}
