﻿using System.Runtime.Serialization;

namespace ReportPortal.Client.Responses
{
    [DataContract]
    public class LaunchCreatedResponse
    {
        [DataMember(Name = "id")]
        public string Uuid { get; set; }
    }
}