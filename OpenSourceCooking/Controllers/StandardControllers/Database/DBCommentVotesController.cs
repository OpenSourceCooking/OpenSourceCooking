using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;

namespace OpenSourceCooking.Controllers.StandardControllers.Database
{
    [Authorize(Roles = "Admin")]
    public class DBCommentVotesController : Controller
    {
        private OpenSourceCookingEntities db = new OpenSourceCookingEntities();

        // GET: CommentVotes
        public async Task<ActionResult> Index()
        {
            var commentVotes = db.CommentVotes.Include(c => c.AspNetUser).Include(c => c.Comment);
            return View(await commentVotes.ToListAsync());
        }

        // GET: CommentVotes/Details/5
        public async Task<ActionResult> Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CommentVote commentVote = await db.CommentVotes.FindAsync(id);
            if (commentVote == null)
            {
                return HttpNotFound();
            }
            return View(commentVote);
        }

        // GET: CommentVotes/Create
        public ActionResult Create()
        {
            ViewBag.VoterId = new SelectList(db.AspNetUsers, "Id", "Email");
            ViewBag.CommentId = new SelectList(db.Comments, "Id", "Text");
            return View();
        }

        // POST: CommentVotes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "CommentId,VoteValue,VoterId")] CommentVote commentVote)
        {
            if (ModelState.IsValid)
            {
                db.CommentVotes.Add(commentVote);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.VoterId = new SelectList(db.AspNetUsers, "Id", "Email", commentVote.VoterId);
            ViewBag.CommentId = new SelectList(db.Comments, "Id", "Text", commentVote.CommentId);
            return View(commentVote);
        }

        // GET: CommentVotes/Edit/5
        public async Task<ActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CommentVote commentVote = await db.CommentVotes.FindAsync(id);
            if (commentVote == null)
            {
                return HttpNotFound();
            }
            ViewBag.VoterId = new SelectList(db.AspNetUsers, "Id", "Email", commentVote.VoterId);
            ViewBag.CommentId = new SelectList(db.Comments, "Id", "Text", commentVote.CommentId);
            return View(commentVote);
        }

        // POST: CommentVotes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "CommentId,VoteValue,VoterId")] CommentVote commentVote)
        {
            if (ModelState.IsValid)
            {
                db.Entry(commentVote).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.VoterId = new SelectList(db.AspNetUsers, "Id", "Email", commentVote.VoterId);
            ViewBag.CommentId = new SelectList(db.Comments, "Id", "Text", commentVote.CommentId);
            return View(commentVote);
        }

        // GET: CommentVotes/Delete/5
        public async Task<ActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CommentVote commentVote = await db.CommentVotes.FindAsync(id);
            if (commentVote == null)
            {
                return HttpNotFound();
            }
            return View(commentVote);
        }

        // POST: CommentVotes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(long id)
        {
            CommentVote commentVote = await db.CommentVotes.FindAsync(id);
            db.CommentVotes.Remove(commentVote);
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
