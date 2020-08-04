using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MT.Infra.BusinessLayer;
using MT.Infra.Common;
using MT.Infra.Tool.Models;

namespace MT.Infra.Tool.Controllers
{
    public class ExpiryController : ApiController
    {
        [Route("api/Expiry/SetAssetInActive")]
        [HttpGet]
        public void SetAssetInActive()
        {
            
            AssetsManagement asm = new AssetsManagement();
            try
            {
                int retVal = asm.SetAssetInActive();
                if (retVal > 0)
                {
                    Log.CreateLog("Assets are Inactive",logLevel:Level.Info);
                }
                else
                {
                    Log.CreateLog("Some error occured. Please check the database", logLevel: Level.Info);
                }
            }
            catch(Exception ex)
            {
                Log.CreateLog(ex.InnerException);
            }
        }

        [Route("api/Expiry/FreeAssetsAfterSRExpiry")]
        [HttpGet]
        public void FreeAssetsAfterSRExpiry()
        {
           
            AssetsManagement asm = new AssetsManagement();
            try
            {
                int retVal = asm.SetAssetInActive();
                if (retVal > 0)
                {

                    Log.CreateLog("Assets are Inactive", logLevel: Level.Info);
                }
                else
                {
                    Log.CreateLog("Some error occured. Please check the database", logLevel: Level.Info);
                }
            }
            catch (Exception ex)
            {
                Log.CreateLog(ex.InnerException);
            }
        }

    }
}
