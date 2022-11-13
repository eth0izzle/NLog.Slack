NLog.Slack
==========

[![Nuget](https://img.shields.io/nuget/v/NLog.Slack.svg)](https://www.nuget.org/packages/NLog.Slack/)

An NLog target for Slack - your logs in one place and instantly searchable, everywhere.

![NLog.Slack](http://i.imgur.com/xRlfNrN.png)

**Note**: it is no longer possible to pass in a custom channel, username or icon. This must be defined by your Slack App.

Installation
============
Via [NuGet](https://www.nuget.org/packages/NLog.Slack/): ```Install-Package NLog.Slack```

... or just build it your self!

Usage
=====
1. Create a [new Slack App](https://api.slack.com/apps?new_app=1) in the correct Workspace.
2. Add the [Incoming Webhooks](https://api.slack.com/apps/AGNC720HF/incoming-webhooks?) functionality to your App.
3. Generate a new Webhook URL and Authorize it to post to a channel.
4. Copy your Webhook URL and configure NLog via your NLog.config file or programmatically, as below.

### NLog.config

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">

  <extensions>
    <add assembly="NLog.Slack" />
  </extensions>

  <targets async="true">
    <target xsi:type="Slack"
            name="slackTarget"
            layout="${message}"
            webHookUrl="https://hooks.slack.com/services/T00000000/B00000000/XXXXXXXXXXXXXXXXXXXXXXXX"
            compact="false">

			<field name="Machine Name" layout="${machinename}" />
			<field name="Process Name" layout="${processname}" />
			<field name="Process PID" layout="${processid}" />
	</target>
  </targets>

  <rules>
    <logger name="*" minlevel="Debug" writeTo="slackTarget" />
  </rules>
</nlog>
```

Note: it's recommended to set ```async="true"``` on `targets` so if the HTTP call to Slack fails or times out it doesn't slow down your application.

### Programmatically 

```c#
var config = new LoggingConfiguration();
var slackTarget = new SlackTarget
{
      Layout = "${message}",
      WebHookUrl = "https://hooks.slack.com/services/T00000000/B00000000/XXXXXXXXXXXXXXXXXXXXXXXX",
};

config.AddTarget("slack", slackTarget);

var slackTargetRules = new LoggingRule("*", LogLevel.Debug, slackTarget);
config.LoggingRules.Add(slackTargetRules);

LogManager.Configuration = config;
```

And you're good to go!

### Configuration Options

Key        | Description
----------:| -----------
WebHookUrl | Grab your Webhook URL (__with the token__) from your Incoming Webhooks integration in Slack
Compact    | Set to true to just send the NLog layout text (no process info, colors, etc)
WebProxyUrl | Url for custom WebProxy (Optional)
