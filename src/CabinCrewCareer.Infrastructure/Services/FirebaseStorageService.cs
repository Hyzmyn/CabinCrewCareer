using CabinCrewCareer.Application.Common.Interfaces.Services;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;

namespace CabinCrewCareer.Infrastructure.Services
{
    public class FirebaseStorageService : IFirebaseStorageService
    {
        private readonly string _bucketName;
        private readonly StorageClient _storageClient;

        public FirebaseStorageService(IConfiguration configuration)
        {
            // Initialize Firebase Admin SDK if not already initialized
            if (FirebaseApp.DefaultInstance == null)
            {
                var firebaseConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "firebase-adminsdk.json");
                
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(firebaseConfigPath),
                    ProjectId = "cabincrew-98e06"
                });
            }

            // Initialize Storage Client
            var credential = GoogleCredential.FromFile(
                Path.Combine(Directory.GetCurrentDirectory(), "firebase-adminsdk.json"));
            
            _storageClient = StorageClient.Create(credential);
            
            // Get bucket name from config
            // Format: gs://{bucket-name} or just {bucket-name}
            var storageBucket = configuration["Firebase:StorageBucket"] ?? "cabincrew-98e06.appspot.com";
            _bucketName = storageBucket.Replace("gs://", "");
            
            // Try to create bucket if it doesn't exist
            try
            {
                _storageClient.GetBucket(_bucketName);
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Bucket doesn't exist - create it
                try
                {
                    var newBucket = new Google.Apis.Storage.v1.Data.Bucket
                    {
                        Name = _bucketName,
                        Location = "ASIA"
                    };
                    _storageClient.CreateBucket("cabincrew-98e06", newBucket);
                }
                catch
                {
                    // If creation fails, it might be a permission issue
                    // User needs to create bucket manually in Firebase Console
                }
            }
        }

        public async Task<string> UploadAudioFileAsync(Stream fileStream, string fileName, string contentType)
        {
            try
            {
                // Generate unique filename with timestamp
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var uniqueFileName = $"audio-tests/{timestamp}_{fileName}";

                // Upload to Firebase Storage
                var storageObject = await _storageClient.UploadObjectAsync(
                    bucket: _bucketName,
                    objectName: uniqueFileName,
                    contentType: contentType,
                    source: fileStream,
                    options: new UploadObjectOptions
                    {
                        PredefinedAcl = PredefinedObjectAcl.PublicRead
                    });

                // Return public URL
                var publicUrl = $"https://storage.googleapis.com/{_bucketName}/{uniqueFileName}";
                
                return publicUrl;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upload file to Firebase Storage: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                // Extract object name from URL
                var uri = new Uri(fileUrl);
                var objectName = uri.AbsolutePath.Split(new[] { _bucketName + "/" }, StringSplitOptions.None).Last();

                await _storageClient.DeleteObjectAsync(_bucketName, objectName);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
