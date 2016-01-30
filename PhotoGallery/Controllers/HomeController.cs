using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.IO;
using System.Drawing;
using PhotoGallery.Models;
using System.Drawing.Drawing2D;

namespace PhotoGallery.Controllers
{
    public class HomeController : Controller
    {
        private object m_syncRoot = new object();

        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";

            //ConfigurationManager.AppSettings            

            List<object> albums = new List<object>();
            Uri rootUri = new Uri(Settings.GalleryRoot);

            foreach (DirectoryInfo dir in GetAllGalleryDirs())
            {
                Uri relative = rootUri.MakeRelativeUri(new Uri(dir.FullName));

                albums.Add(new Album
                {
                    Name = dir.Name,
                    Path = relative.ToString()
                });
            }

            ViewBag.Albums = albums;

            return View();
        }

        public ActionResult ViewAlbum(string album)
        {
            album = HttpUtility.UrlDecode(album ?? "");

            ViewBag.Album = album;
            ViewBag.AlbumName = album == "" ? "root" : album;

            string path = Path.Combine(Settings.GalleryRoot, album);

            DirectoryInfo dir = new DirectoryInfo(path);

            ViewBag.PhotosCount = dir.GetFiles()
                .Where(file => Settings.PhotosExtexsions.Contains(file.Extension.ToLower()))                
                .Count();

            return View();
        }
        
