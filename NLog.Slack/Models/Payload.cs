using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NLog.Slack.Models
{
    [DataContract]
    public class Payload
    {
        //// ----------------------------------------------------------------------------------------------------------

        [DataMember(Name = "channel")]
        public string Channel { get; set; }

        //// ----------------------------------------------------------------------------------------------------------

        [DataMember(Name = "username")]
        public string Username { get; set; }

        //// ----------------------------------------------------------------------------------------------------------

        [DataMember(Name = "icon_url")]
        public string IconUrl { get; set; }

        //// ----------------------------------------------------------------------------------------------------------

        [DataMember(Name = "icon_emoji")]
        public string IconEmoji { get; set; }

        //// ----------------------------------------------------------------------------------------------------------

        [DataMember(Name = "text")]
        public string Text { get; set; }

        //// ----------------------------------------------------------------------------------------------------------

        [DataMember(Name = "attachments")]
        public IList<Attachment> Attachments { get; set; }

        //// ----------------------------------------------------------------------------------------------------------

        public Payload()
        {
            this.Attachments = new List<Attachment>();
        }

        //// ----------------------------------------------------------------------------------------------------------
    }
}