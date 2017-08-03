using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;

namespace OpenSourceCooking.Controllers.StandardControllers.Database
{
    public class DBIngredientsController : Controller
    {
        private OpenSourceCookingEntities db = new OpenSourceCookingEntities();

        // GET: DBIngredients
        public async Task<ActionResult> Index()
        {
            var ingredients = db.Ingredients.Include(i => i.AspNetUser).Include(i => i.CloudFile);
            return View(await ingredients.ToListAsync());
        }

        // GET: DBIngredients/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ingredient ingredient = await db.Ingredients.FindAsync(id);
            if (ingredient == null)
            {
                return HttpNotFound();
            }
            return View(ingredient);
        }

        // GET: DBIngredients/Create
        public ActionResult Create()
        {
            ViewBag.CreatorId = new SelectList(db.AspNetUsers, "Id", "Email");
            ViewBag.CloudFileId = new SelectList(db.CloudFiles, "Id", "Url");
            return View();
        }

        // POST: DBIngredients/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "IngredientName,CloudFileId,CreatorId,CreateDate")] Ingredient ingredient)
        {
            if (ModelState.IsValid)
            {
                db.Ingredients.Add(ingredient);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.CreatorId = new SelectList(db.AspNetUsers, "Id", "Email", ingredient.CreatorId);
            ViewBag.CloudFileId = new SelectList(db.CloudFiles, "Id", "Url", ingredient.CloudFileId);
            return View(ingredient);
        }

        // GET: DBIngredients/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ingredient ingredient = await db.Ingredients.FindAsync(id);
            if (ingredient == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatorId = new SelectList(db.AspNetUsers, "Id", "Email", ingredient.CreatorId);
            ViewBag.CloudFileId = new SelectList(db.CloudFiles, "Id", "Url", ingredient.CloudFileId);
            return View(ingredient);
        }

        // POST: DBIngredients/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "IngredientName,CloudFileId,CreatorId,CreateDate")] Ingredient ingredient)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ingredient).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.CreatorId = new SelectList(db.AspNetUsers, "Id", "Email", ingredient.CreatorId);
            ViewBag.CloudFileId = new SelectList(db.CloudFiles, "Id", "Url", ingredient.CloudFileId);
            return View(ingredient);
        }

        // GET: DBIngredients/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ingredient ingredient = await db.Ingredients.FindAsync(id);
            if (ingredient == null)
            {
                return HttpNotFound();
            }
            return View(ingredient);
        }

        // POST: DBIngredients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            Ingredient ingredient = await db.Ingredients.FindAsync(id);
            db.Ingredients.Remove(ingredient);
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
