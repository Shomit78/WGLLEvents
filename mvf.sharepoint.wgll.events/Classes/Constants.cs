using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mvf.sharepoint.wgll.events.classes
{
    internal static class Constants
    {
        internal static class Configuration
        {
            internal static readonly string ListName = "WGLLConfiguration";
            internal static readonly string ListDescription = "WGLL Configuration key value pairs";
            internal static readonly string ListTitleFieldName = "Configuration Key";
            internal static readonly string ListValueInternalFieldName = "WGLLConfigurationValue";
            internal static readonly string ListValueFieldName = "Configuration Value";
            internal static readonly string MaxImageWidthKey = "MaxImageWidth";
            internal static readonly string MaxImageHeightKey = "MaxImageHeight";
            internal static readonly string MaxImageDefaultValue = "500";
        }
    }
}
