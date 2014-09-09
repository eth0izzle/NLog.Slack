NLog.Slack
==========
An NLog target for Slack - your logs in one place and instantly searchable, everywhere.

![NLog.Slack](http://i.imgur.com/xRlfNrN.png)

Installation
============
Via [NuGet](https://www.nuget.org/packages/NLog.Slack/): ```Install-Package NLog.Slack```

... or just build it your self!

Usage
=====
1. Create a [new Incoming Webhook integration](https://www.slack.com/integrations).
2. Configure NLog to use `NLog.Slack`:

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
            webHookUrl="https://xxx.slack.com/services/hooks/incoming-webhook?token=xxx"
            channel="#log"
            username="NLog.Slack"
            compact="false"
            icon=":ghost:" />
  </targets>

  <rules>
    <logger name="*" minlevel="Debug" writeTo="slackTarget" />
  </rules>
</nlog>
```

Note: it's recommended to set ```async="true"``` on `targets` so if the HTTP call to Slack fails or times out it doesn't slow down your application.

### Programmatically 

```
var config = new LoggingConfiguration();
var slackTarget = new SlackTarget
{
      Layout = "${message}",
      WebHookUrl = "http://xxx.slack.com/services/hooks/incoming-webhook?token=xxx",
      Channel = "#log"
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
Channel    | The channel name (e.g #log) or user (e.g. @eth0) to send NLog messages to. Leave blank to use the integration default
Username   | Name of the user that NLog messages comes from. Leave blank to use the integration default
Compact    | Set to true to just send the NLog layout text (no process info, colors, etc)
Icon       | Leave blank to use the integration default. Can either be a URL or Emoji
