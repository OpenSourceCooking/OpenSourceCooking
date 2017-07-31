using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;

namespace OpenSourceCooking.Controllers.StandardControllers.Database
{
    [Authorize(Roles = "Admin")]
    public class DBCloudFilesController : Controller
    {
        private OpenSourceCookingEntities db = new OpenSourceCookingEntities();

        // GET: DBCloudFiles
        public async Task<ActionResult> Index()
        {
            var cloudFiles = db.CloudFiles.Include(c => c.AspNetUser);
            return View(await cloudFiles.ToListAsync());
        }

        // GET: DBCloudFiles/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CloudFile cloudFile = await db.CloudFiles.FindAsync(id);
            if (cloudFile == null)
            {
                return HttpNotFound();
            }
            return View(cloudFile);
        }

        // GET: DBCloudFiles/Create
        public ActionResult Create()
        {
            ViewBag.CreatorId = new SelectList(db.AspNetUsers, "Id", "Email");
            return View();
        }

        // POST: DBCloudFiles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,FileType,Url,CreatorId")] CloudFile cloudFile)
        {
            if (ModelState.IsValid)
            {
                db.CloudFiles.Add(cloudFile);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.CreatorId = new SelectList(db.AspNetUsers, "Id", "Email", cloudFile.CreatorId);
            return View(cloudFile);
        }

        // GET: DBCloudFiles/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CloudFile cloudFile = await db.CloudFiles.FindAsync(id);
            if (cloudFile == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatorId = new SelectList(db.AspNetUsers, "Id", "Email", cloudFile.CreatorId);
            return View(cloudFile);
        }

        // POST: DBCloudFiles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,FileType,Url,CreatorId")] CloudFile cloudFile)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cloudFile).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.CreatorId = new SelectList(db.AspNetUsers, "Id", "Email", cloudFile.CreatorId);
            return View(cloudFile);
        }

        // GET: DBCloudFiles/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CloudFile cloudFile = await db.CloudFiles.FindAsync(id);
            if (cloudFile == null)
            {
                return HttpNotFound();
            }
            return View(cloudFile);
        }

        // POST: DBCloudFiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            CloudFile cloudFile = await db.CloudFiles.FindAsync(id);
            db.CloudFiles.Remove(cloudFile);
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
