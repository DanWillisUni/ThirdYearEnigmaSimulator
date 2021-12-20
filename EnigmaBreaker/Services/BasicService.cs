using EnigmaBreaker.Configuration.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnigmaBreaker.Services
{
    public class BasicService
    {
        private readonly ILogger<BasicService> _logger;
        private readonly BasicConfiguration _bc;
        public BasicService(ILogger<BasicService> logger, BasicConfiguration bc)
        {
            _logger = logger;
            _bc = bc;
        }

        public void root() {
            _logger.LogInformation("This is the root");
        }
    }
}
