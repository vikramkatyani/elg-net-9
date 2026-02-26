using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ELG.DAL.Utilities;
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
                }
                catch { /* session may not be available; use memory cache */ }
                
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
                    return BadRequest("No cached baseUrl for this course. Please load via the launcher with baseUrl parameter.");
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
                
            // If still not found, try removing the base directory from the path
            // This handles SCORM packages where files are referenced relative to root, not the launch file directory
                if (stream == null && !string.IsNullOrWhiteSpace(baseRelativePath))
                {
                var baseDir = Path.GetDirectoryName(baseRelativePath)?.Replace("\\", "/") ?? "";
                if (!string.IsNullOrEmpty(baseDir) && effectivePath.StartsWith(baseDir + "/", StringComparison.OrdinalIgnoreCase))
                {
                    // Try the path without the base directory prefix
                    var withoutBaseDir = effectivePath.Substring(baseDir.Length + 1);
                    var tryBlobPath = folderPath + withoutBaseDir;
                    try
                    {
                        stream = await _storage.DownloadBlobAsync(tryBlobPath, isThumbnail: false);
                        if (stream != null)
                        {
                            effectivePath = withoutBaseDir;
                            blobPath = tryBlobPath;
                        }
                    }
                    catch { /* fallback didn't work, continue to error */ }
                }
                
                // Also try the original requested path without any base directory prepending
                if (stream == null && !path.Equals(originalPath, StringComparison.OrdinalIgnoreCase))
                        {
                    var tryBlobPath = folderPath + path;
                            try
                            {
                                stream = await _storage.DownloadBlobAsync(tryBlobPath, isThumbnail: false);
                                if (stream != null)
                                {
                            effectivePath = path;
                                    blobPath = tryBlobPath;
                        }
                    }
                    catch { /* fallback didn't work, continue to error */ }
                }
                
                // If the request omitted the base directory (e.g., browser asked for driverOptions.js but files live under scormdriver/)
                if (stream == null && !string.IsNullOrEmpty(baseDir) && !path.StartsWith(baseDir + "/", StringComparison.OrdinalIgnoreCase))
                {
                    var withBaseDir = baseDir + "/" + path;
                    var tryBlobPath = folderPath + withBaseDir;
                    try
                    {
                        stream = await _storage.DownloadBlobAsync(tryBlobPath, isThumbnail: false);
                        if (stream != null)
                        {
                            effectivePath = withBaseDir;
                            blobPath = tryBlobPath;
                        }
                    }
                    catch { /* fallback didn't work, continue to error */ }
                }

                // SCORM Rise packages often store assets under scormcontent/ while HTML uses scormdriver/ relative paths
                // If still not found, try replacing a leading "scormdriver/" with "scormcontent/"
                if (stream == null && effectivePath.StartsWith("scormdriver/", StringComparison.OrdinalIgnoreCase))
                {
                    var withoutDriver = effectivePath.Substring("scormdriver/".Length);
                    var tryBlobPath = folderPath + "scormcontent/" + withoutDriver;
                            try
                            {
                                stream = await _storage.DownloadBlobAsync(tryBlobPath, isThumbnail: false);
                                if (stream != null)
                                {
                            effectivePath = "scormcontent/" + withoutDriver;
                                    blobPath = tryBlobPath;
                        }
                    }
                    catch { /* fallback didn't work, continue to error */ }
                }
            }
            
            if (stream == null)
            {
                return NotFound($"Resource not found: {effectivePath} (looked for: {blobPath})");
            }

            var contentType = _storage.GetContentType(effectivePath);
            if (string.IsNullOrWhiteSpace(contentType)) contentType = "application/octet-stream";

            // If this is an HTML file, rewrite relative URLs to absolute Azure blob SAS URLs
            if (contentType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using (var reader = new StreamReader(stream))
                    {
                        string htmlContent = await reader.ReadToEndAsync();
                        
                        // Inject a <base> tag in <head> to make all relative requests route through CourseProxy
                        // This ensures XMLHttpRequest and fetch() calls from scripts use CourseProxy routing
                        // Include the directory path from baseRelativePath so relative URLs resolve correctly
                        var baseDir = Path.GetDirectoryName(baseRelativePath)?.Replace("\\", "/") ?? "";
                        if (!string.IsNullOrEmpty(baseDir) && !baseDir.EndsWith("/"))
                        {
                            baseDir += "/";
                        }
                        var baseTagHtml = $"<base href=\"/Learner/CourseProxy/{courseId}/{baseDir}\">";
                        if (htmlContent.Contains("</head>", StringComparison.OrdinalIgnoreCase))
                        {
                            htmlContent = htmlContent.Replace("</head>", baseTagHtml + "</head>", StringComparison.OrdinalIgnoreCase);
                        }
                        else if (htmlContent.Contains("<head", StringComparison.OrdinalIgnoreCase))
                        {
                            // If no closing head tag, add after opening head tag
                            var headTagMatch = System.Text.RegularExpressions.Regex.Match(htmlContent, @"<head[^>]*>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            if (headTagMatch.Success)
                            {
                                htmlContent = htmlContent.Insert(headTagMatch.Index + headTagMatch.Length, baseTagHtml);
                            }
                        }
                        
                        // NOTE: We rely on the <base> tag to route all relative URLs through CourseProxy.
                        // We do NOT rewrite URLs to direct SAS URLs because:
                        // 1. The base tag handles static HTML attributes (src, href) automatically
                        // 2. It handles dynamic JS requests (fetch, XMLHttpRequest) automatically  
                        // 3. Rewriting to SAS URLs would expose tokens and bypass CourseProxy caching
                        // 4. Videos, iframes, and other media work better with relative paths
                        
                        // Return modified HTML as a new stream
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
                    // If rewriting fails, return original stream
                    stream.Position = 0;
                    return File(stream, contentType);
                }
            }

            // Handle HTTP Range requests for video seeking support
            // When a video player seeks to a timestamp, it sends a Range header like "Range: bytes=1000-2000"
            // We need to support this by returning 206 Partial Content instead of 200 OK
            var rangeHeader = Request.Headers["Range"].ToString();
            if (!string.IsNullOrEmpty(rangeHeader) && contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    // Get total file size
                    long totalBytes = stream.Length;
                    
                    // Parse Range header (e.g., "bytes=1000-2000" or "bytes=1000-")
                    if (rangeHeader.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase))
                    {
                        var rangeValue = rangeHeader.Substring("bytes=".Length);
                        var parts = rangeValue.Split('-');
                        
                        if (parts.Length == 2 && long.TryParse(parts[0], out long start))
                        {
                            long end;
                            
                            if (string.IsNullOrEmpty(parts[1]))
                            {
                                // "bytes=1000-" means from 1000 to end
                                end = totalBytes - 1;
                            }
                            else if (long.TryParse(parts[1], out long parsedEnd))
                            {
                                // "bytes=1000-2000" means from 1000 to 2000
                                end = Math.Min(parsedEnd, totalBytes - 1);
                            }
                            else
                            {
                                // Invalid range, serve full file
                                return File(stream, contentType);
                            }
                            
                            // Validate range
                            if (start >= 0 && start <= end && end < totalBytes)
                            {
                                // Seek to start position
                                stream.Seek(start, SeekOrigin.Begin);
                                long rangeLength = end - start + 1;
                                
                                // Return 206 Partial Content with proper headers
                                Response.StatusCode = 206;
                                Response.Headers["Content-Range"] = $"bytes {start}-{end}/{totalBytes}";
                                Response.Headers["Content-Length"] = rangeLength.ToString();
                                Response.Headers["Accept-Ranges"] = "bytes";
                                
                                return File(stream, contentType);
                            }
                        }
                    }
                }
                catch
                {
                    // If range parsing fails, serve full file
                }
            }

            // For non-video files or if no Range header, return full file
            Response.Headers["Accept-Ranges"] = "bytes";
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