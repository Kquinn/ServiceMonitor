using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceMonitor
{
    public class ConfigurationModel
    {
        public List<Service> ServiceList { get; set; }
        public SMTPConfig SMTPConfig { get; set; }
    }
}
