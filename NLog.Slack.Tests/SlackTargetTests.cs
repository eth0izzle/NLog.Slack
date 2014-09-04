using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NLog.Slack.Tests
{
    [TestClass]
    public class SlackTargetTests
    {
        //// ----------------------------------------------------------------------------------------------------------

        [TestMethod]
        public void DefaultSettings_ShouldBeCorrect()
        {
            var slackTarget = new TestableSlackTarget();

            slackTarget.Channel.Should().Be(null);
            slackTarget.Compact.Should().Be(false);
            slackTarget.Icon.Should().Be(null);
            slackTarget.Username.Should().Be(null);
            slackTarget.WebHookUrl.Should().Be(null);
        }

        //// ----------------------------------------------------------------------------------------------------------

        [TestMethod]
        public void CustomSettings_ShouldBeCorrect()
        {
            const string channel = "#log";
            const bool compact = true;
            const string icon = ":ghost:";
            const string username = "NLog.Slack";
            const string webHookUrl = "http://slack.is.awesome.com";

            var slackTarget = new TestableSlackTarget
                {
                    Channel = channel,
                    Compact = compact,
                    Icon = icon,
                    Username = username,
                    WebHookUrl = webHookUrl
                };

            slackTarget.Channel.Should().Be(channel);
            slackTarget.Compact.Should().Be(compact);
            slackTarget.Icon.Should().Be(icon);
            slackTarget.Username.Should().Be(username);
            slackTarget.WebHookUrl.Should().Be(webHookUrl);
        }

        //// ----------------------------------------------------------------------------------------------------------

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InitializeTarget_EmptyWebHookUrl_ShouldThrowException()
        {
            var slackTarget = new TestableSlackTarget();

            slackTarget.Initialize();
        }

        //// ----------------------------------------------------------------------------------------------------------

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InitializeTarget_IncorrectWebHookUrl_ShouldThrowException()
        {
            var slackTarget = new TestableSlackTarget
                {
                    WebHookUrl = "IM A BIG FAT PHONY"
                };

            slackTarget.Initialize();
        }

        //// ----------------------------------------------------------------------------------------------------------

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InitializeTarget_IncorrectChannel_ShouldThrowException()
        {
            var slackTarget = new TestableSlackTarget
            {
                WebHookUrl = "http://slack.is.awesome.com",
                Channel = "wrong"
            };

            slackTarget.Initialize();
        }

        //// ----------------------------------------------------------------------------------------------------------

        [TestMethod]
        public void InitializeTarget_CorrectChannelWithHash_TargetShouldInitialize()
        {
            var slackTarget = new TestableSlackTarget
            {
                WebHookUrl = "http://slack.is.awesome.com",
                Channel = "#log"
            };

            slackTarget.Initialize();
        }

        //// ----------------------------------------------------------------------------------------------------------

        [TestMethod]
        public void InitializeTarget_CorrectChannelWithAt_TargetShouldInitialize()
        {
            var slackTarget = new TestableSlackTarget
            {
                WebHookUrl = "http://slack.is.awesome.com",
                Channel = "@slackbot"
            };

            slackTarget.Initialize();
        }

        //// ----------------------------------------------------------------------------------------------------------
    }
}