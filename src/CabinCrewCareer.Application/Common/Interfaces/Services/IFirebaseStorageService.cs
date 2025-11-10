namespace CabinCrewCareer.Application.Common.Interfaces.Services
{
    public interface IFirebaseStorageService
    {
        /// <summary>
        /// Upload audio file to Firebase Storage
        /// </summary>
        /// <param name="file">Audio file (mp3/wav)</param>
        /// <param name="folder">Folder path in Firebase Storage</param>
        /// <returns>Public URL of uploaded file</returns>
        Task<string> UploadAudioFileAsync(Stream fileStream, string fileName, string contentType);
        
        /// <summary>
        /// Delete file from Firebase Storage
        /// </summary>
        Task<bool> DeleteFileAsync(string fileUrl);
    }
}
