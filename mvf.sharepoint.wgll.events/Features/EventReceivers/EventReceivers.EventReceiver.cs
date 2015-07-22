using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using mvf.sharepoint.wgll.events.classes;

namespace mvf.sharepoint.wgll.events.Features.EventReceivers
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("8af05fe5-1254-4c3e-b507-44cd0211b8eb")]
    public class EventReceiversEventReceiver : SPFeatureReceiver
    {
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            try
            {
                SPLogger.WriteInformationToLog("EventReceivers feature activated", null);
                //Create a list with Title and a field for ConfigurationValue
                SPWeb web = properties.Feature.Parent as SPWeb;
                SPList list = web.Lists.TryGetList(Constants.Configuration.ListName);
                if (list == null)
                {
                    SPLogger.WriteInformationToLog("Configuration list not found, creating new list", null);
                    //Create configuration list
                    Guid listId = web.Lists.Add(Constants.Configuration.ListName, "", SPListTemplateType.GenericList);
                    list = web.Lists.GetList(listId, false);
                    SPLogger.WriteInformationToLog("Configuration list created", null);

                    //Rename Title Field
                    SPField titleField = list.Fields[SPBuiltInFieldId.Title];
                    titleField.Title = Constants.Configuration.ListTitleFieldName;
                    titleField.Update();
                    SPLogger.WriteInformationToLog("Configuration list title field renamed", null);
                    
                    //Add configuration field to list
                    list.Fields.AddFieldAsXml(string.Format("<Field Type=\"Text\" Name=\"{0}\" DisplayName=\"{0}\" Title=\"{0}\" Required=\"TRUE\" />", 
                        Constants.Configuration.ListValueInternalFieldName));
                    SPField valueField = list.Fields[Constants.Configuration.ListValueInternalFieldName];
                    valueField.Title = Constants.Configuration.ListValueFieldName;
                    valueField.Update();
                    list.Update();
                    SPLogger.WriteInformationToLog("Configuration list column created", null);
                    
                    //Add configuration value field to default view
                    SPView view = list.DefaultView;
                    view.ViewFields.Add(valueField);
                    view.Update();
                    SPLogger.WriteInformationToLog("New Configuration list column added to default view", null);
                    
                    //Add configuration list item for MaxImageWidth
                    SPListItem maxImageWidthItem = list.AddItem();
                    maxImageWidthItem[SPBuiltInFieldId.Title] = Constants.Configuration.MaxImageWidthKey;
                    maxImageWidthItem[Constants.Configuration.ListValueInternalFieldName] = Constants.Configuration.MaxImageDefaultValue;
                    maxImageWidthItem.Update();
                    SPLogger.WriteInformationToLog("New Configuration list item added", null);

                    //Add configuration list item for MaxImageHeight
                    SPListItem maxImageHeightItem = list.AddItem();
                    maxImageHeightItem[SPBuiltInFieldId.Title] = Constants.Configuration.MaxImageHeightKey;
                    maxImageHeightItem[Constants.Configuration.ListValueInternalFieldName] = Constants.Configuration.MaxImageDefaultValue;
                    maxImageHeightItem.Update();
                    SPLogger.WriteInformationToLog("New Configuration list item added", null);

                }
                SPLogger.WriteInformationToLog("EventReceivers feature activatation complete", null);
            }
            catch (Exception ex)
            {
                SPLogger.WriteErrorToLog(ex.Message.ToString(), null);
            }
        }

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            try
            {
                SPLogger.WriteInformationToLog("EventReceivers feature de-activated", null);
                //Delete Configuration list
                SPWeb web = properties.Feature.Parent as SPWeb;
                if (web != null)
                {
                    SPLogger.WriteInformationToLog("Configuration list exists", null);
                    SPList list = web.Lists.TryGetList(Constants.Configuration.ListName);
                    if (list != null)
                    {
                        web.Lists.Delete(list.ID);
                        SPLogger.WriteInformationToLog("Configuration list deleted", null);
                    }
                }
                SPLogger.WriteInformationToLog("EventReceivers feature deactivation complete", null);
            }
            catch (Exception ex)
            {
                SPLogger.WriteErrorToLog(ex.Message.ToString(), null);
            }
        }

    }
}
