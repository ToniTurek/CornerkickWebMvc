﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Threading.Tasks;

using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.S3.Model;

namespace CornerkickWebMvc
{
  public class AmazonS3FileTransfer
  {
    private const string sBucketName = "ckamazonbucket";
    private RegionEndpoint bucketRegion = RegionEndpoint.EUCentral1;
    const string sAccessKey = "AKIAJKOOKK445KJRPY7Q";
    const string sSecretAccessKey = "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";

    public void uploadFile(string sFile, string sKey = null, string sContentType = "text/plain")
    {
#if _NO_UPLOAD
      return;

#endif
      //var credentials = new Amazon.Runtime.StoredProfileAWSCredentials("ckAwsProfile");

      //var client = new AmazonS3Client(bucketRegion);
      //string accessKey = System.Configuration.ConfigurationManager.AppSettings["AWSAccessKey"];
      //string secretAccessKey = System.Configuration.ConfigurationManager.AppSettings["AWSSecretKey"];

      IAmazonS3 client = new AmazonS3Client(sAccessKey, sSecretAccessKey, bucketRegion);

      if (string.IsNullOrEmpty(sKey)) sKey = sFile;

      // Try to delete existing file
      try {
        deleteFile(sKey, client);
      } catch {
      }

      try {
        MvcApplication.ckcore.tl.writeLog("Try to upload file '" + sFile + "' to key '" + sKey + "' (Type: '" + sContentType + "')");

        PutObjectRequest putRequest = new PutObjectRequest {
          BucketName = sBucketName,
          Key = sKey,
          FilePath = sFile,
          ContentType = sContentType,
          CannedACL = S3CannedACL.PublicRead
        };

        try {
         PutObjectResponse response = client.PutObject(putRequest);
          //using (S3Response r = client.PutObject(putRequest)) { }
        } catch (AmazonS3Exception amazonS3Exception) {
          MvcApplication.ckcore.tl.writeLog("ERROR! S3 PutObjectResponse Exception Message: " + amazonS3Exception.Message, MvcApplication.ckcore.sErrorFile);
        }
      } catch (AmazonS3Exception amazonS3Exception) {
        MvcApplication.ckcore.tl.writeLog("ERROR! S3 Exception Message: " + amazonS3Exception.Message, MvcApplication.ckcore.sErrorFile);

        if (amazonS3Exception.ErrorCode != null) {
          MvcApplication.ckcore.tl.writeLog("Error Code: " + amazonS3Exception.ErrorCode, MvcApplication.ckcore.sErrorFile);

          if (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") || amazonS3Exception.ErrorCode.Equals("InvalidSecurity")) {
            MvcApplication.ckcore.tl.writeLog("Check the provided AWS Credentials.", MvcApplication.ckcore.sErrorFile);
          }
        }
      } catch (Exception e) {
        MvcApplication.ckcore.tl.writeLog("ERROR! Exception Message: " + e.Message + " File '" + sFile + "' not uploaded", MvcApplication.ckcore.sErrorFile);
      } catch {
        MvcApplication.ckcore.tl.writeLog("ERROR! Unknown Exception. File '" + sFile + "' not uploaded", MvcApplication.ckcore.sErrorFile);
      }
    }

    private void deleteFile(string sKey, IAmazonS3 client)
    {
#if _NO_UPLOAD
      return;

#endif
      DeleteObjectRequest deleteRequest = new DeleteObjectRequest();
      deleteRequest.BucketName = sBucketName;
      deleteRequest.Key = sKey;
      client.DeleteObject(deleteRequest);
    }

    public void downloadFile(string sKey, string sTargetPath = "./")
    {
      //IAmazonS3 client = new AmazonS3Client(bucketRegion);
      //ReadObjectDataAsync(client, sKey).Wait();
      try {
        TransferUtility fileTransferUtility = new TransferUtility(new AmazonS3Client(sAccessKey, sSecretAccessKey, bucketRegion));

        // 2. Specify object key name explicitly.
        try {
          fileTransferUtility.Download(sTargetPath, sBucketName, sKey);
        } catch (AmazonS3Exception s3Exception) {
          MvcApplication.ckcore.tl.writeLog(s3Exception.Message);
          return;
        }
        MvcApplication.ckcore.tl.writeLog(String.Format("Succesfully downloaded file: {0} from bucket: {1} to location: {2}", sKey, sBucketName, sTargetPath));
      } catch (AmazonS3Exception s3Exception) {
        //Console.WriteLine(s3Exception.Message, s3Exception.InnerException);
        MvcApplication.ckcore.tl.writeLog(s3Exception.Message);
      } catch {
        MvcApplication.ckcore.tl.writeLog(String.Format("Unknown error while downloading file: {0} from bucket: {1} to location: {2}", sKey, sBucketName, sTargetPath));
      }
    }

    public void downloadAllFiles(string sS3SubDir, string sTargetPath = "./", string sStartsWith = null, string sEndsWith = null)
    {
      IAmazonS3 client = new AmazonS3Client(sAccessKey, sSecretAccessKey, bucketRegion);

      ListObjectsRequest request = new ListObjectsRequest();
      request.BucketName = sBucketName;

      do {
        ListObjectsResponse response = client.ListObjects(sBucketName, sS3SubDir);

        // Process response.
        for (int iS3 = 0; iS3 < response.S3Objects.Count; iS3++) {
          if (!string.IsNullOrEmpty(sStartsWith) && !response.S3Objects[iS3].Key.StartsWith(sStartsWith)) continue;
          if (!string.IsNullOrEmpty(sEndsWith)   && !response.S3Objects[iS3].Key.EndsWith  (sEndsWith))   continue;

          downloadFile(response.S3Objects[iS3].Key, Path.Combine(sTargetPath, response.S3Objects[iS3].Key));
        }

        // If response is truncated, set the marker to get the next set of keys
        if (response.IsTruncated) {
          request.Marker = response.NextMarker;
        } else {
          request = null;
        }
      } while (request != null);
    }

    static async Task ReadObjectDataAsync(IAmazonS3 client, string sKey)
    {
      string responseBody = "";
      try {
        GetObjectRequest request = new GetObjectRequest {
          BucketName = sBucketName,
          Key = sKey
        };
        using (GetObjectResponse response = await client.GetObjectAsync(request))
        using (Stream responseStream = response.ResponseStream)
        using (StreamReader reader = new StreamReader(responseStream)) {
          string title = response.Metadata["x-amz-meta-title"]; // Assume you have "title" as medata added to the object.
          string contentType = response.Headers["Content-Type"];
          //Console.WriteLine("Object metadata, Title: {0}", title);
          //Console.WriteLine("Content type: {0}", contentType);

          responseBody = reader.ReadToEnd(); // Now you process the response body.
        }
      } catch (AmazonS3Exception e) {
        Console.WriteLine("Error encountered ***. Message:'{0}' when writing an object", e.Message);
      } catch (Exception e) {
        Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
      }
    }

  }
}