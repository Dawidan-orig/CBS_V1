using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CBS.Modules
{
    public class UnspecifiedModule : BaseCommandModule
    {
        [UsedImplicitly] public Random Rng { private get; set; }        

        [Command("random")]
        [Aliases("randomize", "ran")]
        [Description("Выдаёт случайное число от первого данного вами числа до второго")]
        public async Task RandomizeInt(CommandContext ctx, [Description("Начальное число")] int a,
            [Description("Конечное число")] int b)
        {
            await ctx.RespondAsync(Rng.Next(a, b) + "");
        }

        [Command("random")]
        [Description("Выдаёт случайное число от 0 до указаного числа")]
        public async Task RandomizeInt(CommandContext ctx, [Description("Конечное число")] int b)
        {
            await ctx.RespondAsync(Rng.Next(b) + "");
        }

        [Command("choose")]
        [Description("выбирает случайное слово из данных")]
        public async Task RandomizeString(CommandContext ctx,
            [Description("Слова или группы слов в кавычках, среди которых будет вестись выбор")] params string[] choices)
        {
            await ctx.RespondAsync(choices[Rng.Next(choices.Length)]);
        }

        private readonly Dictionary<string, PollData> _namedPolls = new Dictionary<string, PollData>();

        private readonly struct PollData // Прямо сейчас голосование сделано по другому
        {
            public readonly ulong MessageId;
            public readonly Dictionary<DiscordUser, HashSet<int>> UserChoices;
            public readonly Dictionary<DiscordUser, int> UserUniqueChoice;

            public PollData(ulong messageId)
            {
                MessageId = messageId;
                UserUniqueChoice = new Dictionary<DiscordUser, int>();
                UserChoices = new Dictionary<DiscordUser, HashSet<int>>();
            }
        }

        private static bool MeansYes(string s)
        {
            switch (s.ToLower())
            {
                case "да":
                case "+":
                case "1":
                case "y":
                case "yes":
                case "true":
                    return true;
                default:
                    return false;
            }
        }

        private static string PollTitle(string pollName, int level, string memberDisplayName)
        {
            switch (level)
            {
                case 1:
                    return $"Инициировано голосование: *\"{pollName}\"*";
                case 2:
                    return $"Инициировано голосование **с повышенным приоритетом**: *\"{pollName}\"* от {memberDisplayName}";
                case 3:
                    return $"**Инициировано важное голосование**: *\"{pollName}\"* от {memberDisplayName}!";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string PollContent(int level)
        {
            switch (level)
            {
                case 2:
                    return "@here";
                case 3:
                    return "@everyone";
                default:
                    return null;
            }
        }

        private static async Task FinilizePoll(CommandContext ctx, ulong messageId, DateTime timestamp, string pollName)
        {
            await Task.Delay(timestamp - DateTime.Now);

            var message = await ctx.Channel.GetMessageAsync(messageId);
            var resPollOptions = message.Embeds[0].Description.Split("\n");
            var max = 0;
            var winnerIndex = 0;  // todo: remove variable
            var winners = new List<int>();
            for (var i = 0; i < resPollOptions.Length; i++)
            {
                var votestr = resPollOptions[i].Split(" ");
                var votes = Convert.ToInt32(votestr.Last());
                if (votes < max) continue;
                if (votes > max) winners.Clear();
                winnerIndex = i;
                max = votes;
                winners.Add(winnerIndex);
            }
            if (max == 0) { await ctx.RespondAsync($"В голосовании *\"{pollName}\"* Никто не проголосовал :(\nвот это голосование: {message.JumpLink}"); return; }
            if (winners.Count == 1) await ctx.RespondAsync($"В голосовании *\"{pollName}\"* победил вариант : {resPollOptions[winnerIndex]},\nвот это голосование: {message.JumpLink}");
            else
            {
                var winnersNames = winners.Select(i => resPollOptions[i]).ToList();
                await ctx.RespondAsync($"В голосовании *\"{pollName}\"* победили варианты : {string.Join(" ,", winnersNames)},\nвот это голосование: {message.JumpLink}");
            }
        }

        private static string PollDescription(IReadOnlyList<string> pollOptions) =>
            string.Join(
                "\n",
                Enumerable
                    .Range(0, pollOptions.Count)
                    .Select(
                        option => $"{option + 1}) {pollOptions[option]} -- 0"
                    )
            );

        private static DiscordEmbed PollEmbed(
            int level, string pollName, string pollTitle, string memberDisplayName, IReadOnlyList<string> pollOptions, string multipleChoices, DateTime timestamp
            )
        {
            var title = PollTitle(pollName, level, memberDisplayName) + "  -  " + pollTitle;
            var description = PollDescription(pollOptions);
            var singleChoice = !MeansYes(multipleChoices);
            var footer = singleChoice ? "Можно голосовать лишь за одну опцию" : null;

            return new DiscordEmbedBuilder()
                .WithTitle(title)
                .WithTimestamp(timestamp)
                .WithDescription(description)
                .WithFooter(footer).Build();
        }

        [Command("vote")] //TODO : Ответ на сообщение должен позволять делать голос без прописывания названия
        [Aliases("poll")] 
        [Description("Стартует голосование")]
        public async Task Poll(
                CommandContext ctx,
                [Description("Название голосования, пишите его максимально ёмко и кратко")] string pollName,
                [Description("Описание голосования")] string pollTitle,
                [Description("Длительность голосования в минутах. Если дано 0 или меньше -- голосование бессрочно.")] int time,
                [Description("Важность голосования от 1 до 3 включительно")] int level,  //TODO должно зависеть от возможностей пользователя, что стартует голосование
                [Description("Можно ли голосовать за несколько опций сразу?")] string multipleChoices,
                [Description("Опции голосования через пробел")] params string[] pollOptions)
        // [Description("Продолжать ли голосования до тех пор, пока не останется один результат?")] string oneResult
        {
            if (_namedPolls.ContainsKey(pollName))
            {
                var messageWithPoll = await ctx.Channel.GetMessageAsync(_namedPolls[pollName].MessageId);
                await ctx.RespondAsync("Голосование с таким названием уже идёт: " + messageWithPoll.JumpLink);
                return;
            }

            time = Math.Max(time, 0);
            var timestamp = DateTime.Now.AddMinutes(time);

            level = Math.Clamp(level, 1, 3);
            var embed = PollEmbed(
                level, pollName, pollTitle, ctx.Member.DisplayName, pollOptions, multipleChoices, timestamp
                );
            var content = PollContent(level);

            var message = await ctx.RespondAsync(content, embed);
            _namedPolls.Add(pollName, new PollData(message.Id));

            if (time > 0) {
                await FinilizePoll(ctx, message.Id, timestamp, pollName);
                _namedPolls.Remove(pollName);
            }
        }

        private static Tuple<int, string> VotesAndPrefix(IReadOnlyList<string> pollOptions, int option)
        {
            var optionText = pollOptions[option];
            var splitOption = optionText.Split(" ");
            return new Tuple<int, string>(
                Convert.ToInt32(splitOption.Last()),
                optionText.Remove(optionText.Length - splitOption.Last().Length)
                );
        }

        [Command("vote")]
        [Description("Позволяет голосовать")]
        public async Task Poll(
            CommandContext ctx,
            [Description("Название голосования, за которое будет отдан голос")] string pollName,
            [Description("Вариант, за который вы и голосуете")] int option
            )
        {
            if (!_namedPolls.ContainsKey(pollName))
            {
                await ctx.RespondAsync("Такого голосования не существует");
                return;
            }

            var pollData = _namedPolls[pollName];

            --option;
            var message = await ctx.Channel.GetMessageAsync(pollData.MessageId);
            var originalEmbed = message.Embeds.First();
            var pollOptions = originalEmbed.Description.Split("\n");
            if (option < 0 || option >= pollOptions.Length)
            {
                await ctx.RespondAsync("Такого варианта голосования не существует");
                return;
            }

            var (votes, prefix) = VotesAndPrefix(pollOptions, option);
            if (originalEmbed.Footer is null)
            {
                var pollUserChoices = pollData.UserChoices;
                if (pollUserChoices.TryGetValue(ctx.User, out var selectedChoices))
                {
                    if (selectedChoices.Contains(option))
                    {
                        selectedChoices.Remove(option);
                        --votes;
                    }
                    else
                    {
                        selectedChoices.Add(option);
                        ++votes;
                    }
                }
                else
                {
                    pollUserChoices.Add(ctx.User, new HashSet<int> { option });
                    ++votes;
                }
            }
            else
            {
                var pollUniqueChoices = pollData.UserUniqueChoice;
                if (pollUniqueChoices.TryGetValue(ctx.User, out var selectedOption))
                {
                    if (selectedOption == option)
                    {
                        pollUniqueChoices.Remove(ctx.User);
                        --votes;
                    }
                    else
                    {
                        var (selectedVotes, selectedPrefix) = VotesAndPrefix(pollOptions, selectedOption);
                        --selectedVotes;
                        pollOptions[selectedOption] = selectedPrefix + selectedVotes;
                        pollUniqueChoices[ctx.User] = option;
                        ++votes;
                    }
                }
                else
                {
                    pollUniqueChoices[ctx.User] = option;
                    ++votes;
                }
            }

            pollOptions[option] = prefix + votes;

            var newDescription = string.Join("\n", pollOptions);
            var embed = new DiscordEmbedBuilder(originalEmbed).WithDescription(newDescription).Build();

            await message.ModifyAsync(embed);
            await ctx.Message.TryDeleteAsync();
        }

        [Command("vote")]
        [Description("Отправляет ссылку на голосование")]
        public async Task Poll(CommandContext ctx, [Description("Название голосования")] string pollName)
        {
            if (!_namedPolls.ContainsKey(pollName))
            {
                await ctx.RespondAsync("Такого голосования не существует");
                return;
            }
            var message = await ctx.Channel.GetMessageAsync(_namedPolls[pollName].MessageId);
            await ctx.RespondAsync("Вот это голосование: " + message.JumpLink);
        }
    }
}
