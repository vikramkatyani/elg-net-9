using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ELG.DAL.DBEntity;
using System.Xml.Linq;

namespace ELG.DAL.Utilities
{
    /// <summary>
    /// Utility class for Azure Blob Storage operations
    /// Handles SCORM package uploads and file management
    /// </summary>
    public class AzureStorageUtility
    {
        private readonly string _connectionString;
        private readonly string _containerName;
        private readonly string _thumbnailContainerName;
        private readonly BlobContainerClient _containerClient;
        private readonly BlobContainerClient _thumbnailContainerClient;

        public AzureStorageUtility(string connectionString, string containerName = "elg-learn", string thumbnailContainerName = "elg-content")
        {
            _connectionString = connectionString;
            _containerName = containerName;
            _thumbnailContainerName = thumbnailContainerName;
            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            // Thumbnails are stored in a separate container
            _thumbnailContainerClient = blobServiceClient.GetBlobContainerClient(thumbnailContainerName);
            try
            {
                _thumbnailContainerClient.CreateIfNotExists();
            }
            catch (Exception)
            {
                // ignore create errors if container already exists or policy forbids
            }
        }

        /// <summary>
        /// Upload SCORM package to Azure storage
        /// Paths:
        ///  - SCORM content: {containerName}/{companyNumber}/course/{uniqueCourseId}/{filename}
        ///  - Thumbnail: leg-content/thumbnails/thumbnail_{uniqueCourseId}.{ext}
        /// </summary>
        public async Task<bool> UploadScormPackageAsync(IFormFile zipFile, IFormFile thumbnailFile, 
            string companyNumber, string courseId, string courseTitle, string coursePath)
        {
            try
            {
                if (string.IsNullOrEmpty(companyNumber) || string.IsNullOrEmpty(courseId))
                    return false;

                // Create folder structure for SCORM content: companyNumber/course/courseId/
                string courseFolderPath = $"{companyNumber}/course/{courseId}";

                // Upload SCORM package (ZIP file)
                bool zipOk = true;
                if (zipFile != null && zipFile.Length > 0)
                {
                    string zipBlobName = $"{courseFolderPath}/{zipFile.FileName}";
                    zipOk = await UploadFileToBlobAsync(zipFile, zipBlobName);
                    if (!zipOk)
                        return false;
                }

                // Upload thumbnail image
                bool thumbOk = true;
                if (thumbnailFile != null && thumbnailFile.Length > 0)
                {
                    string extension = Path.GetExtension(thumbnailFile.FileName);
                    extension = string.IsNullOrWhiteSpace(extension) ? string.Empty : extension;
                    string thumbBlobName = $"thumbnails/thumbnail_{courseId}{extension}";
                    thumbOk = await UploadFileToBlobAsync(thumbnailFile, thumbBlobName, useThumbnailContainer: true);

                    // Verify thumbnail exists
                    if (thumbOk)
                    {
                        BlobClient thumbClient = _thumbnailContainerClient.GetBlobClient(thumbBlobName);
                        var exists = await thumbClient.ExistsAsync();
                        if (!exists.Value)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

                return zipOk && thumbOk;
            }
            catch (Exception)
            {
                // Silent error handling - errors are logged by individual upload methods
                return false;
            }
        }

        /// <summary>
        /// Upload file to Azure Blob Storage
        /// </summary>
        private async Task<bool> UploadFileToBlobAsync(IFormFile file, string blobName, bool useThumbnailContainer = false)
        {
            try
            {
                BlobClient blobClient = useThumbnailContainer
                    ? _thumbnailContainerClient.GetBlobClient(blobName)
                    : _containerClient.GetBlobClient(blobName);
                
                using (Stream uploadFileStream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(uploadFileStream, overwrite: true);
                }

                return true;
            }
            catch (Exception)
            {
                // Error is logged in the service layer
                return false;
            }
        }

        /// <summary>
        /// Extract SCORM package and find start path (typically imsmanifest.xml or index.html)
        /// </summary>
        public async Task<string> ExtractScormPackageAsync(IFormFile zipFile, string companyNumber, string courseId)
        {
            try
            {
                if (zipFile == null || zipFile.Length == 0)
                    return string.Empty;

                string tempExtractPath = Path.Combine(Path.GetTempPath(), $"scorm_{courseId}_{Guid.NewGuid()}");
                Directory.CreateDirectory(tempExtractPath);

                // Extract ZIP file to temp location
                using (var archive = new ZipArchive(zipFile.OpenReadStream(), ZipArchiveMode.Read))
                {
                    archive.ExtractToDirectory(tempExtractPath);
                }

                // Find SCORM manifest or index file
                string startPath = FindScormStartPath(tempExtractPath);

                // Upload extracted contents to Azure
                await UploadDirectoryToAzureAsync(tempExtractPath, companyNumber, courseId);

                // Cleanup temp directory
                if (Directory.Exists(tempExtractPath))
                {
                    Directory.Delete(tempExtractPath, true);
                }

                return startPath;
            }
            catch (Exception)
            {
                // Error is logged in the service layer
                return string.Empty;
            }
        }

        /// <summary>
        /// Find the start path for SCORM course (typically imsmanifest.xml or index.html)
        /// </summary>
        private string FindScormStartPath(string extractedPath)
        {
            try
            {
                // Look for imsmanifest.xml (standard SCORM manifest)
                var manifestFile = Directory.GetFiles(extractedPath, "imsmanifest.xml", SearchOption.AllDirectories).FirstOrDefault();
                if (!string.IsNullOrEmpty(manifestFile))
                {
                    // Parse manifest to find launch href of default organization/resource
                    try
                    {
                        var xdoc = XDocument.Load(manifestFile);
                        var root = xdoc.Root;
                        if (root != null)
                        {
                            // Find organizations and default identifier
                            var orgs = root.Descendants().Where(e => e.Name.LocalName == "organizations").FirstOrDefault();
                            string defaultOrgId = orgs?.Attribute("default")?.Value;
                            var org = (defaultOrgId != null)
                                ? orgs?.Descendants().FirstOrDefault(e => e.Name.LocalName == "organization" && (string)e.Attribute("identifier") == defaultOrgId)
                                : orgs?.Descendants().FirstOrDefault(e => e.Name.LocalName == "organization");

                            // Get first item and identifierref
                            var item = org?.Descendants().FirstOrDefault(e => e.Name.LocalName == "item" && e.Attribute("identifierref") != null)
                                       ?? org?.Descendants().FirstOrDefault(e => e.Name.LocalName == "item");
                            string idRef = item?.Attribute("identifierref")?.Value;

                            // Find matching resource href; fallback to any resource href
                            var resources = root.Descendants().Where(e => e.Name.LocalName == "resource");
                            var res = (idRef != null)
                                ? resources.FirstOrDefault(e => (string)e.Attribute("identifier") == idRef)
                                : resources.FirstOrDefault();
                            string href = res?.Attribute("href")?.Value;

                            if (!string.IsNullOrEmpty(href))
                            {
                                var manifestDir = Path.GetDirectoryName(manifestFile) ?? extractedPath;
                                var combined = Path.Combine(manifestDir, href);
                                // Compute relative path from extracted root
                                string rel = combined.Replace(extractedPath, "").TrimStart('\\', '/');
                                return rel.Replace('\\', '/');
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignore manifest parse errors and fall back below
                    }
                }

                // Fallback to index.html anywhere, return file path (not just directory)
                var indexFile = Directory.GetFiles(extractedPath, "index.html", SearchOption.AllDirectories).FirstOrDefault();
                if (!string.IsNullOrEmpty(indexFile))
                {
                    string rel = indexFile.Replace(extractedPath, "").TrimStart('\\', '/');
                    return rel.Replace('\\', '/');
                }

                // If no standard entry point found, return root
                return "";
            }
            catch (Exception)
            {
                // Error is logged in the service layer
                return string.Empty;
            }
        }

        /// <summary>
        /// Recursively upload directory contents to Azure Blob Storage
        /// </summary>
        private async Task UploadDirectoryToAzureAsync(string directoryPath, string companyNumber, string courseId)
        {
            try
            {
                string folderPath = $"{companyNumber}/course/{courseId}";
                var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);

                foreach (var filePath in files)
                {
                    string relativePath = filePath.Substring(directoryPath.Length).TrimStart('\\', '/');
                    string blobName = $"{folderPath}/{relativePath}";

                    using (FileStream fileStream = File.OpenRead(filePath))
                    {
                        BlobClient blobClient = _containerClient.GetBlobClient(blobName);
                        
                        // Set proper content type and disposition for web viewing
                        var blobHttpHeaders = new BlobHttpHeaders
                        {
                            ContentType = GetContentType(filePath),
                            ContentDisposition = "inline" // Force inline viewing instead of download
                        };
                        
                        var uploadOptions = new BlobUploadOptions
                        {
                            HttpHeaders = blobHttpHeaders
                        };
                        
                        await blobClient.UploadAsync(fileStream, uploadOptions);
                    }
                }
            }
            catch (Exception)
            {
                // Error is logged in the service layer
            }
        }

        /// <summary>
        /// Check if a blob exists
        /// </summary>
        public async Task<bool> BlobExistsAsync(string blobPath, bool isThumbnail = false)
        {
            try
            {
                var containerClient = isThumbnail ? _thumbnailContainerClient : _containerClient;
                // Support full URL paths
                string blobName = ExtractBlobName(blobPath, isThumbnail);
                var blobClient = containerClient.GetBlobClient(blobName);
                var exists = await blobClient.ExistsAsync();
                return exists.Value;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Upload text content as a blob
        /// </summary>
        public async Task<bool> UploadTextAsync(string blobPath, string content, bool isThumbnail = false, string contentType = "application/javascript")
        {
            try
            {
                var containerClient = isThumbnail ? _thumbnailContainerClient : _containerClient;
                string blobName = ExtractBlobName(blobPath, isThumbnail);
                var blobClient = containerClient.GetBlobClient(blobName);

                using (var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)))
                {
                    var headers = new BlobHttpHeaders
                    {
                        ContentType = contentType,
                        ContentDisposition = "inline"
                    };
                    var options = new BlobUploadOptions { HttpHeaders = headers };
                    await blobClient.UploadAsync(ms, options);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Download a blob stream for proxying content to the LMS without exposing SAS per resource.
        /// </summary>
        public async Task<Stream> DownloadBlobAsync(string blobPath, bool isThumbnail = false)
        {
            try
            {
                var containerClient = isThumbnail ? _thumbnailContainerClient : _containerClient;
                string blobName = ExtractBlobName(blobPath, isThumbnail);
                var blobClient = containerClient.GetBlobClient(blobName);
                var exists = await blobClient.ExistsAsync();
                if (!exists.Value)
                {
                    return null;
                }
                var response = await blobClient.DownloadStreamingAsync();
                return response.Value.Content;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Extract blob name from full URL or return as-is if already a relative path
        /// Handles Azure blob URLs with or without SAS tokens
        /// </summary>
        public string ExtractBlobName(string blobPath, bool isThumbnail)
        {
            try
            {
                var containerName = isThumbnail ? _thumbnailContainerName : _containerName;
                
                // Handle full Azure blob URIs with SAS tokens
                if (Uri.TryCreate(blobPath, UriKind.Absolute, out var blobUri))
                {
                    var pathOnly = blobUri.AbsolutePath.Trim('/');
                    // Path format: "container/blob/path/file.ext"
                    var parts = pathOnly.Split('/', 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        // Return blob path without container
                        return parts[1];
                    }
                }
                
                // Handle relative paths - strip container prefix if present and any query strings
                var qIdx = blobPath.IndexOf('?', StringComparison.Ordinal);
                var clean = qIdx > -1 ? blobPath.Substring(0, qIdx) : blobPath;
                clean = clean.TrimStart('/');
                
                if (clean.StartsWith(containerName + "/", StringComparison.OrdinalIgnoreCase))
                {
                    clean = clean.Substring(containerName.Length + 1);
                }
                
                return clean;
            }
            catch
            {
                return blobPath;
            }
        }

        /// <summary>
        /// Get MIME content type based on file extension
        /// </summary>
        public string GetContentType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".html" or ".htm" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                ".pdf" => "application/pdf",
                ".mp4" => "video/mp4",
                ".mp3" => "audio/mpeg",
                ".woff" => "font/woff",
                ".woff2" => "font/woff2",
                ".ttf" => "font/ttf",
                ".eot" => "application/vnd.ms-fontobject",
                ".swf" => "application/x-shockwave-flash",
                _ => "application/octet-stream"
            };
        }

        /// <summary>
        /// Delete SCORM package folder from Azure
        /// </summary>
        public async Task<bool> DeleteScormPackageAsync(string companyNumber, string courseId)
        {
            try
            {
                string folderPath = $"{companyNumber}/course/{courseId}";
                
                // Find all blobs with this prefix and delete them
                await foreach (var blobItem in _containerClient.GetBlobsAsync(prefix: folderPath))
                {
                    try
                    {
                        await _containerClient.DeleteBlobAsync(blobItem.Name);
                    }
                    catch (Exception)
                    {
                        // Continue deleting other blobs even if one fails
                    }
                }

                return true;
            }
            catch (Exception)
            {
                // Error is logged in the service layer
                return false;
            }
        }

        /// <summary>
        /// Get Azure blob URI for accessing the uploaded content
        /// </summary>
        public string GetScormPackageUri(string companyNumber, string courseId, string fileName = "")
        {
            try
            {
                string folderPath = $"{companyNumber}/course/{courseId}";
                if (!string.IsNullOrEmpty(fileName))
                {
                    folderPath = $"{folderPath}/{fileName}";
                }

                var blobClient = _containerClient.GetBlobClient(folderPath);
                return blobClient.Uri.ToString();
            }
            catch (Exception)
            {
                // Error is logged in the service layer
                return string.Empty;
            }
        }

        /// <summary>
        /// Generate a Shared Access Signature (SAS) URL for accessing a blob
        /// SAS tokens provide time-limited access to private containers
        /// </summary>
        public string GenerateSasUrl(string blobPath, int expirationMinutes = 120, bool isThumbnail = false)
        {
            try
            {
                var containerClient = isThumbnail ? _thumbnailContainerClient : _containerClient;

                // Support full URLs by extracting the blob name after the container segment
                string blobName = blobPath;
                if (Uri.TryCreate(blobPath, UriKind.Absolute, out var blobUri))
                {
                    // Expect path: /{container}/{blobName}
                    var segments = blobUri.AbsolutePath.Trim('/').Split('/', 2, StringSplitOptions.RemoveEmptyEntries);
                    if (segments.Length == 2 && string.Equals(segments[0], isThumbnail ? _thumbnailContainerName : _containerName, StringComparison.OrdinalIgnoreCase))
                    {
                        blobName = segments[1];
                    }
                }

                var blobClient = containerClient.GetBlobClient(blobName);
                
                // Generate SAS URI with expiration
                var sasBuilder = new Azure.Storage.Sas.BlobSasBuilder()
                {
                    BlobContainerName = isThumbnail ? _thumbnailContainerName : _containerName,
                    BlobName = blobName,
                    Resource = "b", // "b" for blob
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-1), // Start 1 minute in the past for clock skew
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes)
                };
                
                // Add read permission
                sasBuilder.SetPermissions(Azure.Storage.Sas.BlobSasPermissions.Read);
                
                // Get account key from connection string
                var accountKey = ExtractAccountKeyFromConnectionString(_connectionString);
                if (string.IsNullOrEmpty(accountKey))
                {
                    return string.Empty;
                }
                
                var sasUri = blobClient.GenerateSasUri(sasBuilder);
                return sasUri?.ToString() ?? string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Extract storage account key from connection string
        /// </summary>
        private string ExtractAccountKeyFromConnectionString(string connectionString)
        {
            try
            {
                var parts = connectionString.Split(';');
                var keyPart = parts.FirstOrDefault(p => p.StartsWith("AccountKey="));
                return keyPart?.Replace("AccountKey=", "") ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Delete a blob from Azure storage
        /// </summary>
        public async Task<bool> DeleteBlobAsync(string blobName, bool isThumbnail = false)
        {
            try
            {
                var containerClient = isThumbnail ? _thumbnailContainerClient : _containerClient;
                var blobClient = containerClient.GetBlobClient(blobName);
                
                var response = await blobClient.DeleteIfExistsAsync();
                return response.Value;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Upload a blob stream to Azure storage with specified content type
        /// </summary>
        public async Task<bool> UploadBlobAsync(string blobName, Stream content, string contentType, bool isThumbnail = false)
        {
            try
            {
                var containerClient = isThumbnail ? _thumbnailContainerClient : _containerClient;
                var blobClient = containerClient.GetBlobClient(blobName);
                
                var blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType,
                    ContentDisposition = "inline"
                };
                
                await blobClient.UploadAsync(content, new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders
                });
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

