using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;

using System.Configuration;

using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace CornerkickWebMvc.Controllers
{
  public class BlobsController : Controller
  {
    // GET: Blobs
    public ActionResult Index()
    {
        return View();
    }

    private CloudBlobContainer GetCloudBlobContainer()
    {
      /*
      CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            ConfigurationManager.ConnectionStrings["rTKZvM7yRqHAmidkJqgg50D9KyzWamiZdsJwVEIyhluwluRph+hSfQxVVKac++JSZm69hoGXy+zrz364dSC1WQ=="].ConnectionString);
            */
      string sCcmSetting = CloudConfigurationManager.GetSetting("ckstored");
      CloudStorageAccount storageAccount = CloudStorageAccount.Parse(sCcmSetting);
      CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
      //CloudBlobContainer container = blobClient.GetContainerReference("ckBlobContainer");
      CloudBlobContainer container = blobClient.GetContainerReference("test-blob-container");
      return container;
    }

    public ActionResult CreateBlobContainer()
    {
      CloudBlobContainer container = GetCloudBlobContainer();
      ViewBag.Success = container.CreateIfNotExists();
      ViewBag.BlobContainerName = container.Name;

      return View();
    }

    public bool uploadBlob(string sBlob, string sFile)
    {
      if (!System.IO.File.Exists(sFile)) return false;

      CloudBlobContainer container = GetCloudBlobContainer();
      CloudBlockBlob blob = container.GetBlockBlobReference(sBlob);

      using (var fileStream = System.IO.File.OpenRead(sFile)) {
        blob.UploadFromStream(fileStream);
      }
      return true;
    }

    public bool downloadBlob(string sBlob, string sFile)
    {
      /*
      CloudBlobContainer container = GetCloudBlobContainer();
      CloudBlockBlob blockBlob = container.GetBlockBlobReference(sBlob);
      blockBlob.StartCopy(sFile);
      */

      try {
        CloudBlobContainer container = GetCloudBlobContainer();
        CloudBlockBlob blob = container.GetBlockBlobReference(sBlob);
        blob.DownloadToFile(sFile, FileMode.Create);
      } catch {
      }

      return true;
    }

    public ActionResult ListBlobs()
    {
      CloudBlobContainer container = GetCloudBlobContainer();
      List<string> blobs = new List<string>();
      foreach (IListBlobItem item in container.ListBlobs(useFlatBlobListing: false)) {
        if (item.GetType() == typeof(CloudBlockBlob)) {
          CloudBlockBlob blob = (CloudBlockBlob)item;
          string sBlob = blob.Name;
          foreach (var meta in blob.Metadata) {
            sBlob += "\n  " + meta.ToString();
          }
          blobs.Add(sBlob);
        } else if (item.GetType() == typeof(CloudPageBlob)) {
          CloudPageBlob blob = (CloudPageBlob)item;
          blobs.Add(blob.Name);
        } else if (item.GetType() == typeof(CloudBlobDirectory)) {
          CloudBlobDirectory dir = (CloudBlobDirectory)item;
          blobs.Add(dir.Uri.ToString());
        }
      }

      return View(blobs);
    }

  }
}