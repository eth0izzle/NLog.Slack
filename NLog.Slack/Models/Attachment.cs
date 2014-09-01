using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NLog.Slack.Models
{
    [DataContract]
    public class Attachment
    {
        //// ----------------------------------------------------------------------------------------------------------

        [DataMember(Name = "fallback")]
        public string Fallback { get; set; }

        //// ----------------------------------------------------------------------------------------------------------

        [DataMember(Name = "color")]
        public string Color { get; set; }

        //// ----------------------------------------------------------------------------------------------------------

        [DataMember(Name = "pretext")]
        public string PreText { get; set; }

        //// ----------------------------------------------------------------------------------------------------------

        [DataMember(Name = "text")]
        public string Text { get; set; }

        //// ----------------------------------------------------------------------------------------------------------

        [DataMember(Name = "fields")]
        public IList<Field> Fields { get; set; }

        //// ----------------------------------------------------------------------------------------------------------

        [DataMember(Name = "mrkdwn_in")]
        public IList<string> MarkdownIn { get; private set; }

        //// ----------------------------------------------------------------------------------------------------------

        public Attachment(string text)
        {
            this.Fallback = text;
            this.Fields = new List<Field>();
            this.MarkdownIn = new List<string> { "fields" };
        }

        //// ----------------------------------------------------------------------------------------------------------
    }
}