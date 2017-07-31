using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System;

namespace OpenSourceCooking.Controllers.StandardControllers.Database
{
    public class BugReportsController : Controller
    {
        private OpenSourceCookingEntities db = new OpenSourceCookingEntities();

        // GET: BugReports
        //public async Task<ActionResult> Index()
        //{
        //    var bugReports = db.BugReports.Include(b => b.AspNetUser);
        //    return View(await bugReports.ToListAsync());
        //}
        //
        //// GET: BugReports/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    BugReport bugReport = await db.BugReports.FindAsync(id);
        //    if (bugReport == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(bugReport);
        //}

        // GET: BugReports/Create
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.ReportingAspNetUserId = new SelectList(db.AspNetUsers, "Id", "Email");
            return View();
        }

        // POST: BugReports/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> Create([Bind(Include = "Id,BuggedUrl,Message,BrowserInfo")] BugReport bugReport)
        {
            if (ModelState.IsValid)
            {
                bugReport.ReportingAspNetUserId = User.Identity.GetUserId();
                bugReport.CreateDate = DateTime.UtcNow;
                db.BugReports.Add(bugReport);
                await db.SaveChangesAsync();
                return RedirectToAction("Confirmation",new {id= bugReport.Id });
            }

            ViewBag.ReportingAspNetUserId = new SelectList(db.AspNetUsers, "Id", "Email", bugReport.ReportingAspNetUserId);
            return View(bugReport);
        }

        // GET: BugReports/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    BugReport bugReport = await db.BugReports.FindAsync(id);
        //    if (bugReport == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.ReportingAspNetUserId = new SelectList(db.AspNetUsers, "Id", "Email", bugReport.ReportingAspNetUserId);
        //    return View(bugReport);
        //}
        //
        //// POST: BugReports/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "Id,ReportingAspNetUserId,BuggedUrl,Message,BrowserInfo,CreateDate")] BugReport bugReport)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(bugReport).State = EntityState.Modified;
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.ReportingAspNetUserId = new SelectList(db.AspNetUsers, "Id", "Email", bugReport.ReportingAspNetUserId);
        //    return View(bugReport);
        //}
        //
        //// GET: BugReports/Delete/5
        //public async Task<ActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    BugReport bugReport = await db.BugReports.FindAsync(id);
        //    if (bugReport == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(bugReport);
        //}
        //
        //// POST: BugReports/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteConfirmed(int id)
        //{
        //    BugReport bugReport = await db.BugReports.FindAsync(id);
        //    db.BugReports.Remove(bugReport);
        //    await db.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}

        // GET: BugReports/Confirmation/5
        public async Task<ActionResult> Confirmation(int? id)
        {
            if (id == null)            
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);            
            BugReport bugReport = await db.BugReports.FindAsync(id);
            if (bugReport == null)            
                return HttpNotFound();            
            return View(bugReport);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
