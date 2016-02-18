using Common.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalery2.Core.Implementation
{
    internal class ImageMethods
    {
        private static ILog _log = LogManager.GetLogger<ImageMethods>();

        public static Stream GenerateThumbinail(string path, Size maxSize, out Size resultSize)
        {
            _log.Debug(x => x("generating thumbnail for '{0}', max size={1}", path, maxSize));

            using (var fileStream = new System.IO.FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return GenerateThumbinail(fileStream, maxSize, out resultSize);
            }
        }

        private static Stream GenerateThumbinail(Stream origImageStream, Size maxSize, out Size resultSize)
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

        public static BasicMetadata GetBasicMetadata(string path)
        {
            _log.Debug(x => x("populating basic metadata for '{0}'", path));

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return GetBasicMetadata(stream);
            }

            _log.Debug(x => x("populated basic metadata for '{0}'", path));
        }

        private static BasicMetadata GetBasicMetadata(Stream imageStream)
        {
            // trying to make image load faster
            // http://stackoverflow.com/questions/552467/how-do-i-reliably-get-an-image-dimensions-in-net-without-loading-the-image
            using (Image rawImage = Image.FromStream(imageStream, false, false))
            {
                return new BasicMetadata()
                {
                    OrigSize = new Size(rawImage.Width, rawImage.Height),
                };
            }
        }

        private static void CalcTargetSize(int width, int height, Size maxSize, out int newWidth, out int newHeight)
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
