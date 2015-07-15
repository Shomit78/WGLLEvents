using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Administration;

namespace mvf.sharepoint.wgll.events.classes
{
    internal static class SPLogger
    {
        internal static void WriteInformationToLog(string informationString, object data) 
        {
            SPDiagnosticsService diagSvc = SPDiagnosticsService.Local;
            diagSvc.WriteTrace(0, new SPDiagnosticsCategory("CustomApplication", TraceSeverity.Verbose, EventSeverity.Information), TraceSeverity.Verbose,
                informationString, data);
        }
    }
}
