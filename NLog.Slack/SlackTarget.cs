using System;
using System.Diagnostics;
using System.Linq;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Slack.Models;
using NLog.Targets;

namespace NLog.Slack
{
    [Target("Slack")]
    public class SlackTarget : TargetWithLayout
    {
        //// ----------------------------------------------------------------------------------------------------------

        private readonly Process _currentProcess = Process.GetCurrentProcess();

        //// ----------------------------------------------------------------------------------------------------------

        [RequiredParameter]
        public string WebHookUrl { get; set; }

        //// ----------------------------------------------------------------------------------------------------------

        public SimpleLayout Channel { get; set; }

        //// ----------------------------------------------------------------------------------------------------------

        public SimpleLayout Username { get; set; }

        //// ----------------------------------------------------------------------------------------------------------

        public string Icon { get; set; }

        //// ----------------------------------------------------------------------------------------------------------

        public bool Compact { get; set; }

        //// ----------------------------------------------------------------------------------------------------------

        protected override void InitializeTarget()
        {
            if (String.IsNullOrWhiteSpace(this.WebHookUrl))
                throw new ArgumentOutOfRangeException("WebHookUrl", "Webhook URL cannot be empty.");

            Uri uriResult;
            if (!Uri.TryCreate(this.WebHookUrl, UriKind.Absolute, out uriResult))
                throw new ArgumentOutOfRangeException("WebHookUrl", "Webhook URL is an invalid URL.");

            if (!String.IsNullOrWhiteSpace(this.Channel.Text)
                && (!this.Channel.Text.StartsWith("#") && !this.Channel.Text.StartsWith("@") && !this.Channel.Text.StartsWith("${")))
                throw new ArgumentOutOfRangeException("Channel", "The Channel name is invalid. It must start with either a # or a @ symbol or use a variable.");

            base.InitializeTarget();
        }

        //// ----------------------------------------------------------------------------------------------------------

        protected override void Write(AsyncLogEventInfo info)
        {
            try
            {
                this.SendToSlack(info);
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

            if (!String.IsNullOrWhiteSpace(this.Channel.Render(info.LogEvent)))
                slack.ToChannel(this.Channel.Render(info.LogEvent));

            if (!String.IsNullOrWhiteSpace(this.Icon))
                slack.WithIcon(this.Icon);

            if (!String.IsNullOrWhiteSpace(this.Username.Render(info.LogEvent)))
                slack.AsUser(this.Username.Render(info.LogEvent));

            if (!this.Compact)
            {
                var color = this.GetSlackColorFromLogLevel(info.LogEvent.Level);
                var attachment = new Attachment(message) { Color = color };
                attachment.Fields.Add(new Field("Process Name") { Value = String.Format("{0}\\{1}", (_currentProcess.MachineName != "." ? _currentProcess.MachineName : System.Environment.MachineName), _currentProcess.ProcessName), Short = true });
                attachment.Fields.Add(new Field("Process PID") { Value = _currentProcess.Id.ToString(), Short = true });
                slack.AddAttachment(attachment);

                var exception = info.LogEvent.Exception;
                if (exception != null)
                {
                    var exceptionAttachment = new Attachment(exception.Message) { Color = color };
                    exceptionAttachment.Fields.Add(new Field("Type") { Value = exception.GetType().FullName, Short = true });

                    if (!String.IsNullOrWhiteSpace(exception.StackTrace))
                        exceptionAttachment.Text = exception.StackTrace;

                    slack.AddAttachment(exceptionAttachment);
                }

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