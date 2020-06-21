using System;
using System.Collections.Generic;
using System.Text;
using Discord;

namespace gardener.Utilities
{
    class CustomActivity : IActivity
    {
        public string Name { get; }
        public ActivityType Type { get; }
        public ActivityProperties Flags { get; }
        public string Details { get; }

        public CustomActivity(string name, ActivityType type, ActivityProperties flags, string details)
        {
            Name = name;
            Type = type;
            Flags = flags;
            Details = details;
        }
    }
}
