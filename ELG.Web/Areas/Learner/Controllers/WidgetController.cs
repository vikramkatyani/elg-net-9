using ELG.DAL.LearnerDAL;
using ELG.Model.Learner;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Text;

namespace ELG.Web.Areas.Learner.Controllers
{
    [Area("Learner")]
    public class WidgetController : Controller
    {
        private readonly IConfiguration _configuration;

        public WidgetController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #region Render Widget Views

        /// <summary>
        /// View Text Input Widget (TIW)
        /// </summary>
        [HttpGet]
        public IActionResult ViewWidget(string widgetId, long learnerId, string mode)
        {
            try
            {
                var widgetRep = new WidgetRep();
                TIWResponse response = widgetRep.GetWidgetResponse(widgetId, learnerId);

                if (response != null)
                {
                    response.QuesMode = mode;
                    return PartialView("~/Areas/Learner/Views/Widget/TextInputWidget.cshtml", response);
                }

                return Content("<p>Widget not found</p>", "text/html");
            }
            catch (Exception ex)
            {
                return Content($"<p>Error: {ex.Message}</p>", "text/html");
            }
        }

        /// <summary>
        /// View Multiple Answer Choice Widget (MAC)
        /// </summary>
        [HttpGet]
        public IActionResult ViewWidget_MAC(string widgetId, long learnerId)
        {
            try
            {
                var widgetRep = new WidgetRep();
                MACResponse response = widgetRep.GetMACWidgetResponse(widgetId, learnerId);

                if (response != null)
                {
                    return PartialView("~/Areas/Learner/Views/Widget/MACWidget.cshtml", response);
                }

                return Content("<p>Widget not found</p>", "text/html");
            }
            catch (Exception ex)
            {
                return Content($"<p>Error: {ex.Message}</p>", "text/html");
            }
        }

        /// <summary>
        /// View Binary Pair Comparison Widget (BPC)
        /// </summary>
        [HttpGet]
        public IActionResult ViewWidget_BPC(string widgetId, long learnerId, string mode)
        {
            try
            {
                var widgetRep = new WidgetRep();
                BPCResponse response = widgetRep.GetBPCWidgetResponse(widgetId, learnerId);

                if (response != null)
                {
                    response.QuesMode = mode;
                    return PartialView("~/Areas/Learner/Views/Widget/BPCWidget.cshtml", response);
                }

                return Content("<p>Widget not found</p>", "text/html");
            }
            catch (Exception ex)
            {
                return Content($"<p>Error: {ex.Message}</p>", "text/html");
            }
        }

        #endregion

        #region Save Widget Responses

        /// <summary>
        /// Save Text Input Widget response
        /// </summary>
        [HttpPost]
        public ActionResult SaveResponse_tiw([FromBody] LearnerResponse response)
        {
            try
            {
                if (response != null && !string.IsNullOrWhiteSpace(response.Response))
                {
                    var widgetRep = new WidgetRep();
                    int result = widgetRep.InsertWidgetResponse(response);

                    return Json(new 
                    { 
                        success = result > 0, 
                        message = result > 0 ? "Response saved successfully!" : "Failed to save response.", 
                        result 
                    });
                }

                return Json(new { success = false, message = "Invalid request. Response cannot be empty.", result = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Save Binary Pair Comparison Widget response
        /// </summary>
        [HttpPost]
        public ActionResult SaveResponse_bpc([FromBody] LearnerMACResponse response)
        {
            try
            {
                if (response != null && 
                    !string.IsNullOrWhiteSpace(response.Response_1) && 
                    !string.IsNullOrWhiteSpace(response.Response_2) && 
                    !string.IsNullOrWhiteSpace(response.Response_3))
                {
                    var widgetRep = new WidgetRep();
                    int result = widgetRep.InsertMACWidgetResponse(response);

                    return Json(new 
                    { 
                        success = result > 0, 
                        message = result > 0 ? "Response saved successfully!" : "Failed to save response.", 
                        result 
                    });
                }

                return Json(new { success = false, message = "Invalid request. All responses must be filled.", result = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Save Binary Pair Comparison Widget feedback
        /// </summary>
        [HttpPost]
        public ActionResult SaveFeedback_bpc([FromBody] LearnerMACResponse response)
        {
            try
            {
                if (response != null && !string.IsNullOrWhiteSpace(response.FeedBackResponseText))
                {
                    var widgetRep = new WidgetRep();
                    int result = widgetRep.InsertMACWidgetFeedback(response);

                    return Json(new 
                    { 
                        success = result > 0, 
                        message = result > 0 ? "Feedback saved successfully!" : "Failed to save feedback.", 
                        result 
                    });
                }

                return Json(new { success = false, message = "Invalid request. Feedback cannot be empty.", result = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Save Multiple Answer Choice Widget response
        /// </summary>
        [HttpPost]
        public ActionResult SaveResponse_mac([FromBody] LearnerMACResponse response)
        {
            try
            {
                if (response != null && 
                    !string.IsNullOrWhiteSpace(response.Response_1) && 
                    !string.IsNullOrWhiteSpace(response.Response_2) && 
                    !string.IsNullOrWhiteSpace(response.Response_3))
                {
                    var widgetRep = new WidgetRep();
                    int result = widgetRep.InsertMACWidgetResponse(response);

                    return Json(new 
                    { 
                        success = result > 0, 
                        message = result > 0 ? "Response saved successfully!" : "Failed to save response.", 
                        result 
                    });
                }

                return Json(new { success = false, message = "Invalid request. All responses must be filled.", result = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Save Multiple Answer Choice Widget feedback
        /// </summary>
        [HttpPost]
        public ActionResult SaveFeedback_mac([FromBody] LearnerMACResponse response)
        {
            try
            {
                if (response != null && 
                    !string.IsNullOrWhiteSpace(response.FeedBackResponse) && 
                    !string.IsNullOrWhiteSpace(response.FeedBackResponseText))
                {
                    var widgetRep = new WidgetRep();
                    int result = widgetRep.InsertMACWidgetFeedback(response);

                    return Json(new 
                    { 
                        success = result > 0, 
                        message = result > 0 ? "Feedback saved successfully!" : "Failed to save feedback.", 
                        result 
                    });
                }

                return Json(new { success = false, message = "Invalid request. Feedback cannot be empty.", result = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        #endregion
    }
}
