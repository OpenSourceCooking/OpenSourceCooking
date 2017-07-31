using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;

namespace OpenSourceCooking.Controllers.StandardControllers.Database
{
    [Authorize(Roles = "Admin")]
    public class DBRecipeStepsIngredientsController : Controller
    {
        private OpenSourceCookingEntities db = new OpenSourceCookingEntities();

        // GET: DBRecipeStepsIngredients
        public async Task<ActionResult> Index()
        {
            var recipeStepsIngredients = db.RecipeStepsIngredients.Include(r => r.Ingredient).Include(r => r.MeasurementUnit).Include(r => r.RecipeStep);
            return View(await recipeStepsIngredients.ToListAsync());
        }

        // GET: DBRecipeStepsIngredients/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)            
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);            
            RecipeStepsIngredient recipeStepsIngredient = await db.RecipeStepsIngredients.FindAsync(id);
            if (recipeStepsIngredient == null)            
                return HttpNotFound();            
            return View(recipeStepsIngredient);
        }

        // GET: DBRecipeStepsIngredients/Create
        public ActionResult Create()
        {
            ViewBag.IngredientName = new SelectList(db.Ingredients, "IngredientName", "CreatorId");
            ViewBag.MeasurementUnitName = new SelectList(db.MeasurementUnits, "MeasurementUnitName", "MeasurementUnitName");
            ViewBag.RecipeId = new SelectList(db.RecipeSteps, "RecipeId", "Comment");
            return View();
        }

        // POST: DBRecipeStepsIngredients/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "RecipeId,StepNumber,IngredientName,MeasurementUnitName,MeasurementTypeName,Amount,ToAmount")] RecipeStepsIngredient recipeStepsIngredient)
        {
            if (ModelState.IsValid)
            {
                db.RecipeStepsIngredients.Add(recipeStepsIngredient);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.IngredientName = new SelectList(db.Ingredients, "IngredientName", "CreatorId", recipeStepsIngredient.IngredientName);
            ViewBag.MeasurementUnitName = new SelectList(db.MeasurementUnits, "MeasurementUnitName", "MeasurementUnitName", recipeStepsIngredient.MeasurementUnitName);
            ViewBag.RecipeId = new SelectList(db.RecipeSteps, "RecipeId", "Comment", recipeStepsIngredient.RecipeId);
            return View(recipeStepsIngredient);
        }

        // GET: DBRecipeStepsIngredients/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RecipeStepsIngredient recipeStepsIngredient = await db.RecipeStepsIngredients.FindAsync(id);
            if (recipeStepsIngredient == null)
            {
                return HttpNotFound();
            }
            ViewBag.IngredientName = new SelectList(db.Ingredients, "IngredientName", "CreatorId", recipeStepsIngredient.IngredientName);
            ViewBag.MeasurementUnitName = new SelectList(db.MeasurementUnits, "MeasurementUnitName", "MeasurementUnitName", recipeStepsIngredient.MeasurementUnitName);
            ViewBag.RecipeId = new SelectList(db.RecipeSteps, "RecipeId", "Comment", recipeStepsIngredient.RecipeId);
            return View(recipeStepsIngredient);
        }

        // POST: DBRecipeStepsIngredients/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "RecipeId,StepNumber,IngredientName,MeasurementUnitName,MeasurementTypeName,Amount,ToAmount")] RecipeStepsIngredient recipeStepsIngredient)
        {
            if (ModelState.IsValid)
            {
                db.Entry(recipeStepsIngredient).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.IngredientName = new SelectList(db.Ingredients, "IngredientName", "CreatorId", recipeStepsIngredient.IngredientName);
            ViewBag.MeasurementUnitName = new SelectList(db.MeasurementUnits, "MeasurementUnitName", "MeasurementUnitName", recipeStepsIngredient.MeasurementUnitName);
            ViewBag.RecipeId = new SelectList(db.RecipeSteps, "RecipeId", "Comment", recipeStepsIngredient.RecipeId);
            return View(recipeStepsIngredient);
        }

        // GET: DBRecipeStepsIngredients/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RecipeStepsIngredient recipeStepsIngredient = await db.RecipeStepsIngredients.FindAsync(id);
            if (recipeStepsIngredient == null)
            {
                return HttpNotFound();
            }
            return View(recipeStepsIngredient);
        }

        // POST: DBRecipeStepsIngredients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            RecipeStepsIngredient recipeStepsIngredient = await db.RecipeStepsIngredients.FindAsync(id);
            db.RecipeStepsIngredients.Remove(recipeStepsIngredient);
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
