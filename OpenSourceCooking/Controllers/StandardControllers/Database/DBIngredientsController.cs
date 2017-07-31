using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;

namespace OpenSourceCooking.Controllers.StandardControllers.Database
{
    [Authorize(Roles = "Admin")]
    public class DBIngredientsController : Controller
    {
        private OpenSourceCookingEntities db = new OpenSourceCookingEntities();

        // GET: DB_Ingredients
        public async Task<ActionResult> Index()
        {
            var ingredients = db.Ingredients.Include(i => i.AspNetUser).Include(i => i.CloudFile);
            return View(await ingredients.ToListAsync());
        }

        // GET: DB_Ingredients/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)            
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);            
            Ingredient ingredient = await db.Ingredients.FindAsync(id);
            if (ingredient == null)            
                return HttpNotFound();            
            return View(ingredient);
        }

        // GET: DB_Ingredients/Create
        public ActionResult Create()
        {
            ViewBag.CreatorId = new SelectList(db.AspNetUsers, "Id", "Email");
            ViewBag.CloudFilePicId = new SelectList(db.CloudFiles, "Id", "FileType");
            return View();
        }

        // POST: DB_Ingredients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "IngredientName,CloudFilePicId,CreatorId,CreateDate")] Ingredient ingredient)
        {
            if (ModelState.IsValid)
            {
                db.Ingredients.Add(ingredient);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.CreatorId = new SelectList(db.AspNetUsers, "Id", "Email", ingredient.CreatorId);
            ViewBag.CloudFilePicId = new SelectList(db.CloudFiles, "Id", "FileType", ingredient.CloudFileId);
            return View(ingredient);
        }

        // GET: DB_Ingredients/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)            
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);            
            Ingredient ingredient = await db.Ingredients.FindAsync(id);
            if (ingredient == null)            
                return HttpNotFound();
            ViewBag.CreatorId = new SelectList(db.AspNetUsers, "Id", "Email", ingredient.CreatorId);
            ViewBag.CloudFilePicId = new SelectList(db.CloudFiles, "Id", "FileType", ingredient.CloudFileId);
            return View(ingredient);
        }

        // POST: DB_Ingredients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "IngredientName,CloudFilePicId,CreatorId,CreateDate")] Ingredient ingredient)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ingredient).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.CreatorId = new SelectList(db.AspNetUsers, "Id", "Email", ingredient.CreatorId);
            ViewBag.CloudFilePicId = new SelectList(db.CloudFiles, "Id", "FileType", ingredient.CloudFileId);
            return View(ingredient);
        }

        // GET: DB_Ingredients/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)            
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);            
            Ingredient ingredient = await db.Ingredients.FindAsync(id);
            if (ingredient == null)            
                return HttpNotFound();            
            return View(ingredient);
        }

        // POST: DB_Ingredients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            Ingredient ingredient = await db.Ingredients.FindAsync(id);
            AzureCloudStorageWrapper.DeleteIngredientCloudFileIfExist(ingredient);
            db.Ingredients.Remove(ingredient);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)            
                db.Dispose();            
            base.Dispose(disposing);
        }
    }
}
