using System;
using System.Diagnostics;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Slack.Models;
using NLog.Targets;

namespace NLog.Slack
{
    [Target("Slack")]
    public sealed class SlackTarget : TargetWithLayout
    {
        //// ----------------------------------------------------------------------------------------------------------

        private readonly Process _currentProcess = Process.GetCurrentProcess();

        //// ----------------------------------------------------------------------------------------------------------

        [RequiredParameter]
        public string WebHookUrl { get; set; }

        //// ----------------------------------------------------------------------------------------------------------

        public string Channel { get; set; }

        //// ----------------------------------------------------------------------------------------------------------

        public string Username { get; set; }

        //// ----------------------------------------------------------------------------------------------------------

        public string Icon { get; set; }

        //// ----------------------------------------------------------------------------------------------------------

        public bool Verbose { get; set; }

        //// ----------------------------------------------------------------------------------------------------------

        protected override void Write(AsyncLogEventInfo info)
        {
            try
            {
                SendToSlack(info);
            }
            catch (Exception e)
            {
                info.Continuation(e);
            }
        }

        //// ----------------------------------------------------------------------------------------------------------

        private void SendToSlack(AsyncLogEventInfo info)
        {
            var message = Layout.Render(info.LogEvent);
            var slack = SlackMessageBuilder
                .Build(this.WebHookUrl)
                .OnError(e => info.Continuation(e))
                .WithMessage(message);

            if (!String.IsNullOrWhiteSpace(this.Channel))
                slack.ToChannel(this.Channel);

            if (!String.IsNullOrWhiteSpace(this.Icon))
                slack.WithIcon(this.Icon);

            if (!String.IsNullOrWhiteSpace(this.Username))
                slack.AsUser(this.Username);

            if (this.Verbose)
            {
                var attachment = new Attachment(message);
                attachment.Color = this.GetSlackColorFromLogLevel(info.LogEvent.Level);

                var exception = info.LogEvent.Exception;
                if (exception != null)
                {
                    if (!String.IsNullOrWhiteSpace(exception.StackTrace))
                        attachment.Fields.Insert(0, new Field("Stack Trace") { Value = "```" + exception.StackTrace + "```" });

                    attachment.Fields.Insert(0, new Field("Type") { Value = exception.GetType().FullName, Short = true });
                    attachment.Fields.Insert(0, new Field("Message") { Value = exception.Message, Short = true });
                }

                attachment.Fields.Add(new Field("Process Name") { Value = String.Format("{0}\\{1}", _currentProcess.MachineName, _currentProcess.ProcessName), Short = true });
                attachment.Fields.Add(new Field("Process PID") { Value = _currentProcess.Id.ToString(), Short = true });

                slack.AddAttachment(attachment);
            }

            slack.Send();
        }

        //// ----------------------------------------------------------------------------------------------------------

        private string GetSlackColorFromLogLevel(LogLevel level)
        {
            switch (level.Name.ToLowerInvariant())
            {
                case "warn":
                    return "warning";

                case "error":
                case "fatal":
                    return "danger";

                case "info":
                    return "#2a80b9";

                default:
                    return "#cccccc";
            }
        }

        //// ----------------------------------------------------------------------------------------------------------
    }
}