using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;

namespace OpenSourceCooking.Controllers.StandardControllers.Database
{
    [Authorize(Roles = "Admin")]
    public class DBBugReportsController : Controller
    {
        private OpenSourceCookingEntities db = new OpenSourceCookingEntities();

        // GET: DBBugReports
        public async Task<ActionResult> Index()
        {
            var bugReports = db.BugReports.Include(b => b.AspNetUser);
            return View(await bugReports.ToListAsync());
        }

        // GET: DBBugReports/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BugReport bugReport = await db.BugReports.FindAsync(id);
            if (bugReport == null)
            {
                return HttpNotFound();
            }
            return View(bugReport);
        }

        // GET: DBBugReports/Create
        public ActionResult Create()
        {
            ViewBag.ReportingAspNetUserId = new SelectList(db.AspNetUsers, "Id", "Email");
            return View();
        }

        // POST: DBBugReports/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,ReportingAspNetUserId,BuggedUrl,Message,BrowserInfo,CreateDate")] BugReport bugReport)
        {
            if (ModelState.IsValid)
            {
                db.BugReports.Add(bugReport);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.ReportingAspNetUserId = new SelectList(db.AspNetUsers, "Id", "Email", bugReport.ReportingAspNetUserId);
            return View(bugReport);
        }

        // GET: DBBugReports/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BugReport bugReport = await db.BugReports.FindAsync(id);
            if (bugReport == null)
            {
                return HttpNotFound();
            }
            ViewBag.ReportingAspNetUserId = new SelectList(db.AspNetUsers, "Id", "Email", bugReport.ReportingAspNetUserId);
            return View(bugReport);
        }

        // POST: DBBugReports/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,ReportingAspNetUserId,BuggedUrl,Message,BrowserInfo,CreateDate")] BugReport bugReport)
        {
            if (ModelState.IsValid)
            {
                db.Entry(bugReport).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.ReportingAspNetUserId = new SelectList(db.AspNetUsers, "Id", "Email", bugReport.ReportingAspNetUserId);
            return View(bugReport);
        }

        // GET: DBBugReports/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BugReport bugReport = await db.BugReports.FindAsync(id);
            if (bugReport == null)
            {
                return HttpNotFound();
            }
            return View(bugReport);
        }

        // POST: DBBugReports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            BugReport bugReport = await db.BugReports.FindAsync(id);
            db.BugReports.Remove(bugReport);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
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
