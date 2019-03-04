using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace NLog.Slack.Models
{
    [DataContract]
    public class Payload
    {
        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "attachments")]
        public IList<Attachment> Attachments { get; set; }

        public Payload()
        {
            this.Attachments = new List<Attachment>();
        }

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
    }
}