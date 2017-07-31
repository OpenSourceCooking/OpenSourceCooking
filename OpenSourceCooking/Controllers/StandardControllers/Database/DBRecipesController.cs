using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;

namespace OpenSourceCooking.Controllers.StandardControllers.Database
{
    public class DBRecipesController : Controller
    {
        private OpenSourceCookingEntities db = new OpenSourceCookingEntities();

        // GET: DBRecipes
        public async Task<ActionResult> Index()
        {
            var recipes = db.Recipes.Include(r => r.AspNetUser).Include(r => r.RecipeViewableType);
            return View(await recipes.ToListAsync());
        }

        // GET: DBRecipes/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Recipe recipe = await db.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return HttpNotFound();
            }
            return View(recipe);
        }

        // GET: DBRecipes/Create
        public ActionResult Create()
        {
            ViewBag.CreatorId = new SelectList(db.AspNetUsers, "Id", "Email");
            ViewBag.ViewableType = new SelectList(db.RecipeViewableTypes, "Name", "Name");
            return View();
        }

        // POST: DBRecipes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,CreationStep,Description,ServingSize,CreatorId,CreateDate,LastEditDate,ViewableType")] Recipe recipe)
        {
            if (ModelState.IsValid)
            {
                db.Recipes.Add(recipe);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.CreatorId = new SelectList(db.AspNetUsers, "Id", "Email", recipe.CreatorId);
            ViewBag.ViewableType = new SelectList(db.RecipeViewableTypes, "Name", "Name", recipe.ViewableType);
            return View(recipe);
        }

        // GET: DBRecipes/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Recipe recipe = await db.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatorId = new SelectList(db.AspNetUsers, "Id", "Email", recipe.CreatorId);
            ViewBag.ViewableType = new SelectList(db.RecipeViewableTypes, "Name", "Name", recipe.ViewableType);
            return View(recipe);
        }

        // POST: DBRecipes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,CreationStep,Description,ServingSize,CreatorId,CreateDate,LastEditDate,ViewableType")] Recipe recipe)
        {
            if (ModelState.IsValid)
            {
                db.Entry(recipe).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.CreatorId = new SelectList(db.AspNetUsers, "Id", "Email", recipe.CreatorId);
            ViewBag.ViewableType = new SelectList(db.RecipeViewableTypes, "Name", "Name", recipe.ViewableType);
            return View(recipe);
        }

        // GET: DBRecipes/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Recipe recipe = await db.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return HttpNotFound();
            }
            return View(recipe);
        }

        // POST: DBRecipes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Recipe recipe = await db.Recipes.FindAsync(id);
            db.Recipes.Remove(recipe);
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
