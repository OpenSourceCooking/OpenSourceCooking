using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;

namespace OpenSourceCooking.Controllers.StandardControllers.Database
{
    [Authorize(Roles = "Admin")]
    public class DBRecipeFlagsController : Controller
    {
        private OpenSourceCookingEntities db = new OpenSourceCookingEntities();

        // GET: DBRecipeFlags
        public async Task<ActionResult> Index()
        {
            var recipeFlags = db.RecipeFlags.Include(r => r.AspNetUser).Include(r => r.Flag).Include(r => r.Recipe);
            return View(await recipeFlags.ToListAsync());
        }

        // GET: DBRecipeFlags/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RecipeFlag recipeFlag = await db.RecipeFlags.FindAsync(id);
            if (recipeFlag == null)
            {
                return HttpNotFound();
            }
            return View(recipeFlag);
        }

        // GET: DBRecipeFlags/Create
        public ActionResult Create()
        {
            ViewBag.ReportingAspNetUserId = new SelectList(db.AspNetUsers, "Id", "Email");
            ViewBag.FlagName = new SelectList(db.Flags, "FlagName", "FlagName");
            ViewBag.RecipeId = new SelectList(db.Recipes, "Id", "Name");
            return View();
        }

        // POST: DBRecipeFlags/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "RecipeId,ReportingAspNetUserId,FlagName,CreateDate")] RecipeFlag recipeFlag)
        {
            if (ModelState.IsValid)
            {
                db.RecipeFlags.Add(recipeFlag);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.ReportingAspNetUserId = new SelectList(db.AspNetUsers, "Id", "Email", recipeFlag.ReportingAspNetUserId);
            ViewBag.FlagName = new SelectList(db.Flags, "FlagName", "FlagName", recipeFlag.FlagName);
            ViewBag.RecipeId = new SelectList(db.Recipes, "Id", "Name", recipeFlag.RecipeId);
            return View(recipeFlag);
        }

        // GET: DBRecipeFlags/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RecipeFlag recipeFlag = await db.RecipeFlags.FindAsync(id);
            if (recipeFlag == null)
            {
                return HttpNotFound();
            }
            ViewBag.ReportingAspNetUserId = new SelectList(db.AspNetUsers, "Id", "Email", recipeFlag.ReportingAspNetUserId);
            ViewBag.FlagName = new SelectList(db.Flags, "FlagName", "FlagName", recipeFlag.FlagName);
            ViewBag.RecipeId = new SelectList(db.Recipes, "Id", "Name", recipeFlag.RecipeId);
            return View(recipeFlag);
        }

        // POST: DBRecipeFlags/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "RecipeId,ReportingAspNetUserId,FlagName,CreateDate")] RecipeFlag recipeFlag)
        {
            if (ModelState.IsValid)
            {
                db.Entry(recipeFlag).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.ReportingAspNetUserId = new SelectList(db.AspNetUsers, "Id", "Email", recipeFlag.ReportingAspNetUserId);
            ViewBag.FlagName = new SelectList(db.Flags, "FlagName", "FlagName", recipeFlag.FlagName);
            ViewBag.RecipeId = new SelectList(db.Recipes, "Id", "Name", recipeFlag.RecipeId);
            return View(recipeFlag);
        }

        // GET: DBRecipeFlags/Delete/5
        public async Task<ActionResult> Delete(int recipeId, string reportingAspNetUserId, string flagName)
        {
            RecipeFlag recipeFlag = await db.RecipeFlags.FindAsync(recipeId, reportingAspNetUserId, flagName);
            if (recipeFlag == null)
            {
                return HttpNotFound();
            }
            return View(recipeFlag);
        }

        // POST: DBRecipeFlags/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int recipeId, string reportingAspNetUserId, string flagName)
        {
            RecipeFlag recipeFlag = await db.RecipeFlags.FindAsync(recipeId, reportingAspNetUserId, flagName);
            db.RecipeFlags.Remove(recipeFlag);
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
