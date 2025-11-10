using Asp.Versioning;
using CabinCrewCareer.Application.Common.Interfaces.Services;
using CabinCrewCareer.Domain.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CabinCrewCareer.Api.Controllers
{
    [Route("api/v{version:apiVersion}/files")]
    [ApiVersion(1)]
    [Authorize]
    public class FilesController(
        IFirebaseStorageService firebaseStorageService,
        ILogger<FilesController> logger) : ApiController(null, logger)
    {
        private readonly IFirebaseStorageService _firebaseStorageService = firebaseStorageService;

        #region Upload Audio File
        /// <summary>
        /// Upload audio file (.mp3 or .wav) to Firebase Storage
        /// </summary>
        /// <param name="file">Audio file to upload</param>
        /// <remarks>
        /// Test endpoint to upload audio file and get Firebase Storage URL
        /// 
        /// Accepted file types:
        /// - .mp3 (audio/mpeg)
        /// - .wav (audio/wav)
        /// 
        /// Max file size: 50 MB
        /// </remarks>
        /// <returns>Public URL of uploaded file</returns>
        /// <response code="200">Ok - File uploaded successfully</response>
        /// <response code="400">Bad Request - Invalid file type or size</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost("upload-audio")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UploadAudioAsync(IFormFile file)
        {
            try
            {
                // Validate file exists
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new Response<string>(
                        code: 400,
                        message: "No file uploaded"));
                }

                // Validate file extension
                var allowedExtensions = new[] { ".mp3", ".wav" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new Response<string>(
                        code: 400,
                        message: "Invalid file type. Only .mp3 and .wav files are allowed"));
                }

                // Validate file size (max 50 MB)
                const long maxFileSize = 50 * 1024 * 1024; // 50 MB
                if (file.Length > maxFileSize)
                {
                    return BadRequest(new Response<string>(
                        code: 400,
                        message: "File size exceeds maximum allowed size of 50 MB"));
                }

                // Upload to Firebase
                using var stream = file.OpenReadStream();
                var fileUrl = await _firebaseStorageService.UploadAudioFileAsync(
                    stream,
                    file.FileName,
                    file.ContentType);

                return Ok(new Response<string>(
                    data: fileUrl,
                    code: 200,
                    message: "File uploaded successfully"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error uploading audio file");
                return StatusCode(500, new Response<string>(
                    code: 500,
                    message: $"Failed to upload file: {ex.Message}"));
            }
        }
        #endregion
    }
}
