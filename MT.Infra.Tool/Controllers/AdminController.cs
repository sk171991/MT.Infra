using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MT.Infra.BusinessLayer;
using MT.Infra.Common;
using System.Configuration;
using static MT.Infra.Tool.Global;
using MT.Infra.Tool.Models;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace MT.Infra.Tool.Controllers
{

    public class AdminController : Controller
    {
        // GET: Admin
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
                ViewBag.ExistingSR = getExistingSR();
            }
            string[] values = (ConfigurationManager.AppSettings["DropdownValues"]).Split(',').Select(sValue => sValue.Trim()).ToArray();
            List<SelectListItem> dropDowns = new List<SelectListItem>(); for (int i = 0; i < values.Length; i++)
            {
                dropDowns.Add(new SelectListItem { Text = values[i], Value = values[i] });
            }
            ViewBag.DropdownVals = dropDowns;
            ViewData["Status"] = getStatuses();

            return View();
        }

        [NonAction]
        public SelectList getStatuses()
        {
            SRNumber srn = new SRNumber();

            return srn.getStatus();
        }


        [HttpPost]
        public ActionResult ExistingSR(ExistingSR esr)
        {
            ViewData["Status"] = getStatuses();
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
        public IEnumerable<object> getExistingSR()
        {
            SRNumber assetObj = new SRNumber();
            return assetObj.getApproverExistingSR();
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

        [HttpGet]
        public ActionResult SRReports()
        {

            ViewBag.Status = getStatus();
            return View();
        }


        [HttpPost]
        public ActionResult SRReports(SRReports.Reports rept)
        {
            ViewBag.ReportTable = getReport(rept);
            ViewBag.Status = getStatus();
            return View();
        }

        [NonAction]
        public IEnumerable<object> getReport(SRReports.Reports rept)
        {
            SRReports rptObj = new SRReports();
            return rptObj.SRReport(rept);
        }

        [NonAction]
        public IEnumerable<object> getStatus()
        {
            SRReports rptObj = new SRReports();
            return rptObj.getStatus();
        }

        public ActionResult ConfigureAsset()
        {
            MachineType();
            ViewBag.AssetsData = ConfigureAssets();
            AssetsManagement asm = new AssetsManagement();
            ViewBag.SoftwareDetails = asm.SoftwareDetails();
            return View();
        }


        [HttpPost]
        public ActionResult ConfigureAsset(ViewAssetsModel assest)
        {
            ViewBag.AssetsData = ConfigureAssets();
            try
            {
                if (ModelState.IsValid)
                {
                    AssetsManagement AMB = new AssetsManagement();
                    int retuser = AMB.AssetsInsert(assest.SystemName, assest.SystemIP, assest.SerialNumber,assest.MachineCode, assest.MachineType, assest.RegistrationDate, assest.ExpirationDate);

                    if (retuser == 1)
                    {
                        ViewBag.Message = "Record Inserted successfully";
                    }
                }
            }
            catch (Exception e)
            {
                Log.CreateLog(e);
            }

            MachineType();
            ModelState.Clear();
            return View();
        }


        [HttpGet]
        public ActionResult AssetManageEdit(int ID)
        {
            MachineType();
            AssetsManagement AMB = new AssetsManagement();
            var std = AMB.ConfigureAssets().Where(s => s.ID == ID).FirstOrDefault();
            std.MachineType = std.MachineType.Trim();
            return View(std);
        }


        [HttpPost]
        public ActionResult AssetManageEdit()
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ViewAssetsModel vm = new ViewAssetsModel();
                    UpdateModel<ViewAssetsModel>(vm);
                    AssetsManagement AMB = new AssetsManagement();
                    AMB.UpdateAssets(vm);
                    TempData["msg"] = "<script>alert('Record Updated successfully');</script>";

                }
            }
            catch (Exception e)
            {
                Log.CreateLog(e);
            }
            return RedirectToAction("ConfigureAsset", "Admin");
        }

        [HttpGet]
        public ActionResult Delete(int ID)
        {
            try
            {
                AssetsManagement AMB = new AssetsManagement();
                int retVal = AMB.DeleteAssests(ID);
                if (retVal == 1)
                {
                    TempData["msg"] = "<script>alert('Record deleted successfully');</script>";
                }
                else
                {
                    TempData["msg"] = "<script>alert('This Asset cannot be deleted as it is assigned to a Service Request. Please check on the View Asset Page !!');</script>";
                }
            }
            catch (Exception e)
            {
                Log.CreateLog(e);
            }
            return RedirectToAction("ConfigureAsset");
        }

        [NonAction]
        public IEnumerable<object> ConfigureAssets()
        {
            AssetsManagement asm = new AssetsManagement();
            return asm.ConfigureAssets();
        }
       
        public IEnumerable<object> SearchAsset(AssetsManagement.Asset assetdata)
        {
            AssetsManagement asm = new AssetsManagement();
            return asm.SearchAssets(assetdata);
        }

        [NonAction]
        public void MachineType()
        {
            string[] values = (ConfigurationManager.AppSettings["MachineType"]).Split(',').Select(sValue => sValue.Trim()).ToArray();
            List<SelectListItem> dropDown = new List<SelectListItem>();
            for (int i = 0; i < values.Length; i++)
            {
                dropDown.Add(new SelectListItem { Text = values[i], Value = values[i] });
            }
            ViewBag.MachineType = dropDown;
        }

        [HttpGet]
        public ActionResult AdminDashboard()
        {
            AssetsManagement sa = new AssetsManagement();
            ViewBag.Roles = new SelectList(Enum.GetValues(typeof(Roles)));
            ViewBag.getAvailable = sa.getAvailableAssets();
            ViewBag.getUnAvailable = sa.getUnAvailableAssets();
            ViewBag.admindash = Admindash();
            ViewBag.adminTask = AdminTask();
            IEnumerable<Dashboardmanage.StatusValue> data = Admindash();

            int sum = 0;
            foreach (var a in data)
            {
                sum = sum + a.Total;
            }
            ViewData["sum"] = sum;
            return View();
        }

        [NonAction]
        public IEnumerable<object> AdminTask()
        {
            Dashboardmanage Dm = new Dashboardmanage();
            return Dm.TaskApproverforAdmin();
        }

        [NonAction]
        public IEnumerable<Dashboardmanage.StatusValue> Admindash()
        {
            string MID = null;
            Dashboardmanage Dm = new Dashboardmanage();
            return Dm.Dash(MID);
        }

        [NonAction]
        public void AssetStatus()
        {
            string[] values = (ConfigurationManager.AppSettings["Status"]).Split(',').Select(sValue => sValue.Trim()).ToArray();
            List<SelectListItem> dropDown = new List<SelectListItem>();
            for (int i = 0; i < values.Length; i++)
            {
                dropDown.Add(new SelectListItem { Text = values[i], Value = values[i] });
            }
            ViewBag.AssetStatus = dropDown;
        }

        [HttpGet]
        public ActionResult ViewAsset()
        {
            AssetsManagement asm = new AssetsManagement();
            if (!string.IsNullOrEmpty(Session["ajax"] as string))
            {
                string sessionVal = Session["ajax"].ToString();
                int index = sessionVal.IndexOf(' ');
                string status = sessionVal.Substring(0, index);
                string machineType = sessionVal.Substring(index + 1);
               
                ViewBag.AssetDetails = asm.AssetDashboardSearch(machineType, status);
                TempData["Machinetype"] = machineType;
                TempData["ServerStatus"] = status;
                Session["ajax"] = null;

            }
            else
            {
               
                ViewBag.AssetDetails = asm.GetAssets();
            }
            MachineType();
            AssetStatus();
            return View();
        }

        [HttpPost]
        public ActionResult ViewAsset(AssetsManagement.Asset assetdata)
        {
            MachineType();
            AssetStatus();
            ViewBag.AssetDetails = SearchAsset(assetdata);
            return View();
        }

        [HttpPost]
        public JsonResult AjaxMethod(string id)
        {
            Session["ajax"] = id;
            return Json(true);
        }

        [HttpGet]
        public JsonResult SoftwareDetails()
        {
            AssetsManagement asm = new AssetsManagement();
            IEnumerable<AssetsManagement.Softwares> software = asm.SoftwareDetails();
            return Json(software, JsonRequestBehavior.AllowGet);
        }
       
        [HttpPost]
        public JsonResult AddSoftware(int ID , string Name , string Description)
        {
            AssetsManagement asm = new AssetsManagement();
            int retVal = 1;
            if (retVal == asm.AddSoftware(ID, Name, Description))
            {
                return Json(true);
            }
            else
            {
                return Json(false);
            }
        }

        [HttpPost]
        public JsonResult SoftwareDelete(int ID)
        {
            AssetsManagement asm = new AssetsManagement();
            int retVal = 1;
            if (retVal == asm.SoftwareDelete(ID))
            {
                return Json(true);
            }
            else
            {
                return Json(false);
            }
        }

        public void AssetUnassign(int ID)
        {
            AssetsManagement asm = new AssetsManagement();
            asm.AssetUnassign(ID);
        }

        [HttpPost]
        public JsonResult ViewSoftwares(int AssetID)
        {
            AssetsManagement asm = new AssetsManagement();
            IEnumerable<AssetsManagement.Softwares> softwarelist = asm.ViewSoftwares(AssetID);
            if (softwarelist.Count() == 0)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(softwarelist, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult AddSoftwares(int AssetID)
        {
            AssetsManagement asm = new AssetsManagement();
            IEnumerable<AssetsManagement.Softwares> softwarelist = asm.AddSoftwares(AssetID);
            return Json(softwarelist, JsonRequestBehavior.AllowGet);
         }

        [HttpPost]
        public JsonResult UnTagSoftware(int AssetID, int SoftwareID)
        {
            AssetsManagement asm = new AssetsManagement();
            int retVal = asm.UnTagSoftware(AssetID,SoftwareID);
            return Json(retVal, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult TagSoftware(int AssetID, int SoftwareID)
        {
            AssetsManagement asm = new AssetsManagement();
            int retVal = asm.TagSoftware(AssetID, SoftwareID);
            return Json(retVal, JsonRequestBehavior.AllowGet);
            // return Content("<script>alert('Asset has been successfully unassigned to the Service Request');</script>");
        }

        [HttpPost]
        public JsonResult GetRequestedMID(int SRID)
        {
            SRNumber srn = new SRNumber();
            int retVal = srn.CheckSRIDforMID(SRID);
            if (retVal == 1)
            {
                IEnumerable<SRNumber.EmployeeData> employeeDatas = srn.GetRequestedMID(SRID);
                if (employeeDatas.Count() != 0)
                {
                    return Json(employeeDatas, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult CheckAssetMIDMapping(int MID,int AssetID)
        {
            SRNumber srn = new SRNumber();
            string retVal = srn.CheckAssetMIDMapping(MID,AssetID);
            if (retVal != "Exists" && retVal != null)
            {
                return Json(retVal, JsonRequestBehavior.AllowGet);
            }
            else if(retVal == "Exists")
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult TagAssetMID(int MID, int AssetID)
        {
            SRNumber srn = new SRNumber();
            int retVal = srn.TagAssetMID(MID,AssetID);
            if (retVal == 1)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult UnTagAssetMID(int MID, int AssetID)
        {
            SRNumber srn = new SRNumber();
            int retVal = srn.UnTagAssetMID(MID, AssetID);
            if (retVal == 1)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetTagAssetMID(int AssetID)
        {
            SRNumber srn = new SRNumber();
            IEnumerable<SRNumber.EmployeeData> employees = srn.GetTagAssetMID(AssetID);
            if (employees.Count() != 0)
            {
                return Json(employees, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult AssetAssign(int SRID, int AssetID, string AssetName)
        {
            AssetsManagement asm = new AssetsManagement();
            asm.AssetAssign(SRID, AssetID);
            string toMailID = asm.SRCreatorMail(SRID);
            string subject = "Asset Assignment against SR000000" + SRID;
            string mailBody = "This is to inform you that" + " " + AssetName + " " + "has been assigned against SR raised SR000000" + SRID + "<br/> Kindly visit the SR for more details";
            SendMail(subject, mailBody, toMailID);
            return Json(true);

        }

        [NonAction]
        public void SendMail(string subject, string mailBody, string toMailID)
        {
            try
            {
                // Create the Outlook application.
                Microsoft.Office.Interop.Outlook.Application oApp = new Outlook.Application();

                // Create a new mail item.
                Outlook.MailItem oMsg = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);

                // Set the subject.
                oMsg.Subject = subject;
                oMsg.To = toMailID;

                // Set HTMLBody.
                String sHtml;
                //sHtml = "Please take action against below SR12345"+" <br><a href='google.com'><img src='C:\\Users\\M1054034\\Documents\\Visual Studio 2017\\Projects\\SendMail\\SendMail\\Images\\Approve.jpg'></a>" + "  "+  "<a href='google.com'><img src='C:\\Users\\M1054034\\Documents\\Visual Studio 2017\\Projects\\SendMail\\SendMail\\Images\\Reject.jpg'></a></br>";

                sHtml = mailBody;
                oMsg.HTMLBody = sHtml;

                // Add a recipient.
                Outlook.Recipients oRecips = (Outlook.Recipients)oMsg.Recipients;
                // TODO: Change the recipient in the next line if necessary.
                Outlook.Recipient oRecip = (Outlook.Recipient)oRecips.Add(toMailID);
                oRecip.Resolve();

                // Send.
                oMsg.Send();

                // Clean up.
                oRecip = null;
                oRecips = null;
                oMsg = null;
                //oNS = null;
                oApp = null;

            }

            // Simple error handling.
            catch (Exception e)
            {
                ViewBag.Message("Error");
            }

            // Default return value.


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
            //return View();
        }

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
            ViewBag.AssetName = sa.GetAsset(SRID);
            ViewBag.AssetMIDList = sa.GetAssetMIDList(SRID);
            ViewBag.SRDetails = fetchSRDetails(SRID);
            ViewBag.TicketHistory = fetchTicketHistory(SRID);
            ViewBag.Attachment = getAttachments(SRID);
            ViewBag.NextStatus = new SelectList(Enum.GetValues(typeof(NextStatus)));

            foreach (var SRDetails in ViewBag.SRDetails)
            {
                if (SRDetails.statusName == "Open")
                {
                    ViewBag.statushelper = "Open - SR is currently pending for Approval/Rejection";
                }
                if (SRDetails.statusName == "Assigned")
                {
                    ViewBag.statushelper = "Assigned - SR is currently assigned to Infra Team";
                }
                if (SRDetails.statusName == "InProgress")
                {
                    ViewBag.statushelper = "InProgress - SR is currently worked upon by Infra team";
                }
                if (SRDetails.statusName == "Approved")
                {
                    ViewBag.statushelper = "Approved - SR is currently Approved by Admin";
                }
                if (SRDetails.statusName == "Cancel")
                {
                    ViewBag.statushelper = "Cancal - SR is cancelled";
                }
                if (SRDetails.statusName == "Closed")
                {
                    ViewBag.statushelper = "Closed - SR has been closed by Admin";
                }
                if (SRDetails.statusName == "Resolved")
                {
                    ViewBag.statushelper = "Resolved - SR is Resolved";
                }
                ViewBag.Statuses = SRDetails.statusName;
            }
            return View();
        }

        [NonAction]
        public IEnumerable<object> fetchSRDetails(int SRID)
        {
            SRNumber assetObj = new SRNumber();
            return assetObj.getSRDetails(SRID);
        }
        public IEnumerable<SRNumber.GetAttachment> getAttachments(int SRID)
        {
            SRNumber srn = new SRNumber();
            return srn.GetAttachments(SRID);
        }
        [NonAction]
        public IEnumerable<object> fetchTicketHistory(int SRID)
        {
            SRNumber assetObj = new SRNumber();
            return assetObj.getTicketHistory(SRID);
        }

        [HttpPost]
        public ActionResult SRDetails(ApproverSRDetails obj)
        {
            string mid = System.Web.HttpContext.Current.Session["id"].ToString();
            try
            {
                SRNumber srn = new SRNumber();
                int userID = Convert.ToInt32(srn.getUserId(mid));
                int RoleID = Convert.ToInt32(srn.generateRoleID(userID));
                int count = Convert.ToInt32(System.Web.HttpContext.Current.Session["CountforSRDetails"]);
                Session["CountforSRDetails"] = count;
                int SRID = Convert.ToInt32(System.Web.HttpContext.Current.Session["SRIDforSRDetailsPost"]);

                int retValforSRS = srn.InsertNextSRStatus(SRID, userID, RoleID, obj.Comments, obj.selectStatus);

                if (retValforSRS == 1)
                {
                    SRDetails();
                    if (obj.selectStatus == "Closed")
                    {
                        string Subject = "SR000000" + SRID + " " + "-Resolved";
                        string mailId = srn.SRCreatorMail(SRID);
                        string mailBody = "Hi," + "<br/> The SR000000" + SRID + " " + "has been closed by Admin team. Please visit SR for any further concern";
                        SendMail(Subject, mailBody, mailId);
                    }
                    TempData["AlertMessage"] = "Ticket is now " + obj.selectStatus;
                }
                else if(retValforSRS == 2)
                 {
                            SRDetails();
                            TempData["AlertMessage"] = "Service Ticket is already in Progress !!!";

                 }
                else
                {
                    //return Content("<script> alert('Please make the Service request in Progress before closing it')</script>");
                    SRDetails();
                    TempData["AlertMessage"] = "Please make the Service request in Progress before closing it";

                }
                
            }
            catch (Exception e)
            {
                Log.CreateLog(e.InnerException);
            }

            return View();
        }

        [HttpGet]
        public ActionResult AssetReport()
        {
            AssetsManagement asm = new AssetsManagement();
            MachineType();
            ViewBag.AssetReport = asm.AssetReport();
            return View();
        }

        [HttpPost]
        public ActionResult AssetReport(AssetsManagement.AssetReports reports)
        {
            AssetsManagement asm = new AssetsManagement();
            MachineType();
            
            ViewBag.AssetReport = asm.AssetReportSearch(reports);
            if (ViewBag.AssetReport.Count == 0)
            {
                TempData["Search"] = "No Data found";
                return RedirectToAction("AssetReport");
                
            }
            else
            {
                return View();
            }
        }

    }
}