using NLog.Common;

namespace NLog.Slack.Tests
{
    public class TestableSlackTarget : SlackTarget
    {
        public void Initialize()
        {
            InitializeTarget();
        }

        public new void Write(AsyncLogEventInfo eventInfo)
        {
            base.Write(eventInfo);
        }
    }
}