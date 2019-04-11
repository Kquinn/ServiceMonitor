using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceMonitor
{
    public class SMTPConfig
    {
        public string ToEmail { get; set; }
        public string FromEmail { get; set; }
        public string Server { get; set; }
    }
}
