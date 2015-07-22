using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Permissions;
using Microsoft.Office.Server.Diagnostics;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using mvf.sharepoint.wgll.events.classes;

namespace mvf.sharepoint.wgll.events.EventReceviers.FileAdded
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class FileAdded : SPItemEventReceiver
    {
      
        /// <summary>
        /// An item was added.
        /// </summary>
        public override void ItemAdded(SPItemEventProperties properties)
        {
            SPLogger.WriteInformationToLog("ItemAdded event fired", null);
            bool enabledEventFiring = false;

            try
            {
                if (properties.ListItem != null)
                {
                    if (properties.ListItem[SPBuiltInFieldId.DocIcon] != null)
                    {
                        var fileType = properties.ListItem[SPBuiltInFieldId.DocIcon].ToString();
                        var resize = false;
                        ImageFormat format = ImageFormat.Jpeg;
                        SPLogger.WriteInformationToLog(string.Format("Checking file format: {0}", fileType), null);
                        switch (fileType)
                        {
                            case "jpeg":
                                resize = true;
                                format = ImageFormat.Jpeg;
                                break;
                            case "jpg":
                                resize = true;
                                format = ImageFormat.Jpeg;
                                break;
                            case "png":
                                resize = true;
                                format = ImageFormat.Png;
                                break;
                            case "gif":
                                resize = true;
                                format = ImageFormat.Gif;
                                break;
                            case "bmp":
                                resize = true;
                                format = ImageFormat.Bmp;
                                break;
                            default:
                                resize = false;
                                format = ImageFormat.Jpeg;
                                break;
                        }

                        if (resize)
                        {
                            SPLogger.WriteInformationToLog("File is an image, resizing started", null);
                            enabledEventFiring = base.EventFiringEnabled;
                            base.EventFiringEnabled = false;

                            SPLogger.WriteInformationToLog("Disabled EventFiring", null);
                            SPFile imageFile = properties.ListItem.File;

                            int MaxAllowedWidth = 500;
                            int MaxAllowedHeight = 300;
                            SPLogger.WriteInformationToLog("Set default max width and height", null);

                            SPList configurationList = properties.Web.Lists.TryGetList(Constants.Configuration.ListName);
                            if (configurationList != null)
                            {
                                SPLogger.WriteInformationToLog("Found Configuration list", null);
                                SPQuery query = new SPQuery();
                                query.ViewFields = string.Format("<FieldRef Name=\"Title\" /><FieldRef Name=\"{0}\" />", Constants.Configuration.ListValueInternalFieldName);
                                SPListItemCollection items = configurationList.GetItems(query);
                                if (items.Count > 0)
                                {
                                    SPLogger.WriteInformationToLog("Retrieved Configuration list items", null);
                                    foreach (SPListItem item in items)
                                    {
                                        SPLogger.WriteInformationToLog("Iterating items", null);
                                        if (item[SPBuiltInFieldId.Title] != null)
                                        {
                                            SPLogger.WriteInformationToLog(string.Format("Title field is not null. Value: {0}", item[SPBuiltInFieldId.Title].ToString()), null);
                                            if (item[SPBuiltInFieldId.Title].ToString() == Constants.Configuration.MaxImageWidthKey)
                                            {
                                                SPLogger.WriteInformationToLog(string.Format("Match found on key: {0}", Constants.Configuration.MaxImageWidthKey), null);
                                                int number;
                                                bool result = int.TryParse(item[Constants.Configuration.ListValueInternalFieldName].ToString(), out number);
                                                if (result)
                                                {
                                                    MaxAllowedWidth = number;
                                                    SPLogger.WriteInformationToLog(string.Format("Set max allowed width to configuration value: {0}", number), null);
                                                }
                                                else
                                                {
                                                    SPLogger.WriteInformationToLog(string.Format(
                                                        "Error parsing Configuration value for max allowed width, setting to default. Configuration value: {0}",
                                                        item[Constants.Configuration.ListValueInternalFieldName].ToString()),
                                                        null);
                                                }
                                            }
                                            else
                                            {
                                                SPLogger.WriteInformationToLog(string.Format("No match found on key: {0}", Constants.Configuration.MaxImageWidthKey), null);
                                                if (item[SPBuiltInFieldId.Title].ToString() == Constants.Configuration.MaxImageHeightKey)
                                                {
                                                    SPLogger.WriteInformationToLog(string.Format("Match found on key: {0}", Constants.Configuration.MaxImageHeightKey), null);
                                                    int number;
                                                    bool result = int.TryParse(item[Constants.Configuration.ListValueInternalFieldName].ToString(), out number);
                                                    if (result)
                                                    {
                                                        MaxAllowedHeight = number;
                                                        SPLogger.WriteInformationToLog(string.Format("Set max allowed height to configuration value: {0}", number), null);
                                                    }
                                                    else
                                                    {
                                                        SPLogger.WriteInformationToLog(string.Format(
                                                            "Error parsing Configuration value for max allowed height, setting to default. Configuration value: {0}",
                                                            item[Constants.Configuration.ListValueInternalFieldName].ToString()),
                                                            null);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            SPLogger.WriteInformationToLog("Item title field was null", null);
                                        }
                                    }
                                }
                            }

                            SPLogger.WriteInformationToLog("Loading file into memory", null);
                            MemoryStream outStream = new MemoryStream();
                            MemoryStream inStream = new MemoryStream(imageFile.OpenBinary(), true);
                            SPLogger.WriteInformationToLog("File loaded into memory", null);


                            //Get the Image
                            Image imageObject = Image.FromStream(inStream);

                            //Set Height and Width
                            SPLogger.WriteInformationToLog("Setting height and width according to max allowed values", null);
                            //Now Calculate the Aspect Ratio
                            double aspectRatio = (double)imageObject.Width / (double)imageObject.Height;
                            //Check if the height of image is greater than the allowed height and reset it
                            int newHeight = (imageObject.Height > MaxAllowedHeight) ? MaxAllowedHeight : imageObject.Height;
                            //Check if the width of image is greater than the allowed width and reset it
                            int newWidth = (imageObject.Width > MaxAllowedWidth) ? MaxAllowedWidth : imageObject.Width;
                            //Calculate new height or width according to aspect ratio based on the orientation of the picture

                            //If width is greater than the height it’s a Landscape
                            if (newWidth > newHeight)
                            {
                                //Calculate new Height by multiplying Width with aspect ratio
                                newHeight = (int)(newWidth / aspectRatio);
                                //in some cases newly calculated height can be greater than the Max Allowed Height so we trim down the height to maximum allowed limit limit and calculate the width

                                if (newHeight > MaxAllowedHeight)
                                {
                                    newHeight = MaxAllowedHeight;
                                    newWidth = (int)(aspectRatio * newHeight);
                                }
                            }
                            //otherwise it’s a portrait
                            else
                            {
                                //Calculate new Width by multiplying Height with aspect ratio
                                newWidth = (int)(aspectRatio * newHeight);
                            }

                            SPLogger.WriteInformationToLog("Creating empty Bitmap to store resized image", null);
                            //Create an empty Bitmap and do not think this means you have to have a bitmap to resize we will set the format in next line
                            Bitmap resultantImage = new Bitmap(newWidth, newHeight);

                            using (Graphics graphics = Graphics.FromImage(resultantImage))
                            {
                                SPLogger.WriteInformationToLog("Using Graphics to draw new image", null);
                                //set the parameters to maintain the quality
                                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                                //Draw the new resized image
                                graphics.DrawImage(imageObject, 0, 0, (float)newWidth, (float)newHeight);
                                //And save it   
                                resultantImage.Save(outStream, format);
                                SPLogger.WriteInformationToLog("Image saved to stream", null);
                            }

                            imageFile.SaveBinary(outStream, false);
                            imageFile.Update();
                            SPLogger.WriteInformationToLog("Image resizing completed", null);

                            inStream.Close();
                            outStream.Close();

                            base.EventFiringEnabled = enabledEventFiring;
                            SPLogger.WriteInformationToLog("Reset EventFiring to initial value", null);

                        }
                        else
                        {
                            SPLogger.WriteInformationToLog("File uploaded not an image", null);
                        }
                    }
                    else
                    {
                        SPLogger.WriteInformationToLog("File type not set, no resizing done", null);
                    }
                }
                else
                {
                    SPLogger.WriteInformationToLog("ListItem is null", null);
                }
            }
            catch (Exception ex)
            {
                SPLogger.WriteErrorToLog(ex.Message.ToString(), null);
            }
        }


    }
}