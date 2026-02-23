using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ELG.DAL.Utilities;
using ELG.Web.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace ELG.Web.Areas.Learner.Controllers
{
    [Area("Learner")]
    [Route("Learner/CourseProxy/{courseId}/{*path}")]
    public class CourseProxyController : Controller
    {
        private readonly AzureStorageUtility _storage;
        // In-memory cache for course folder paths (fallback if session isn't available)
        private static readonly Dictionary<string, string> _folderPathCache = new Dictionary<string, string>();

        public CourseProxyController(AzureStorageUtility storage)
        {
            _storage = storage;
        }

        // Streams course files from Azure or local file system based on a base URL and relative path.
        // On first request (with baseUrl query param), stores the file path in session and memory cache.
        // Subsequent requests for the same courseId reuse the cached file path.
        [HttpGet]
        public async Task<IActionResult> Get(string courseId, string path, [FromQuery] string baseUrl, [FromServices] Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            // Validate courseId is not empty or invalid
            if (string.IsNullOrWhiteSpace(courseId) || courseId == "0")
            {
                return BadRequest($"Invalid courseId: '{courseId}'. CourseId must be a non-empty, non-zero value.");
            }

            string folderPath = null;
            string baseRelativePath = null; // launch file relative path, e.g. res/index.html
            bool isLocalFile = false; // Indicates whether we're serving from local file system
            var sessionKey = $"CourseProxy_{courseId}";
            var sessionKeyRel = $"CourseProxyRel_{courseId}";
            var sessionKeyIsLocal = $"CourseProxyLocal_{courseId}";

            // If baseUrl is provided, derive folder path and cache it in session + memory
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                try
                {
                    Logger.Info($"CourseProxyController.Get: Initializing courseId={courseId} with baseUrl={baseUrl}");
                    // Check if baseUrl is a relative path (starts with .. or ./) or local path (contains ../)
                    if (baseUrl.StartsWith("..") || baseUrl.StartsWith("./") || baseUrl.Contains("../"))
                    {
                        // Local file path - resolve relative to content root
                        isLocalFile = true;
                        string resolvedPath = System.IO.Path.Combine(env.ContentRootPath, baseUrl);
                        // Normalize path
                        resolvedPath = System.IO.Path.GetFullPath(resolvedPath);
                        
                        // Extract folder path and launch file relative path
                        string launchFileFullPath = resolvedPath;
                        string launchFileName = System.IO.Path.GetFileName(launchFileFullPath);
                        folderPath = System.IO.Path.GetDirectoryName(launchFileFullPath);
                        
                        // Ensure trailing separator
                        if (!folderPath.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                            folderPath += System.IO.Path.DirectorySeparatorChar;
                        
                        baseRelativePath = launchFileName;
                    }
                    else if (baseUrl.StartsWith("http://") || baseUrl.StartsWith("https://"))
                    {
                        // Azure blob URI with SAS token
                        // Expected format: https://account.blob.core.windows.net/container/companyId/course/uuid/path/to/file.html?sv=...
                        var baseUri = new Uri(baseUrl);
                        var pathWithoutQuery = baseUri.AbsolutePath.TrimStart('/');
                        
                        // pathWithoutQuery format: "elg-learn/H9A3M9W3/course/uuid/res/index.html" or similar
                        var parts = pathWithoutQuery.Split('/');
                        if (parts.Length >= 4)
                        {
                            folderPath = $"{parts[1]}/course/{parts[3]}/";
                            if (parts.Length > 4)
                            {
                                var relParts = parts.Skip(4);
                                baseRelativePath = string.Join('/', relParts);
                            }
                            else
                            {
                                baseRelativePath = "index.html";
                            }
                        }
                        else
                        {
                            return BadRequest($"Invalid baseUrl path format. Expected at least 4 segments, got {parts.Length}");
                        }
                    }
                    else
                    {
                        return BadRequest($"BaseUrl must be either an absolute URI (http/https) or a relative file path");
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest($"Failed to parse baseUrl: {ex.Message}");
                }

                // Store in both session and memory cache for redundancy
                try
                {
                    HttpContext.Session.SetString(sessionKey, folderPath);
                    if (!string.IsNullOrWhiteSpace(baseRelativePath))
                        HttpContext.Session.SetString(sessionKeyRel, baseRelativePath);
                    HttpContext.Session.SetString(sessionKeyIsLocal, isLocalFile.ToString());
                    Logger.Info($"CourseProxyController.Get: Cached courseId={courseId} in session. FolderPath={folderPath}, BaseRelativePath={baseRelativePath}");
                }
                catch (Exception ex) { Logger.Info($"CourseProxyController.Get: Session not available for courseId={courseId}: {ex.Message}"); }
                
                lock (_folderPathCache)
                {
                    _folderPathCache[sessionKey] = folderPath;
                    if (!string.IsNullOrWhiteSpace(baseRelativePath))
                        _folderPathCache[sessionKeyRel] = baseRelativePath;
                    _folderPathCache[sessionKeyIsLocal] = isLocalFile.ToString();
                }
            }
            else
            {
                // Try to retrieve cached folder path from session first, then memory cache
                try
                {
                    folderPath = HttpContext.Session.GetString(sessionKey);
                    baseRelativePath = HttpContext.Session.GetString(sessionKeyRel);
                    var isLocalStr = HttpContext.Session.GetString(sessionKeyIsLocal);
                    if (!string.IsNullOrWhiteSpace(isLocalStr))
                        isLocalFile = bool.Parse(isLocalStr);
                }
                catch { /* session may not be available */ }

                if (string.IsNullOrWhiteSpace(folderPath))
                {
                    lock (_folderPathCache)
                    {
                        _folderPathCache.TryGetValue(sessionKey, out folderPath);
                        _folderPathCache.TryGetValue(sessionKeyRel, out baseRelativePath);
                        if (_folderPathCache.TryGetValue(sessionKeyIsLocal, out var isLocalStr))
                            isLocalFile = bool.Parse(isLocalStr);
                    }
                }

                if (string.IsNullOrWhiteSpace(folderPath))
                {
                    Logger.Error($"CourseProxyController.Get: No cached baseUrl for courseId={courseId}, path={path}. Session keys: {sessionKey}={HttpContext.Session.GetString(sessionKey)}, Memory cache size: {_folderPathCache.Count}");
                    return BadRequest($"Course {courseId} not initialized. Please launch the course through the learner interface with the baseUrl parameter.");
                }
            }

            // Prefer cached baseRelativePath when path is empty or default "index.html"
            var effectivePath = string.IsNullOrWhiteSpace(path) || path.Equals("index.html", StringComparison.OrdinalIgnoreCase)
                ? (string.IsNullOrWhiteSpace(baseRelativePath) ? "index.html" : baseRelativePath)
                : path;
            
            // Note: path already includes the base directory from the URL (e.g., "scormdriver/lib/file.css")
            // because the <base> tag in HTML causes the browser to prepend it
            // So we should NOT prepend it again - just use the path as-is
            
            var blobPath = folderPath + effectivePath;
            var originalPath = path; // Save original requested path for fallback attempts

            Stream stream = null;
            string resolvedFilePath = null;

            if (isLocalFile)
            {
                // Serve from local file system
                resolvedFilePath = System.IO.Path.Combine(folderPath, effectivePath).Replace("/", System.IO.Path.DirectorySeparatorChar.ToString());
                if (System.IO.File.Exists(resolvedFilePath))
                {
                    stream = System.IO.File.OpenRead(resolvedFilePath);
                }
            }
            else
            {
                // Serve from Azure Blob Storage
                try
                {
                    stream = await _storage.DownloadBlobAsync(blobPath, isThumbnail: false);
                }
                catch (Exception ex)
                {
                    return StatusCode(503, $"Azure storage error: {ex.Message}");
                }
            }
            
            // If not found and effectivePath is just "index.html", try common subdirectories
            if (stream == null && effectivePath == "index.html" && string.IsNullOrWhiteSpace(baseRelativePath))
            {
                var tryPaths = new[] { "res/index.html", "index/index.html", "dist/index.html" };
                foreach (var tryPath in tryPaths)
                {
                    if (isLocalFile)
                    {
                        resolvedFilePath = System.IO.Path.Combine(folderPath, tryPath).Replace("/", System.IO.Path.DirectorySeparatorChar.ToString());
                        if (System.IO.File.Exists(resolvedFilePath))
                        {
                            stream = System.IO.File.OpenRead(resolvedFilePath);
                            effectivePath = tryPath;
                            blobPath = folderPath + tryPath;
                            break;
                        }
                    }
                    else
                    {
                        var tryBlobPath = folderPath + tryPath;
                        try
                        {
                            stream = await _storage.DownloadBlobAsync(tryBlobPath, isThumbnail: false);
                            if (stream != null)
                            {
                                effectivePath = tryPath;
                                blobPath = tryBlobPath;
                                break;
                            }
                        }
                        catch { /* continue trying other paths */ }
                    }
                }
            }
            
            if (stream == null)
            {
                Logger.Error($"CourseProxyController.Get: Resource not found. CourseId={courseId}, Path={path}, EffectivePath={effectivePath}, BlobPath={blobPath}, FolderPath={folderPath}");
                return NotFound($"Resource not found: {effectivePath} (looked for: {blobPath}");
            }

            // Determine content type based on file extension
            string contentType = "application/octet-stream";
            if (isLocalFile && !string.IsNullOrEmpty(resolvedFilePath))
            {
                contentType = GetContentTypeFromExtension(System.IO.Path.GetExtension(resolvedFilePath));
            }
            else if (_storage != null)
            {
                var storageContentType = _storage.GetContentType(effectivePath);
                if (!string.IsNullOrWhiteSpace(storageContentType))
                    contentType = storageContentType;
            }

            Logger.Debug($"CourseProxyController.Get: Serving resource. CourseId={courseId}, Path={path}, EffectivePath={effectivePath}, ContentType={contentType}");

            // If this is an HTML file, inject a <base> tag to route relative requests through CourseProxy
            if (contentType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using (var reader = new StreamReader(stream))
                    {
                        string htmlContent = await reader.ReadToEndAsync();
                        
                        // Inject a <base> tag in <head> to make all relative requests route through CourseProxy
                        var baseDir = System.IO.Path.GetDirectoryName(baseRelativePath ?? "index.html")?.Replace("\\", "/") ?? "";
                        
                        // Build base href - ensure proper formatting
                        string baseHref = $"/Learner/CourseProxy/{courseId}/";
                        if (!string.IsNullOrEmpty(baseDir))
                        {
                            baseHref += baseDir + "/";
                        }
                        
                        // Normalize multiple slashes
                        baseHref = System.Text.RegularExpressions.Regex.Replace(baseHref, @"/+", "/");
                        
                        var baseTagHtml = $"<base href=\"{baseHref}\">";
                        if (htmlContent.Contains("</head>", StringComparison.OrdinalIgnoreCase))
                        {
                            htmlContent = htmlContent.Replace("</head>", baseTagHtml + "</head>", StringComparison.OrdinalIgnoreCase);
                        }
                        else if (htmlContent.Contains("<head", StringComparison.OrdinalIgnoreCase))
                        {
                            var headTagMatch = System.Text.RegularExpressions.Regex.Match(htmlContent, @"<head[^>]*>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            if (headTagMatch.Success)
                            {
                                htmlContent = htmlContent.Insert(headTagMatch.Index + headTagMatch.Length, baseTagHtml);
                            }
                        }
                        
                        var modifiedStream = new MemoryStream();
                        var writer = new StreamWriter(modifiedStream);
                        await writer.WriteAsync(htmlContent);
                        await writer.FlushAsync();
                        modifiedStream.Position = 0;
                        
                        return File(modifiedStream, contentType);
                    }
                }
                catch
                {
                    stream.Position = 0;
                    return File(stream, contentType);
                }
            }

            return File(stream, contentType);
        }

        private string GetContentTypeFromExtension(string extension)
        {
            var extensionMap = new Dictionary<string, string>
            {
                { ".html", "text/html" },
                { ".htm", "text/html" },
                { ".css", "text/css" },
                { ".js", "application/javascript" },
                { ".json", "application/json" },
                { ".xml", "application/xml" },
                { ".pdf", "application/pdf" },
                { ".png", "image/png" },
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".gif", "image/gif" },
                { ".svg", "image/svg+xml" },
                { ".ico", "image/x-icon" },
                { ".mp3", "audio/mpeg" },
                { ".mp4", "video/mp4" },
                { ".webm", "video/webm" },
                { ".woff", "font/woff" },
                { ".woff2", "font/woff2" },
                { ".ttf", "font/ttf" },
                { ".eot", "application/vnd.ms-fontobject" },
                { ".zip", "application/zip" }
            };

            extension = extension.ToLowerInvariant();
            return extensionMap.TryGetValue(extension, out var type) ? type : "application/octet-stream";
        }
    }
}