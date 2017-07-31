using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;
using OpenSourceCooking.Models;
using System.Linq;

namespace OpenSourceCooking.Controllers.StandardControllers.OpenSourceCooking
{
    public class DietaryRestrictionsController : Controller
    {
        private OpenSourceCookingEntities db = new OpenSourceCookingEntities();

        /*
        // GET: DietaryRestrictions
        public async Task<ActionResult> Index()
        {
            return View(await db.DietaryRestrictions.ToListAsync());
        }

        // GET: DietaryRestrictions/Details/5
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

        // GET: DietaryRestrictions/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DietaryRestrictions/Create
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

        // GET: DietaryRestrictions/Edit/5
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

        // POST: DietaryRestrictions/Edit/5
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

        // GET: DietaryRestrictions/Delete/5
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

        // POST: DietaryRestrictions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            DietaryRestriction dietaryRestriction = await db.DietaryRestrictions.FindAsync(id);
            db.DietaryRestrictions.Remove(dietaryRestriction);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        */

        public async Task<JsonResult> AjaxGetDietaryRestrictions()
        {
            var DietaryRestrictionDataTransferObjects = await db.DietaryRestrictions.Select(x=> new DietaryRestrictionDataTransferObject
            {
                Name = x.Name,
                IconUrl = x.IconUrl
            }).ToListAsync();
            return Json(DietaryRestrictionDataTransferObjects, JsonRequestBehavior.AllowGet);
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
