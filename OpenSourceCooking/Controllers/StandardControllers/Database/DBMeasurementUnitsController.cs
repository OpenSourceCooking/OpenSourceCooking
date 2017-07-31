using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Linq;

namespace OpenSourceCooking.Controllers.StandardControllers.Database
{
    [Authorize(Roles = "Admin")]
    public class DBMeasurementUnitsController : Controller
    {
        private OpenSourceCookingEntities db = new OpenSourceCookingEntities();

        // GET: DB_MeasurementUnits
        public async Task<ActionResult> Index()
        {
            var measurementUnits = db.MeasurementUnits.OrderBy(x=>x.MeasurementTypeName).ThenBy(x=>x.MeasurementUnitName).Include(m => m.MeasurementType);
            return View(await measurementUnits.ToListAsync());
        }

        // GET: DB_MeasurementUnits/Details/5
        public async Task<ActionResult> Details(string measurementUnitName, string measurementTypeName)
        {
            MeasurementUnit measurementUnit = await db.MeasurementUnits.FindAsync(measurementUnitName, measurementTypeName);
            if (measurementUnit == null)
            {
                return HttpNotFound();
            }
            return View(measurementUnit);
        }

        // GET: DB_MeasurementUnits/Create
        public ActionResult Create()
        {
            ViewBag.MeasurementTypeName = new SelectList(db.MeasurementTypes, "MeasurementTypeName", "MeasurementTypeName");
            return View();
        }

        // POST: DB_MeasurementUnits/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "MeasurementUnitName,MeasurementTypeName")] MeasurementUnit measurementUnit)
        {
            if (ModelState.IsValid)
            {
                db.MeasurementUnits.Add(measurementUnit);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.MeasurementTypeName = new SelectList(db.MeasurementTypes, "MeasurementTypeName", "MeasurementTypeName", measurementUnit.MeasurementTypeName);
            return View(measurementUnit);
        }

        // GET: DB_MeasurementUnits/Edit/5
        public async Task<ActionResult> Edit(string measurementUnitName, string measurementTypeName)
        {
            MeasurementUnit measurementUnit = await db.MeasurementUnits.FindAsync(measurementUnitName, measurementTypeName);
            if (measurementUnit == null)
            {
                return HttpNotFound();
            }
            ViewBag.MeasurementTypeName = new SelectList(db.MeasurementTypes, "MeasurementTypeName", "MeasurementTypeName", measurementUnit.MeasurementTypeName);
            return View(measurementUnit);
        }

        // POST: DB_MeasurementUnits/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "MeasurementUnitName,MeasurementTypeName")] MeasurementUnit measurementUnit)
        {
            if (ModelState.IsValid)
            {
                db.Entry(measurementUnit).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.MeasurementTypeName = new SelectList(db.MeasurementTypes, "MeasurementTypeName", "MeasurementTypeName", measurementUnit.MeasurementTypeName);
            return View(measurementUnit);
        }

        // GET: DB_MeasurementUnits/Delete/5
        public async Task<ActionResult> Delete(string measurementUnitName, string measurementTypeName)
        {
            MeasurementUnit measurementUnit = await db.MeasurementUnits.FindAsync(measurementUnitName, measurementTypeName);
            if (measurementUnit == null)
            {
                return HttpNotFound();
            }
            return View(measurementUnit);
        }

        // POST: DB_MeasurementUnits/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string measurementUnitName, string measurementTypeName)
        {
            MeasurementUnit measurementUnit = await db.MeasurementUnits.FindAsync(measurementUnitName, measurementTypeName);
            db.MeasurementUnits.Remove(measurementUnit);
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
