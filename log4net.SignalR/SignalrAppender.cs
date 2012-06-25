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
                hub.Clients.OnMessageLogged(new LogEntry(formattedEvent, loggingEvent));
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
