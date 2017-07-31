using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;

namespace OpenSourceCooking.Controllers.StandardControllers.Database
{
    [Authorize(Roles = "Admin")]
    public class DBFlagsController : Controller
    {
        private OpenSourceCookingEntities db = new OpenSourceCookingEntities();

        // GET: DBFlags
        public async Task<ActionResult> Index()
        {
            return View(await db.Flags.ToListAsync());
        }

        // GET: DBFlags/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Flag flag = await db.Flags.FindAsync(id);
            if (flag == null)
            {
                return HttpNotFound();
            }
            return View(flag);
        }

        // GET: DBFlags/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DBFlags/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "FlagName")] Flag flag)
        {
            if (ModelState.IsValid)
            {
                db.Flags.Add(flag);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(flag);
        }

        // GET: DBFlags/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Flag flag = await db.Flags.FindAsync(id);
            if (flag == null)
            {
                return HttpNotFound();
            }
            return View(flag);
        }

        // POST: DBFlags/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "FlagName")] Flag flag)
        {
            if (ModelState.IsValid)
            {
                db.Entry(flag).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(flag);
        }

        // GET: DBFlags/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Flag flag = await db.Flags.FindAsync(id);
            if (flag == null)
            {
                return HttpNotFound();
            }
            return View(flag);
        }

        // POST: DBFlags/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            Flag flag = await db.Flags.FindAsync(id);
            db.Flags.Remove(flag);
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
