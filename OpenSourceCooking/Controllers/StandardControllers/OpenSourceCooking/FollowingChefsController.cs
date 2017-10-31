using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Linq;

namespace OpenSourceCooking.Controllers.StandardControllers.OpenSourceCooking
{
    [Authorize]
    public class FollowingChefsController : Controller
    {
        OpenSourceCookingEntities db = new OpenSourceCookingEntities();

        public async Task<JsonResult> AjaxFollowRecipeChef(int recipeId)
        {
            string AspNetId = User.Identity.GetUserId();
            string RecipeChefId = await db.Recipes.Where(x=>x.Id == recipeId).Select(x=>x.CreatorId).FirstOrDefaultAsync();
            if(AspNetId == RecipeChefId)            
                return Json("You can not follow yourself", JsonRequestBehavior.AllowGet);
            FollowingChef FollowingChef = new FollowingChef()
            {
                AspNetUserId = AspNetId,
                FollowingChefId = RecipeChefId,
            };
            db.FollowingChefs.Add(FollowingChef);
            await db.SaveChangesAsync();
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Unfollow")]
        public async Task<JsonResult> AjaxUnfollowRecipeChef(int recipeId)
        {
            string AspNetId = User.Identity.GetUserId();
            string RecipeChefId = await db.Recipes.Where(x => x.Id == recipeId).Select(x => x.CreatorId).FirstOrDefaultAsync();
            FollowingChef FollowingChef = await db.FollowingChefs.FindAsync(AspNetId, RecipeChefId);
            db.FollowingChefs.Remove(FollowingChef);
            await db.SaveChangesAsync();
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        /*
        // GET: FollowingChefs
        public async Task<ActionResult> Index()
        {
            var followingChefs = db.FollowingChefs.Include(f => f.Chef).Include(f => f.Chef1);
            return View(await followingChefs.ToListAsync());
        }

        // GET: FollowingChefs/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FollowingChef followingChef = await db.FollowingChefs.FindAsync(id);
            if (followingChef == null)
            {
                return HttpNotFound();
            }
            return View(followingChef);
        }

        // GET: FollowingChefs/Create
        public ActionResult Create()
        {
            ViewBag.AspNetUserId = new SelectList(db.Chefs, "AspNetUserId", "AspNetUserId");
            ViewBag.FollowingChefId = new SelectList(db.Chefs, "AspNetUserId", "AspNetUserId");
            return View();
        }

        // POST: FollowingChefs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "AspNetUserId,FollowingChefId,CreateDate")] FollowingChef followingChef)
        {
            if (ModelState.IsValid)
            {
                db.FollowingChefs.Add(followingChef);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.AspNetUserId = new SelectList(db.Chefs, "AspNetUserId", "AspNetUserId", followingChef.AspNetUserId);
            ViewBag.FollowingChefId = new SelectList(db.Chefs, "AspNetUserId", "AspNetUserId", followingChef.FollowingChefId);
            return View(followingChef);
        }

        // GET: FollowingChefs/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FollowingChef followingChef = await db.FollowingChefs.FindAsync(id);
            if (followingChef == null)
            {
                return HttpNotFound();
            }
            ViewBag.AspNetUserId = new SelectList(db.Chefs, "AspNetUserId", "AspNetUserId", followingChef.AspNetUserId);
            ViewBag.FollowingChefId = new SelectList(db.Chefs, "AspNetUserId", "AspNetUserId", followingChef.FollowingChefId);
            return View(followingChef);
        }

        // POST: FollowingChefs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "AspNetUserId,FollowingChefId,CreateDate")] FollowingChef followingChef)
        {
            if (ModelState.IsValid)
            {
                db.Entry(followingChef).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.AspNetUserId = new SelectList(db.Chefs, "AspNetUserId", "AspNetUserId", followingChef.AspNetUserId);
            ViewBag.FollowingChefId = new SelectList(db.Chefs, "AspNetUserId", "AspNetUserId", followingChef.FollowingChefId);
            return View(followingChef);
        }

        // GET: FollowingChefs/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FollowingChef followingChef = await db.FollowingChefs.FindAsync(id);
            if (followingChef == null)
            {
                return HttpNotFound();
            }
            return View(followingChef);
        }

        // POST: FollowingChefs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            FollowingChef followingChef = await db.FollowingChefs.FindAsync(id);
            db.FollowingChefs.Remove(followingChef);
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
    }
}
