﻿using System.Collections.Generic;
using Lykke.Service.Decred_SignService.Core.Domain.Health;

namespace Lykke.Service.Decred_SignService.Core.Services
{
    // NOTE: See https://lykkex.atlassian.net/wiki/spaces/LKEWALLET/pages/35755585/Add+your+app+to+Monitoring
    public interface IHealthService
    {
        string GetHealthViolationMessage();
        IEnumerable<HealthIssue> GetHealthIssues();
    }
}