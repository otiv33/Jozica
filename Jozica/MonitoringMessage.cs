using System;
using System.Collections.Generic;
using System.Text;

namespace Jozica
{
    class MonitoringMessage
    {
        public KeyValuePair<string, string> Document { get; set; }
        public string TransID { get; set; }
        public string EvtDateTime { get; set; }
        public string TenantID { get; set; }
        public string ObjectId { get; set; }
        public string GPSLongitude { get; set; }
        public string GPSAltitude { get; set; }
        public string GPSLatitude { get; set; }
        public string ClearPartialUpdate { get; set; }
    }
}
