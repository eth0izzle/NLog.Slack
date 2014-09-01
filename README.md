NLog.Slack
==========
An NLog target for Slack - your logs in one place and instantly searchable, everywhere.

![NLog.Slack](http://i.imgur.com/xRlfNrN.png)

Installation
============
```Install-Package NLog.Slack```

... or just build it your self!

Usage
=====

Simply update your NLog.config to load the NLog.Slack extension and add a new `<target />`.

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
            layout="${message}" <!-- NLog layout format -->
            webHookUrl="https://tummi.slack.com/services/hooks/incoming-webhook?token=xxx"
            channel="#log"
            username="NLog.Slack"
            verbose="true"
            icon=":ghost:" />
  </targets>

  <rules>
    <logger name="*" minlevel="Debug" writeTo="slackTarget" />
  </rules>
</nlog>
```

Key        | Description
----------:| -----------
webHookUrl | Grab your Webhook URL from your Incoming WebHooks integration in Slack
channel    | Leave blank to use the integration default
username   | Leave blank to use the integration default
verbose    | Set to false to just send the NLog layout text (no process info, colors, etc)
icon       | Leave blank to use the integration default. Can either be a URL or Emoji

Note: it's recommended to set ```async="true"``` so if the HTTP call to Slack fails or timesout it doesn't slow down your application.
