﻿using System.Runtime.Serialization;

namespace ReportPortal.Client.Abstractions.Responses
{
    [DataContract]
    public class LaunchCreatedResponse
    {
        [DataMember(Name = "id")]
        public string Uuid { get; set; }

        [DataMember(Name = "number")]
        public long Number { get; set; }
    }
}
