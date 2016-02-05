using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalery2.Core.Implementation
{
    public class ThumbnailGenerator
    {
        public Stream GenerateThumbinail(Stream origImageStream, Size maxSize, out Size resultSize)
        {
            var resizedImageStream = new MemoryStream();
            using (Image rawImage = Image.FromStream(origImageStream))
            {
                int targetWidth = rawImage.Width;
                int targetHeight = rawImage.Height;

                if (rawImage.Width > maxSize.Width || rawImage.Height > maxSize.Height)
                {
                    CalcTargetSize(rawImage.Width, rawImage.Height, maxSize,
                        out targetWidth, out targetHeight);

                    using (Bitmap image = new Bitmap(targetWidth, targetHeight))
                    {
                        using (Graphics gr = Graphics.FromImage(image))
                        {
                            //gr.SmoothingMode = SmoothingMode.HighQuality;
                            //gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            //gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            gr.DrawImage(rawImage, new Rectangle(0, 0, targetWidth, targetHeight));
                        }

                        image.Save(resizedImageStream, System.Drawing.Imaging.ImageFormat.Jpeg);

                        resizedImageStream.Position = 0;

                        resultSize = new Size(targetWidth, targetHeight);

                        return resizedImageStream;
                    }
                }
                else // image already with allowed size
                {
                    origImageStream.Seek(0, SeekOrigin.Begin);
                    origImageStream.CopyTo(resizedImageStream);

                    resizedImageStream.Seek(0, SeekOrigin.Begin);

                    resultSize = new Size(rawImage.Width, rawImage.Height);

                    return resizedImageStream;
                }
            }
        }

        private void CalcTargetSize(int width, int height, Size maxSize, out int newWidth, out int newHeight)
        {
            float aspect = width / (float)height;

            if ((width / (float)maxSize.Width) > (height / (float)maxSize.Height))
            {
                newWidth = maxSize.Width;
                newHeight = (int)(maxSize.Width / aspect);
            }
            else
            {
                newHeight = maxSize.Height;
                newWidth = (int)(maxSize.Height * aspect);
            }
        }
    }
}
