using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Configuration
{
    public class RabbitMQConfiguration
    {
        public string Server { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ExchangeName { get; set; }
        public string Bindingkey { get; set; }
    }
}
