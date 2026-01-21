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

        // Streams blobs from Azure based on a base folder URL and relative path.
        // On first request (with baseUrl query param), stores the folder path in session and memory cache.
        // Subsequent requests for the same courseId reuse the cached folder path.
        [HttpGet]
        public async Task<IActionResult> Get(string courseId, string path, [FromQuery] string baseUrl)
        {
            string folderPath = null;
            string baseRelativePath = null; // launch file relative path, e.g. res/index.html
            var sessionKey = $"CourseProxy_{courseId}";
            var sessionKeyRel = $"CourseProxyRel_{courseId}";

            // If baseUrl is provided, derive folder path and cache it in session + memory
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                // baseUrl is a full Azure blob URI with SAS token
                // Expected format: https://account.blob.core.windows.net/container/companyId/course/uuid/path/to/file.html?sv=...
                // Example: https://elgdocstorage.blob.core.windows.net/elg-learn/H9A3M9W3/course/92d9afab-8c6c-4688-bb02-dbd505ba4d21/res/index.html?sv=...
                
                try
                {
                    var baseUri = new Uri(baseUrl);
                    var pathWithoutQuery = baseUri.AbsolutePath.TrimStart('/');
                    
                    // pathWithoutQuery format: "elg-learn/H9A3M9W3/course/uuid/res/index.html" or similar
                    // We need to extract: "H9A3M9W3/course/uuid/" and remember launch relative path (e.g., res/index.html)
                    
                    // Split by / and rebuild: skip container (0), then take company/course/uuid
                    var parts = pathWithoutQuery.Split('/');
                    if (parts.Length >= 4) // At minimum: container, company, course, uuid
                    {
                        // parts[0] = container (elg-learn)
                        // parts[1] = company (H9A3M9W3)
                        // parts[2] = "course"
                        // parts[3] = UUID (92d9afab-8c6c-4688-bb02-dbd505ba4d21)
                        folderPath = $"{parts[1]}/course/{parts[3]}/";

                        // launch file relative path = everything after company/course/uuid/
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
                }
                catch { /* session may not be available; use memory cache */ }
                
                lock (_folderPathCache)
                {
                    _folderPathCache[sessionKey] = folderPath;
                    if (!string.IsNullOrWhiteSpace(baseRelativePath))
                        _folderPathCache[sessionKeyRel] = baseRelativePath;
                }
            }
            else
            {
                // Try to retrieve cached folder path from session first, then memory cache
                try
                {
                    folderPath = HttpContext.Session.GetString(sessionKey);
                    baseRelativePath = HttpContext.Session.GetString(sessionKeyRel);
                }
                catch { /* session may not be available */ }

                if (string.IsNullOrWhiteSpace(folderPath))
                {
                    lock (_folderPathCache)
                    {
                        _folderPathCache.TryGetValue(sessionKey, out folderPath);
                        _folderPathCache.TryGetValue(sessionKeyRel, out baseRelativePath);
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
            try
            {
                stream = await _storage.DownloadBlobAsync(blobPath, isThumbnail: false);
            }
            catch (Exception ex)
            {
                return StatusCode(503, $"Azure storage error: {ex.Message}");
            }
            
            // If not found and effectivePath is just "index.html", try common subdirectories
            if (stream == null && effectivePath == "index.html" && string.IsNullOrWhiteSpace(baseRelativePath))
            {
                var tryPaths = new[] { "res/index.html", "index/index.html", "dist/index.html" };
                foreach (var tryPath in tryPaths)
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

            // Return the stream without buffering entire file in memory
            return File(stream, contentType);
        }
    }
}