        public ActionResult GetPhotos(string album)
        {
            album = HttpUtility.UrlDecode(album ?? "");

            string path = Path.Combine(Settings.GalleryRoot, album);

            DirectoryInfo dir = new DirectoryInfo(path);

            IEnumerable<FileInfo> photoFiles = dir.GetFiles()
                .Where(file => Settings.PhotosExtexsions.Contains(file.Extension.ToLower()));

            List<Photo> photos = new List<Photo>();
            Uri rootUri = new Uri(Settings.GalleryRoot);

            foreach (var photoFile in photoFiles.OrderBy(f => f.Name))
            {
                string photoName = photoFile.Name;
                string thumbFileName = Photo.GetThumbFileNameFor(album, photoName);
                string thumbPath = null;

                //string thumbPhysPath = Path.Combine(VirtualPathUtility.ToAbsolute("~/Thumbs/"), thumbFileName);
                string thumbPhysPath = Server.MapPath(Url.Content(Path.Combine("~/Thumbs/", thumbFileName)));

                if (System.IO.File.Exists(thumbPhysPath))
                {
                    //thumbPath = ResolveServerUrl(VirtualPathUtility.ToAbsolute("~/Thumbs/" + thumbFileName), false);
                    thumbPath = Url.Content(Path.Combine("~/Thumbs/", thumbFileName));
                }
                
                photos.Add(
                    new Photo()
                    {
                        FileName = photoFile.Name,
                        ThumbUri = thumbPath,
                        //OriginalUri = Url.Action("GetImage", new { album = album, photo = photoFile.Name})
                        OriginalUri = Url.Action("GetImageResized", new { album = album, photo = photoFile.Name })
                    });                
            }

            return Json(photos, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetImage(string album, string photo)
        {
            album = HttpUtility.UrlDecode(album ?? "");

            string path = Path.Combine(Settings.GalleryRoot, album, photo);

            return base.File(path, "image/jpeg");
        }

        public ActionResult GetImageResized(string album, string photo)
        {
            album = HttpUtility.UrlDecode(album ?? "");

            string path = Path.Combine(Settings.GalleryRoot, album, photo);

            using (MemoryStream resizedImageStream = new MemoryStream())
            {
                using (Image rawImage = Image.FromFile(path))
                {
                    int targetWidth = rawImage.Width;
                    int targetHeight = rawImage.Height;

                    if (rawImage.Width > Settings.MaxViewWidth || rawImage.Height > Settings.MaxViewHeight)
                    {
                        CalcLimitedSize(rawImage.Width, rawImage.Height,
                            Settings.MaxViewWidth, Settings.MaxViewHeight, out targetWidth, out targetHeight);

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

                            // to "resurrect" stream
                            var resetedStream = new MemoryStream(resizedImageStream.ToArray());

                            return new FileStreamResult(resetedStream, "image/jpeg");

                        }
                    }
                    else // image already with allowed size
                    {
                        return File(path, "image/jpeg");
                    }
                }
            }
        }

        public ActionResult GetThumbFor(string album, string photo)
        {
            album = HttpUtility.UrlDecode(album ?? "");

            string path = Path.Combine(Settings.GalleryRoot, album, photo);
            string thumbFileName = Photo.GetThumbFileNameFor(album, photo);
            string thumbPhysPath = Server.MapPath(Url.Content(Path.Combine("~/Thumbs/", thumbFileName)));

            int targetWidth = Settings.ThumbWidth;
            int targetHeight = Settings.ThumbHeight;

            if (!System.IO.File.Exists(thumbPhysPath))
            {
                // makes thumb calculation synchronous
                lock (m_syncRoot)
                {
                    // create thumb
                    using (Image image = Image.FromFile(path))
                    {
                        int thumbWidth;
                        int thumbHeight;

                        CalcLimitedSize(image.Width, image.Height, targetWidth, targetHeight, out thumbWidth, out thumbHeight);

                        using (Bitmap newImage = new Bitmap(thumbWidth, thumbHeight))
                        {
                            using (Graphics gr = Graphics.FromImage(newImage))
                            {
                                gr.SmoothingMode = SmoothingMode.HighQuality;
                                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                gr.DrawImage(image, new Rectangle(0, 0, thumbWidth, thumbHeight));
                            }

                            newImage.Save(thumbPhysPath);
                        }

                        //using (Image thumb = image.GetThumbnailImage(thumbWidth, thumbHeight, () => false, IntPtr.Zero))
                        //{
                        //    thumb.Save(thumbPhysPath);
                        //}
                    }
                }
            }

            return Json(new
                {
                    //File = path,
                    ThumbUri = Url.Content(Path.Combine("~/Thumbs/", thumbFileName))
                }, JsonRequestBehavior.AllowGet);
        }

        private void CalcLimitedSize(int width, int height, int maxWidth, int maxHeight, out int newWidth, out int newHeight)
        {            
            float aspect = width / (float)height;

            if ((width / (float)maxWidth) > (height / (float)maxHeight))
            {
                newWidth = maxWidth;
                newHeight = (int)(maxWidth / aspect);
            }
            else
            {
                newHeight = maxHeight;
                newWidth = (int)(maxHeight * aspect);
            }
        }
        private IEnumerable<DirectoryInfo> GetAllGalleryDirs()
        {
            DirectoryInfo root = new DirectoryInfo(Settings.GalleryRoot);
            
            List<DirectoryInfo> allDirs = new List<DirectoryInfo>(GetAllSubDirsFor(root));

            allDirs.Insert(0, root);

            return allDirs;
        }
        private IEnumerable<DirectoryInfo> GetAllSubDirsFor(DirectoryInfo root)
        {
            List<DirectoryInfo> dirs = new List<DirectoryInfo>();

            foreach (var subDir in root.GetDirectories().Where(dir => !dir.Attributes.HasFlag(FileAttributes.Hidden)))
            {
                dirs.Add(subDir);

                dirs.AddRange(GetAllSubDirsFor(subDir));
            }

            return dirs;
        }

        private static string ResolveServerUrl(string serverUrl, bool forceHttps)
        {
            if (serverUrl.IndexOf("://") > -1)
                return serverUrl;

            string newUrl = serverUrl;
            Uri originalUri = System.Web.HttpContext.Current.Request.Url;
            newUrl = (forceHttps ? "https" : originalUri.Scheme) +
                "://" + originalUri.Authority + newUrl;
            return newUrl;
        } 
    }
}
