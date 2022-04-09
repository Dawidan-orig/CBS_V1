using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace CBS.Modules
{
    [UsedImplicitly]
    public class SimpleHelpFormatter : BaseHelpFormatter
    {
        //TODO

        public SimpleHelpFormatter(CommandContext ctx) : base(ctx)
        {

        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            throw new NotImplementedException();
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            throw new NotImplementedException();
        }

        public override CommandHelpMessage Build()
        {
            throw new NotImplementedException();
        }
    }
}