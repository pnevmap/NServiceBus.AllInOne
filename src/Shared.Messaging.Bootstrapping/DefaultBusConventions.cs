using System;
using System.Linq;

namespace Shared.Messaging
{
    public static class DefaultBusConventions
    {
        private static readonly string[] CommandMarkers =
        {
            ".command.contract",
            ".command.contracts",
            ".commands.contract",
            ".commands.contracts",
            ".command",
            ".commands"
        };

        private static readonly string[] EventMarkers =
        {
            ".event.contract",
            ".event.contracts",
            ".events.contract",
            ".events.contracts",
            ".event",
            ".events"
        };

        private static readonly string[] MessageMarkers =
        {
            ".message.contract",
            ".message.contracts",
            ".messages.contract",
            ".messages.contracts",
            ".message",
            ".messages"
        };

        public static bool IsCommand(Type type)
        {
            var @namespace = type.Namespace;

            return @namespace != null && CommandMarkers.Any(m => @namespace.EndsWith(m, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsEvent(Type type)
        {
            var @namespace = type.Namespace;

            return @namespace != null && EventMarkers.Any(m => @namespace.EndsWith(m, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsMessage(Type type)
        {
            var @namespace = type.Namespace;

            return @namespace != null && MessageMarkers.Any(m => @namespace.EndsWith(m, StringComparison.OrdinalIgnoreCase));
        }
    }
}