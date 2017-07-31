using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace OpenSourceCooking
{
    public static class AzureCloudStorageWrapper
    {
        public enum AzureBlobContainer
        {
            //These have to be lowercase because Azure containers have to be lowercase
            ingredientcloudfiles,
            recipecloudfiles
        }

        static readonly string[] AllowedFileMimeTypes = 
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "video/mp4"
        };

        static CloudStorageAccount CloudStorageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=opensourcecookingstorage;AccountKey=BdVuetWWU5asQtQBRUBwSJZXLDdjNEPu/HlUpVDCVE1MLI+jZfG+G3tqRy3rHd14sr9ClcEM57zGZ9Fpba4Zkw==;EndpointSuffix=core.windows.net");
        static OpenSourceCookingEntities db = new OpenSourceCookingEntities();

        static string GetCloucdBlockBlobName(string CloudFileUrl)
        {
            return CloudFileUrl.Substring(CloudFileUrl.LastIndexOf(AzureBlobContainer.recipecloudfiles.ToString() + "/", StringComparison.OrdinalIgnoreCase) + AzureBlobContainer.recipecloudfiles.ToString().Count() + 1);//Get the block blob name which is after the last '/' Example URL: //https://opensourcecookingstorage.blob.core.windows.net/recipecloudfiles/15b4f032-4158-4d0b-bd14-ad207fce8b1d%202017-04-07%2001%3A12%3A41.78IMG_2006.JPG
        }

        public static void DeleteCloudFile(CloudFile cloudFile)
        {
            if (cloudFile == null)
                throw new ArgumentNullException("cloudFile");
            string CloudBlockBlobName;
            if (cloudFile.CloudFilesThumbnail != null)
            {
                CloudBlockBlobName = GetCloucdBlockBlobName(cloudFile.CloudFilesThumbnail.Url);
                DeleteCloudFile(AzureBlobContainer.recipecloudfiles, CloudBlockBlobName);
            }
            CloudBlockBlobName = GetCloucdBlockBlobName(cloudFile.Url);
            DeleteCloudFile(AzureBlobContainer.recipecloudfiles, CloudBlockBlobName);
            return;

        }
        public static bool DeleteCloudFile(AzureBlobContainer azureBlobContainers, string blockBlobName)
        {
            CloudBlobClient CloudBlobClient = CloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer CloudBlobContainer = CloudBlobClient.GetContainerReference(azureBlobContainers.ToString());
            CloudBlockBlob cloudBlockBlob = CloudBlobContainer.GetBlockBlobReference(blockBlobName);
            return cloudBlockBlob.DeleteIfExists();
        }
        
        public static bool DeleteIngredientCloudFileIfExist(Ingredient ingredient)
        {
            if (ingredient == null)
                throw new ArgumentNullException("ingredient");
            if (ingredient.CloudFile != null)
            {
                string CloucdBlockBlobName = ingredient.CloudFile.Url.Substring(ingredient.CloudFile.Url.LastIndexOf('/') + 1);//Get the block blob name which is after the last '/' Example URL: //https://opensourcecookingstorage.blob.core.windows.net/ingredientcloudfiles/15b4f032-4158-4d0b-bd14-ad207fce8b1d%202017-04-07%2001%3A12%3A41.78IMG_2006.JPG
                if (DeleteCloudFile(AzureBlobContainer.ingredientcloudfiles, CloucdBlockBlobName))
                    return true;
                else
                    //TODO:
                    //Should Log that a Cloud file didnt delete 
                    //And if it doesnt hurt performance too bad, just run an async task to delete all cloud files that are not contained in the DB
                    return false;
            }
            return false;
        }

        public static void DeleteRecipeCloudFilesIfExist(Recipe recipe)
        {
            if (recipe == null)
                throw new ArgumentNullException("recipe");
            List<CloudFile> CloudFiles = recipe.RecipeCloudFiles.Where(x => x.RecipeCloudFileTypeName == "MainImage" || x.RecipeCloudFileTypeName == "MainVideo").Select(x=>x.CloudFile).ToList();
            CloudFiles.AddRange(recipe.RecipeSteps.SelectMany(x => x.RecipeStepsCloudFiles).Select(y => y.CloudFile).ToList());
            foreach (CloudFile CloudFile in CloudFiles)
            {
                if (CloudFile == null)
                    continue;
                DeleteCloudFile(CloudFile);
            }
            return;
        }

        public async static Task<bool> DeleteUnreferencedCloudFilesIfExist(AzureBlobContainer azureBlobContainers, IReadOnlyCollection<string> cloudFileUrlsToKeep)
        {
            CloudBlobClient CloudBlobClient = CloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer CloudBlobContainer = CloudBlobClient.GetContainerReference(azureBlobContainers.ToString());
            foreach (IListBlobItem IListBlobItem in CloudBlobContainer.ListBlobs(null, true))
            {
                if (IListBlobItem.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob CloudBlockBlob = (CloudBlockBlob)IListBlobItem;
                    if (cloudFileUrlsToKeep.Contains(CloudBlockBlob.Uri.AbsoluteUri))
                        continue;
                    await CloudBlockBlob.DeleteIfExistsAsync();
                }
                else if (IListBlobItem.GetType() == typeof(CloudPageBlob))
                {
                    CloudPageBlob CloudPageBlob = (CloudPageBlob)IListBlobItem;
                    if (cloudFileUrlsToKeep.Contains(CloudPageBlob.Uri.AbsoluteUri))
                        continue;
                    await CloudPageBlob.DeleteIfExistsAsync();
                }
            }
            return true;
        }

        public static bool IsFileTypeAllowed(string mimeType)
        {
            
            if (AllowedFileMimeTypes.Contains(mimeType))
                return true;
            return false;
        }

        public static async Task<int> UploadCloudFile(AzureBlobContainer azureBlobContainers, HttpPostedFileBase httpPostedFileBase, string aspNetUserId, string fileName, bool createThumbnail)
        {
            if (httpPostedFileBase == null)
                throw new ArgumentNullException("httpPostedFileBase");
            if (!IsFileTypeAllowed(httpPostedFileBase.ContentType))
                throw new ArgumentException("Invalid httpPostedFileBase ContentType");
            if (httpPostedFileBase.ContentLength > 31457280) //30MB            
                return -1;//File size too big
            if (String.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("httpPostedFileBase");
            if (fileName.Length > 256) //30MB            
                throw new ArgumentException("File name is too long");
            CloudBlobClient blobClient = CloudStorageAccount.CreateCloudBlobClient();
            var properties = blobClient.GetServiceProperties();
            if (String.IsNullOrEmpty(properties.DefaultServiceVersion))
            {
                //set the version of storage to latest
                properties.DefaultServiceVersion = "2015-04-05";
                blobClient.SetServiceProperties(properties);
            }
            CloudBlobContainer CloudBlobContainer = blobClient.GetContainerReference(azureBlobContainers.ToString());
            CloudBlobContainer.CreateIfNotExists();
            CloudBlobContainer.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            string FileExtension = Path.GetExtension(httpPostedFileBase.FileName).Remove(0, 1)/*Removes the first period*/.ToLower(CultureInfo.InvariantCulture);
            string blockBlobName = (fileName + "-" + FileExtension + "-" + aspNetUserId + "-" + DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss-ff", CultureInfo.InvariantCulture));
            CloudBlockBlob CloudBlockBlob = CloudBlobContainer.GetBlockBlobReference(blockBlobName + "." + FileExtension); //Azure Blob names cant be over 256 chars
            using (Stream MemoryStream = new MemoryStream())
            {
                httpPostedFileBase.InputStream.Seek(0, SeekOrigin.Begin);
                await httpPostedFileBase.InputStream.CopyToAsync(MemoryStream);
                MemoryStream.Seek(0, SeekOrigin.Begin);
                CloudBlockBlob.UploadFromStream(MemoryStream);
            }
            string URL = CloudBlockBlob.Uri.ToString();
            CloudFile UploadedCloudFile = new CloudFile() { Url = URL, FileExtension = FileExtension, CreatorId = aspNetUserId };
            db.CloudFiles.Add(UploadedCloudFile);

            if (createThumbnail)
            {
                CloudBlockBlob ThumbnailCloudBlockBlob = CloudBlobContainer.GetBlockBlobReference(blockBlobName + "-Thumb.jpeg"); //Azure Blob names cant be over 256 chars
                using (Stream MemoryStream = new MemoryStream())
                {
                    httpPostedFileBase.InputStream.Seek(0, SeekOrigin.Begin);
                    await httpPostedFileBase.InputStream.CopyToAsync(MemoryStream);
                    MemoryStream.Seek(0, SeekOrigin.Begin);
                    Image Image = Image.FromStream(MemoryStream, true, true);
                    if (Image.Width > 480 || Image.Height > 480)                    
                        Image = ScaleImage(Image, 480, 480);
                    using (Stream ThumbnailMemoryStream = new MemoryStream())
                    {
                        Image.Save(ThumbnailMemoryStream, ImageFormat.Jpeg);
                        ThumbnailMemoryStream.Seek(0, SeekOrigin.Begin);
                        ThumbnailCloudBlockBlob.UploadFromStream(ThumbnailMemoryStream);
                    }
                    string ThumbnailUrl = ThumbnailCloudBlockBlob.Uri.ToString();
                    db.CloudFilesThumbnails.Add(new CloudFilesThumbnail() { CloudFileId = UploadedCloudFile.Id, Url = ThumbnailUrl, FileExtension = "jpeg" });
                }
            }
            db.SaveChanges();
            return UploadedCloudFile.Id;
        }

        public static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            if (image == null)
                throw new ArgumentNullException("image");
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);
            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);
            var newImage = new Bitmap(newWidth, newHeight);
            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            return newImage;            
        }
    }
}