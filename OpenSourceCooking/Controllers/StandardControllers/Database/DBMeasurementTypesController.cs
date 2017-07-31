using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;

namespace OpenSourceCooking.Controllers.StandardControllers.Database
{
    [Authorize(Roles = "Admin")]
    public class DBMeasurementTypesController : Controller
    {
        private OpenSourceCookingEntities db = new OpenSourceCookingEntities();

        // GET: DB_MeasurementTypes
        public async Task<ActionResult> Index()
        {
            return View(await db.MeasurementTypes.ToListAsync());
        }

        // GET: DB_MeasurementTypes/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MeasurementType measurementType = await db.MeasurementTypes.FindAsync(id);
            if (measurementType == null)
            {
                return HttpNotFound();
            }
            return View(measurementType);
        }

        // GET: DB_MeasurementTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DB_MeasurementTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "MeasurementTypeName")] MeasurementType measurementType)
        {
            if (ModelState.IsValid)
            {
                db.MeasurementTypes.Add(measurementType);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(measurementType);
        }

        // GET: DB_MeasurementTypes/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MeasurementType measurementType = await db.MeasurementTypes.FindAsync(id);
            if (measurementType == null)
            {
                return HttpNotFound();
            }
            return View(measurementType);
        }

        // POST: DB_MeasurementTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "MeasurementTypeName")] MeasurementType measurementType)
        {
            if (ModelState.IsValid)
            {
                db.Entry(measurementType).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(measurementType);
        }

        // GET: DB_MeasurementTypes/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MeasurementType measurementType = await db.MeasurementTypes.FindAsync(id);
            if (measurementType == null)
            {
                return HttpNotFound();
            }
            return View(measurementType);
        }

        // POST: DB_MeasurementTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            MeasurementType measurementType = await db.MeasurementTypes.FindAsync(id);
            db.MeasurementTypes.Remove(measurementType);
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
