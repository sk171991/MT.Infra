using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using MT.Infra.Tool.Models;
using MT.Infra.BusinessLayer;
using MT.Infra.Common;
using static MT.Infra.Tool.Global;
using System.Web;
using System.IO;

namespace MT.Infra.Tool.Controllers
{
   
    public class UserController : Controller
    {
        private string srvalue;

        [HttpGet]
        public ActionResult userDashboard()  
        {
                ViewBag.Userdash = Userdash();
                IEnumerable<Dashboardmanage.StatusValue> data = Userdash();

                int sum = 0;
                foreach (var a in data)
                {
                    sum = sum + a.Total;
                }
                ViewData["sum"] = sum;
            return View();
        }

        [NonAction]
        public IEnumerable<Dashboardmanage.StatusValue> Userdash()
        {
            string MID = System.Web.HttpContext.Current.Session["id"].ToString();
            Dashboardmanage Dm = new Dashboardmanage();
            return Dm.Dash(MID);
        }

        #region New SR

        [HttpGet]
        public ActionResult newSR()
        {
            string[] values = (ConfigurationManager.AppSettings["DropdownValues"]).Split(',').Select(sValue => sValue.Trim()).ToArray();
            List<SelectListItem> dropDowns = new List<SelectListItem>(); for (int i = 0; i < values.Length; i++)
            {
                dropDowns.Add(new SelectListItem { Text = values[i], Value = values[i] });
            }
            ViewBag.DropdownVals = dropDowns;
            ViewData["Purpose"]= getPurpose();
            ViewBag.Softwares = getSoftwares();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult newSR(ServiceRequest obj) 
        {
            newSeriveRequest(obj);
            if (srvalue != null)
            {
                ViewBag.message = "Your SR Number is " + Session["SRNumber"];
            }
            newSR();
            return View();
        }

        public void newSeriveRequest(ServiceRequest obj)
        {
            string mid = System.Web.HttpContext.Current.Session["id"].ToString();
            int purpose = obj.Purpose;
            string srDescription = obj.SRDescription;
            if (ModelState.IsValid)
            {
                try
                {
                    SRNumber srn = new SRNumber();     /*BusinessLayer layer object */
                    int userID = Convert.ToInt32(srn.getUserId(mid));
                    string userMailID = TempData["userMailID"] as string;
                    // int SRID = Convert.ToInt32(srn.generateServiceRequestNumber());
                    // string retVal = srn.generateSRNumber(srDescription, purpose, obj.FromDate, obj.TillDate, userID, obj.UserLocation, obj.ContactNumber);
                    int SRID = srn.generateSRNumber(srDescription, purpose, obj.FromDate, obj.TillDate, userID, obj.UserLocation, obj.ContactNumber);
                    //srvalue = retVal;
                    srvalue = SRID.ToString();
                    Session["SRID"] = SRID;
                    Session["CountForSRDetails"] = 1;

                    Token tk = new Token();
                    tk.insertToken(SRID);
                    int RoleID = Convert.ToInt32(srn.generateRoleID(userID));
                    srn.generateServiceRequestStatusID(SRID, RoleID, userID);
                    createServiceRequestNumber(SRID);

                }
                catch (Exception EX)
                {
                    ViewBag.Message("Error");
                }
            }
        }

        public void createServiceRequestNumber(int SRID)
        {
            string SRNumber = "SR000000";
            SRNumber = SRNumber + SRID.ToString();
            Session["SRNumber"] = SRNumber;
        }

        [NonAction]
        public IEnumerable<object> getSoftwares()
        {
            SRNumber assetObj = new SRNumber(); //business Layer object
            return assetObj.GetSoftwares();
        }

        [NonAction]
        public IEnumerable<object> getPurpose()
        {
            SRNumber assetObj = new SRNumber();
            return assetObj.getPurpose();
        }

        #endregion New SR

        [HttpPost]
        public JsonResult Upload()
        {

            bool flag = true;
            string responseMessage = string.Empty;

            if (Request.Files.Count > 0)
            {
                HttpPostedFileBase file = Request.Files[0];

                //add more conditions like file type, file size etc as per your need.
                if (file != null && file.ContentLength > 0 && (Path.GetExtension(file.FileName).ToLower() == ".xlsx" || Path.GetExtension(file.FileName).ToLower() == ".xls"))
                {

                    string Uploadpath = Server.MapPath("~/Upload/");
                    if (!Directory.Exists(Uploadpath))
                    {
                        Directory.CreateDirectory(Uploadpath);
                    }
                    try
                    {
                        string fileName = Path.GetFileName(file.FileName);
                        string filePath = Path.Combine(Uploadpath, fileName);
                        file.SaveAs(filePath);

                        SRNumber srn = new SRNumber();
                        int retVal = srn.UploadAttachment(fileName, filePath);
                        if (retVal == 1)
                        {
                            flag = true;
                            responseMessage = "Upload Successful.";
                        }
                    }
                    catch (Exception ex)
                    {
                        flag = false;
                        responseMessage = "Upload Failed with error: " + ex.Message;
                    }
                }
                else
                {
                    flag = false;
                    responseMessage = "File is invalid. Please upload file with .xlsx/.xls";
                }
            }
            else
            {
                flag = false;
                responseMessage = "File Upload has no file.";
            }

            return Json(new { success = flag, responseMessage = responseMessage }, JsonRequestBehavior.AllowGet);
        }

        #region Existing SR

        [HttpGet]
        public ActionResult ExistingSR()
        {
            string MID = System.Web.HttpContext.Current.Session["id"].ToString();
            SRNumber srn = new SRNumber();
            string Role = srn.getRole(MID);
            Session["Role"] = Role;

            if (!string.IsNullOrEmpty(Session["ajax"] as string))
            {
                string status = Session["ajax"].ToString();
                ViewBag.ExistingSR = srn.getSRDashboardSearch(MID, status, Role);
                TempData["SRStatus"] = status;
                Session["ajax"] = null;
            }
            else
            {
                ViewBag.ExistingSR = getExistingSR(MID);
            }
            string[] values = (ConfigurationManager.AppSettings["DropdownValues"]).Split(',').Select(sValue => sValue.Trim()).ToArray();
            List<SelectListItem> dropDowns = new List<SelectListItem>(); for (int i = 0; i < values.Length; i++)
            {
                dropDowns.Add(new SelectListItem { Text = values[i], Value = values[i] });
            }
            ViewBag.DropdownVals = dropDowns;
            ViewData["Status"] = getStatus();

            return View();
        }

        [HttpPost]
        public ActionResult ExistingSR(ExistingSR esr)
        {
            ViewData["Status"] = getStatus();
            try
            {
                string MID = System.Web.HttpContext.Current.Session["id"].ToString();
                string Role = System.Web.HttpContext.Current.Session["Role"].ToString();
                ViewBag.ExistingSR = findExistingSR(esr, Role, MID);
            }
            catch (Exception e)
            {
                Log.CreateLog(e);
            }
            return View();
        }

        [NonAction]
        public SelectList getStatus()
        {
            SRNumber assetObj = new SRNumber();

            return assetObj.getStatus();
        }

        [NonAction]
        public IEnumerable<object> getExistingSR(string MID)
        {
            SRNumber assetObj = new SRNumber();
            return assetObj.getExistingSR(MID);
        }

        [HttpGet]
        public ActionResult ExistingSRtoSRDetails(string ID)
        {
            try
            {
                string srid = ID.ToString();
                srid = srid.TrimStart('S', 'R', '0');
                Session["SRIDforSRDetails"] = Convert.ToInt32(srid);

                Session["CountforSRDetails"] = 2;
            }
            catch (Exception e)
            {
                Log.CreateLog(e);
            }
            return RedirectToAction("SRDetails");
        }

        [NonAction]
        public IEnumerable<object> findExistingSR(ExistingSR esr, string Role, string MID)
        {
            string ServiceRequest_ID = esr.SR_Number;
            int? SRID;

            if (ServiceRequest_ID == null)
            {
                SRID = null;
            }
            else
            {
                SRID = Convert.ToInt32(ServiceRequest_ID.TrimStart('S', 'R', '0'));
            }
            DateTime? FromDate;
            if (esr.SR_Ticket_From == DateTime.MinValue)
            {
                FromDate = null;
            }
            else
            {
                FromDate = esr.SR_Ticket_From;
            }
            DateTime? TillDate;
            if (esr.SR_Ticket_To == DateTime.MinValue)
            {
                TillDate = null;
            }
            else
            {
                TillDate = esr.SR_Ticket_To;
            }
            int? Status = 0;
            if (esr.Status == 0)
            {
                Status = null;
            }
            else
            {
                Status = esr.Status;
            }
            SRNumber assetObj = new SRNumber();
            return assetObj.findExistingSR(SRID, FromDate, TillDate, Status, Role, MID);
        }

        #endregion Existing SR

        #region SR Details

        [HttpGet]
        public ActionResult SRDetails()
        {
            AssetsManagement sa = new AssetsManagement();
            ViewBag.errmsg = null;
            int SRID = 0;
            int check = Convert.ToInt32(Session["CountforSRDetails"]);
            if (check == 1)
            {
                SRID = Convert.ToInt32(System.Web.HttpContext.Current.Session["SRID"]);
            }
            if (check == 2)
            {
                SRID = Convert.ToInt32(Session["SRIDforSRDetails"]);
            }
            Session["SRIDforSRDetailsPost"] = SRID;
            Session["CountforSRDetails"] = check;
            ViewBag.SRDetails = fetchSRDetails(SRID);
            ViewBag.TicketHistory = fetchTicketHistory(SRID);
            ViewBag.Attachment = getAttachments(SRID);
            ViewBag.AssetName = sa.GetAsset(SRID);
            foreach (var SRDetails in ViewBag.SRDetails)
            {
                if (SRDetails.statusName == "Open")
                {
                    ViewBag.statushelper = "Open - SR is currently pending for Approval/Rejection";
                }
                if (SRDetails.statusName == "Assigned")
                {
                    ViewBag.statushelper = "Assigned - SR is currently assigned to Admin Team";
                }
                if (SRDetails.statusName == "InProgress")
                {
                    ViewBag.statushelper = "InProgress - SR is currently worked upon by Admin team";
                }
                if (SRDetails.statusName == "Approved")
                {
                    ViewBag.statushelper = "Approved - SR is currently Approved by Approver";
                }
                if (SRDetails.statusName == "Cancel")
                {
                    ViewBag.statushelper = "Cancel - SR is cancelled";
                }
                if (SRDetails.statusName == "Closed")
                {
                    ViewBag.statushelper = "Closed - SR has been rejected by Admin";
                }
                if (SRDetails.statusName == "Resolved")
                {
                    ViewBag.statushelper = "Resolved - SR is Resolved";
                }
                ViewBag.Status = SRDetails.statusName;
            }
            return View();
        }

        public IEnumerable<SRNumber.GetAttachment> getAttachments(int SRID)
        {
            SRNumber srn = new SRNumber();
            return srn.GetAttachments(SRID);
        }

        [HttpPost]
        public ActionResult SRDetails(ServiceRequest obj)
        {
            string mid = System.Web.HttpContext.Current.Session["id"].ToString();
            try
            {
                SRNumber srn = new SRNumber();
                int userID = Convert.ToInt32(srn.getUserId(mid));
                int RoleID = Convert.ToInt32(srn.generateRoleID(userID));
                int SRID = Convert.ToInt32(System.Web.HttpContext.Current.Session["SRIDforSRDetailsPost"]);
                int statusID = Convert.ToInt32(srn.generateStatusID(SRID));

                int count = Convert.ToInt32(System.Web.HttpContext.Current.Session["CountforSRDetails"]);

                Session["CountforSRDetails"] = count;
                SRDetails();


                if (Request.Form["cancelsr"] != null)
                {
                    foreach (var SRDetails in ViewBag.SRDetails)
                    {
                        string CurrentSRID = SRDetails.ServiceRequest_ID;
                        CurrentSRID = CurrentSRID.TrimStart('S', 'R', '0');
                        DateTime CurrentLastModifiedTimeStamp = SRDetails.LastModifiedTimeStamp;
                        int retValforSRS1 = srn.CancelSR(CurrentSRID, userID, RoleID, obj.Comments,CurrentLastModifiedTimeStamp);
                    }
                    SRDetails();
                }
            }
            catch (Exception e)
            {
                Log.CreateLog(e);
            }
            return View();
        }

        [NonAction]
        public IEnumerable<object> fetchSRDetails(int SRID)
        {
            SRNumber assetObj = new SRNumber();
            return assetObj.getSRDetails(SRID);
        }

        [NonAction]
        public IEnumerable<object> fetchTicketHistory(int SRID)
        {
            SRNumber assetObj = new SRNumber();
            return assetObj.getTicketHistory(SRID);
        }

        #endregion SR Details

        [HttpPost]
        public JsonResult AjaxMethod(string id)
        {
            Session["ajax"] = id;
            return Json(true);
        }
    }
}