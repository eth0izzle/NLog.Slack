using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

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

        public string ToJson()
        {
            var serializer = new DataContractJsonSerializer(typeof(Payload));
            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, this);
                memoryStream.Position = 0;
                using (var reader = new StreamReader(memoryStream))
                {
                    string json = reader.ReadToEnd();
                    return json;
                }
            }
        }

        //// ----------------------------------------------------------------------------------------------------------
    }
}