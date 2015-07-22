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
            string message = string.Format("MVF.SharePoint.WGLL.Events information: {0}", informationString);
            SPDiagnosticsService diagSvc = SPDiagnosticsService.Local;
            diagSvc.WriteTrace(0, new SPDiagnosticsCategory("CustomApplication", TraceSeverity.Verbose, EventSeverity.Information), TraceSeverity.Verbose,
                message, data);
        }

        internal static void WriteErrorToLog(string errorMessage, object data)
        {
            string message = string.Format("MVF.SharePoint.WGLL.Events error: {0}", errorMessage);
            SPDiagnosticsService diagSvc = SPDiagnosticsService.Local;
            diagSvc.WriteTrace(0, new SPDiagnosticsCategory("CustomApplication", TraceSeverity.High, EventSeverity.Error), TraceSeverity.High,
                message, data);
        }
    }
}
