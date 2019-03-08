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

    public void uploadFile(string sFile, string sKey = null, string sContentType = "text/plain")
    {
      var client = new AmazonS3Client(bucketRegion);

      if (string.IsNullOrEmpty(sKey)) sKey = sFile;

      try {
        PutObjectRequest putRequest = new PutObjectRequest {
          BucketName = sBucketName,
          Key = sKey,
          FilePath = sFile,
          ContentType = sContentType
        };

        PutObjectResponse response = client.PutObject(putRequest);
      } catch (AmazonS3Exception amazonS3Exception) {
        MvcApplication.ckcore.tl.writeLog("ERROR! S3 Exception Message: " + amazonS3Exception.Message, MvcApplication.ckcore.sErrorFile);

        if (amazonS3Exception.ErrorCode != null) {
          MvcApplication.ckcore.tl.writeLog("Error Code: " + amazonS3Exception.ErrorCode, MvcApplication.ckcore.sErrorFile);

          if (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") || amazonS3Exception.ErrorCode.Equals("InvalidSecurity")) {
            MvcApplication.ckcore.tl.writeLog("Check the provided AWS Credentials.", MvcApplication.ckcore.sErrorFile);
          }
        }
      }
    }

    public void downloadFile(string sKey, string sTargetPath = "./")
    {
      //IAmazonS3 client = new AmazonS3Client(bucketRegion);
      //ReadObjectDataAsync(client, sKey).Wait();
      try {
        TransferUtility fileTransferUtility = new TransferUtility(new AmazonS3Client(bucketRegion));

        // 2. Specify object key name explicitly.
        fileTransferUtility.Download(sTargetPath, sBucketName, sKey);
        MvcApplication.ckcore.tl.writeLog(String.Format("Succesfully downloaded file: {0} from bucket: {1} to location: {2}", sKey, sBucketName, sTargetPath));
      } catch (AmazonS3Exception s3Exception) {
        //Console.WriteLine(s3Exception.Message, s3Exception.InnerException);
        MvcApplication.ckcore.tl.writeLog(s3Exception.Message);
      } catch {
        MvcApplication.ckcore.tl.writeLog(String.Format("Unknown error while downloading file: {0} from bucket: {1} to location: {2}", sKey, sBucketName, sTargetPath));
      }
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