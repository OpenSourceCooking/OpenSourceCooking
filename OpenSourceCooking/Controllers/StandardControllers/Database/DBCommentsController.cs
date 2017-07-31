using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;

namespace OpenSourceCooking.Controllers.StandardControllers.Database
{
    [Authorize(Roles = "Admin")]
    public class DBCommentsController : Controller
    {
        private OpenSourceCookingEntities db = new OpenSourceCookingEntities();

        // GET: DBComments
        public async Task<ActionResult> Index()
        {
            var comments = db.Comments.Include(c => c.AspNetUser).Include(c => c.Comment1);
            return View(await comments.ToListAsync());
        }

        // GET: DBComments/Details/5
        public async Task<ActionResult> Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = await db.Comments.FindAsync(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // GET: DBComments/Create
        public ActionResult Create()
        {
            ViewBag.CreatorId = new SelectList(db.AspNetUsers, "Id", "Email");
            ViewBag.ParentCommentId = new SelectList(db.Comments, "Id", "Text");
            return View();
        }

        // POST: DBComments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Text,CreatorId,CreateDate,EditDate,ParentCommentId")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.CreatorId = new SelectList(db.AspNetUsers, "Id", "Email", comment.CreatorId);
            ViewBag.ParentCommentId = new SelectList(db.Comments, "Id", "Text", comment.ParentCommentId);
            return View(comment);
        }

        // GET: DBComments/Edit/5
        public async Task<ActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = await db.Comments.FindAsync(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatorId = new SelectList(db.AspNetUsers, "Id", "Email", comment.CreatorId);
            ViewBag.ParentCommentId = new SelectList(db.Comments, "Id", "Text", comment.ParentCommentId);
            return View(comment);
        }

        // POST: DBComments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Text,CreatorId,CreateDate,EditDate,ParentCommentId")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(comment).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.CreatorId = new SelectList(db.AspNetUsers, "Id", "Email", comment.CreatorId);
            ViewBag.ParentCommentId = new SelectList(db.Comments, "Id", "Text", comment.ParentCommentId);
            return View(comment);
        }

        // GET: DBComments/Delete/5
        public async Task<ActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = await db.Comments.FindAsync(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // POST: DBComments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(long id)
        {
            Comment comment = await db.Comments.FindAsync(id);
            db.Comments.Remove(comment);
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
