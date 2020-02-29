using System;
using System.Collections.Generic;
using System.Text;

namespace Jozica
{
    class Message
    {
        public MonitoringMessage monitoringMessage { get; set; }
        public string connectionEvent { get; set; }
        public string ObjectId { get; set; }
        public string GPSAltitude { get; set; }
        public string UnknownFields { get; set; }
        public string ClearPartialUpdate { get; set; }
        public string GPSLongitude { get; set; }
        public string GPSLatitude { get; set; }
        public string EvtDateTime { get; set; }
        public string TransID { get; set; }
        public string TenantID { get; set; }
        public string Jobname { get; set; }
        public string ROName { get; set; }
        public string Increment { get; set; }
        public string MessageType { get; set; }
        public string Document { get; set; }
    }
}
