using System;
using System.Linq;
using EventStore.ClientAPI;

namespace AggregateSource.EventStore.Framework
{
    public static class EventStoreConnectionExtensions
    {
        public static void DeleteAllStreams(this IEventStoreConnection connection)
        {
            var slice = connection.ReadAllEventsForward(Position.Start, Int32.MaxValue, false,
                                                        EmbeddedEventStore.Credentials);
            var streams = slice.
                Events.
                Select(_ => _.OriginalStreamId).
                Where(StreamNameIsNotReserved).
                Distinct();
            foreach (var stream in 
                from _ in streams
                let streamStatusSlice = connection.ReadStreamEventsForward(_, 0, 1, false)
                where streamStatusSlice.Status != SliceReadStatus.StreamDeleted &&
                      streamStatusSlice.Status != SliceReadStatus.StreamNotFound
                select _)
            {
                connection.DeleteStream(stream, ExpectedVersion.Any, EmbeddedEventStore.Credentials);
            }
        }

        static bool StreamNameIsNotReserved(string streamName)
        {
            if (streamName.StartsWith("$$$"))
                return false;
            if (streamName.StartsWith("$$"))
                return false;
            if (streamName.StartsWith("$"))
                return false;
            return true;
        }
    }
}