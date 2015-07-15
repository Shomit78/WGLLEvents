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
            bool enabledEventFiring = false;

            try
            {
                var fileType = properties.ListItem[SPBuiltInFieldId.DocIcon].ToString();
                var resize = false;
                ImageFormat format = ImageFormat.Jpeg;
                switch (fileType)
                {
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
                    enabledEventFiring = base.EventFiringEnabled;
                    base.EventFiringEnabled = false;

                    SPFile imageFile = properties.ListItem.File;

                    //Here I am setting these manually but this will be the point where you will get these from some XML file or Config Store or some list etc.
                    int MaxAllowedWidth = 500;
                    int MaxAllowedHeight = 300;
                    MemoryStream outStream = new MemoryStream();
                    MemoryStream inStream = new MemoryStream(imageFile.OpenBinary(), true);

                    //Get the Image
                    Image imageObject = Image.FromStream(inStream);

                    //Set Height and Width

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


                    //Create an empty Bitmap and do not think this means you have to have a bitmap to resize we will set the format in next line
                    Bitmap resultantImage = new Bitmap(newWidth, newHeight);

                    using (Graphics graphics = Graphics.FromImage(resultantImage))
                    {
                        //set the parameters to maintain the quality
                        graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                        //Draw the new resized image
                        graphics.DrawImage(imageObject, 0, 0, (float)newWidth, (float)newHeight);
                        //And save it   
                        resultantImage.Save(outStream, format);
                    }

                    imageFile.SaveBinary(outStream, false);
                    imageFile.Update();
                    inStream.Close();
                    outStream.Close();
                    
                    base.EventFiringEnabled = enabledEventFiring;
                }
                else
                {
                    SPLogger.WriteInformationToLog("File uploaded not an image", null);
                }
            }
            catch
            {
                throw;
            }
        }


    }
}