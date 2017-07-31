using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;

namespace OpenSourceCooking.Controllers.StandardControllers.Database
{
    [Authorize(Roles = "Admin")]
    public class DBRecipeStepsController : Controller
    {
        private OpenSourceCookingEntities db = new OpenSourceCookingEntities();

        // GET: DB_RecipeSteps
        public async Task<ActionResult> Index()
        {
            var recipeSteps = db.RecipeSteps.Include(r => r.Recipe);
            return View(await recipeSteps.ToListAsync());
        }

        // GET: DB_RecipeSteps/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RecipeStep recipeStep = await db.RecipeSteps.FindAsync(id);
            if (recipeStep == null)
            {
                return HttpNotFound();
            }
            return View(recipeStep);
        }

        // GET: DB_RecipeSteps/Create
        public ActionResult Create()
        {
            ViewBag.RecipeId = new SelectList(db.Recipes, "Id", "Name");
            return View();
        }

        // POST: DB_RecipeSteps/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "RecipeId,StepNumber,Comment,EstimatedTimeInSeconds,CreateDate")] RecipeStep recipeStep)
        {
            if (ModelState.IsValid)
            {
                db.RecipeSteps.Add(recipeStep);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.RecipeId = new SelectList(db.Recipes, "Id", "Name", recipeStep.RecipeId);
            return View(recipeStep);
        }

        // GET: DB_RecipeSteps/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RecipeStep recipeStep = await db.RecipeSteps.FindAsync(id);
            if (recipeStep == null)
            {
                return HttpNotFound();
            }
            ViewBag.RecipeId = new SelectList(db.Recipes, "Id", "Name", recipeStep.RecipeId);
            return View(recipeStep);
        }

        // POST: DB_RecipeSteps/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "RecipeId,StepNumber,Comment,EstimatedTimeInSeconds,CreateDate")] RecipeStep recipeStep)
        {
            if (ModelState.IsValid)
            {
                db.Entry(recipeStep).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.RecipeId = new SelectList(db.Recipes, "Id", "Name", recipeStep.RecipeId);
            return View(recipeStep);
        }

        // GET: DB_RecipeSteps/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RecipeStep recipeStep = await db.RecipeSteps.FindAsync(id);
            if (recipeStep == null)
            {
                return HttpNotFound();
            }
            return View(recipeStep);
        }

        // POST: DB_RecipeSteps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            RecipeStep recipeStep = await db.RecipeSteps.FindAsync(id);
            db.RecipeSteps.Remove(recipeStep);
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
