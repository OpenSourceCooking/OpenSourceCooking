using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;

namespace OpenSourceCooking.Controllers.StandardControllers.Database
{
    [Authorize(Roles = "Admin")]
    public class DBDietaryRestrictionsController : Controller
    {
        private OpenSourceCookingEntities db = new OpenSourceCookingEntities();

        // GET: DBDietaryRestrictions
        public async Task<ActionResult> Index()
        {
            return View(await db.DietaryRestrictions.ToListAsync());
        }

        // GET: DBDietaryRestrictions/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DietaryRestriction dietaryRestriction = await db.DietaryRestrictions.FindAsync(id);
            if (dietaryRestriction == null)
            {
                return HttpNotFound();
            }
            return View(dietaryRestriction);
        }

        // GET: DBDietaryRestrictions/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DBDietaryRestrictions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Name,IconUrl")] DietaryRestriction dietaryRestriction)
        {
            if (ModelState.IsValid)
            {
                db.DietaryRestrictions.Add(dietaryRestriction);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(dietaryRestriction);
        }

        // GET: DBDietaryRestrictions/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DietaryRestriction dietaryRestriction = await db.DietaryRestrictions.FindAsync(id);
            if (dietaryRestriction == null)
            {
                return HttpNotFound();
            }
            return View(dietaryRestriction);
        }

        // POST: DBDietaryRestrictions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Name,IconUrl")] DietaryRestriction dietaryRestriction)
        {
            if (ModelState.IsValid)
            {
                db.Entry(dietaryRestriction).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(dietaryRestriction);
        }

        // GET: DBDietaryRestrictions/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DietaryRestriction dietaryRestriction = await db.DietaryRestrictions.FindAsync(id);
            if (dietaryRestriction == null)
            {
                return HttpNotFound();
            }
            return View(dietaryRestriction);
        }

        // POST: DBDietaryRestrictions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            DietaryRestriction dietaryRestriction = await db.DietaryRestrictions.FindAsync(id);
            db.DietaryRestrictions.Remove(dietaryRestriction);
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
