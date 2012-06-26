using System;
using log4net.Core;
using SignalR;
using SignalR.Hubs;

namespace log4net.SignalR
{
    public class SignalrAppender : Appender.AppenderSkeleton
    {
        private FixFlags _fixFlags = FixFlags.All;

        IHubContext hub = null;

        public SignalrAppender()
        {
            hub = GlobalHost.ConnectionManager.GetHubContext<SignalrAppenderHub>();
        }

        virtual public FixFlags Fix
        {
            get { return _fixFlags; }
            set { _fixFlags = value; }
        }

        override protected void Append(LoggingEvent loggingEvent)
        {
            // LoggingEvent may be used beyond the lifetime of the Append()
            // so we must fix any volatile data in the event
            loggingEvent.Fix = Fix;

            var formattedEvent = RenderLoggingEvent(loggingEvent);

            if (hub != null)
            {
                var e = new LogEntry(formattedEvent, loggingEvent);
                var logEventObject = new
                {
                    e.FormattedEvent,
                    Message = e.LoggingEvent.ExceptionObject != null ? e.LoggingEvent.ExceptionObject.Message : e.LoggingEvent.RenderedMessage,
                    Level = e.LoggingEvent.Level.Name,
                    TimeStamp = e.LoggingEvent.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss.fff")
                    /* e.LoggingEvent.Domain,
                    e.LoggingEvent.Identity,
                    e.LoggingEvent.LocationInformation,
                    e.LoggingEvent.LoggerName,
                    e.LoggingEvent.MessageObject,
                    e.LoggingEvent.Properties,
                    e.LoggingEvent.ThreadName,
                    e.LoggingEvent.UserName */
                };

                hub.Clients.onLoggedEvent(logEventObject);
            }
        }
    }


    public class LogEntry
    {
        public string FormattedEvent { get; private set; }
        public LoggingEvent LoggingEvent { get; private set; }

        public LogEntry(string formttedEvent, LoggingEvent loggingEvent)
        {
            FormattedEvent = formttedEvent;
            LoggingEvent = loggingEvent;
        }
    }
}
