using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;

namespace OpenSourceCooking.Controllers.StandardControllers.OpenSourceCooking
{
    public class RecipeFlagsController : Controller
    {
        private OpenSourceCookingEntities db = new OpenSourceCookingEntities();

        /*
        // GET: RecipeFlags
        public async Task<ActionResult> Index()
        {
            var recipeFlags = db.RecipeFlags.Include(r => r.AspNetUser).Include(r => r.Flag).Include(r => r.Recipe);
            return View(await recipeFlags.ToListAsync());
        }

        // GET: RecipeFlags/Details/5
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

        // GET: RecipeFlags/Create
        public ActionResult Create()
        {
            ViewBag.ReportingAspNetUserId = new SelectList(db.AspNetUsers, "Id", "Email");
            ViewBag.FlagName = new SelectList(db.Flags, "FlagName", "FlagName");
            ViewBag.RecipeId = new SelectList(db.Recipes, "Id", "Name");
            return View();
        }

        // POST: RecipeFlags/Create
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

        // GET: RecipeFlags/Edit/5
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

        // POST: RecipeFlags/Edit/5
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

        // GET: RecipeFlags/Delete/5
        public async Task<ActionResult> Delete(int? id)
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

        // POST: RecipeFlags/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            RecipeFlag recipeFlag = await db.RecipeFlags.FindAsync(id);
            db.RecipeFlags.Remove(recipeFlag);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        */

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [Authorize]
        public async Task<JsonResult> AjaxCreateRecipeFlag(int recipeId, string flagName)
        {
            string AspNetId = User.Identity.GetUserId();
            //Set recipe to secrete because it was reported
            Recipe Recipe = await db.Recipes.FindAsync(recipeId);
            if(Recipe.AspNetUser.Id == AspNetId)
                return Json("Can't Report You Own Recipe", JsonRequestBehavior.AllowGet);
            Recipe.ViewableType = "Secret";
            db.Entry(Recipe).State = EntityState.Modified;
            RecipeFlag CurrentRecipeFlag = Recipe.RecipeFlags.Where(x => x.RecipeId == recipeId && x.FlagName == flagName && x.ReportingAspNetUserId == AspNetId).FirstOrDefault();
            if (CurrentRecipeFlag != null)
                return Json("Already Reported", JsonRequestBehavior.AllowGet);            
            //Create the flag
            Flag Flag = await db.Flags.FindAsync(flagName);
            db.RecipeFlags.Add(new RecipeFlag {CreateDateUtc = DateTime.UtcNow, FlagName = flagName, RecipeId = recipeId, ReportingAspNetUserId = AspNetId });
            await db.SaveChangesAsync();
            return Json(true, JsonRequestBehavior.AllowGet);
        }       
    }
}