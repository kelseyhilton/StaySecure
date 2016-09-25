using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaySecure
{
    public class Report
    {
        int Id { get; set; }
        int NumVulnerabilitiesDetected { get; set; }
        List<Vulnerability> Vulnerabilities { get; set; }
    }

    public class Vulnerability
    {
        int Id { get; set; }
        int ReportId { get; set; }
        string Description { get; set; }
        VulnerabilityType Type { get; set; }
    }

    public class VulnerabilityType
    {
        int Id { get; set; }
        string TypeName { get; set; }
        string Description { get; set; }
    }
}
