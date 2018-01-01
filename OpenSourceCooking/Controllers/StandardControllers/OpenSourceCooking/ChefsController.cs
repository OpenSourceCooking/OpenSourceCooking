using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace OpenSourceCooking.Controllers.StandardControllers.OpenSourceCooking
{
    public class ChefsController : Controller
    {
        private OpenSourceCookingEntities db = new OpenSourceCookingEntities();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //Ajax Updates
        [Authorize]
        public async Task<JsonResult> AjaxUpdateIsEmailNotificationEnabled(bool isEmailNotificationEnabled)
        {
            string AspNetId = User.Identity.GetUserId();
            Chef Chef = await db.Chefs.FindAsync(AspNetId);
            Chef.IsEmailNotificationEnabled = isEmailNotificationEnabled;
            db.Entry(Chef).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}
