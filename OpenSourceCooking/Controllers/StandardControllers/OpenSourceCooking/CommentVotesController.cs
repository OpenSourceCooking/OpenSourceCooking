using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace OpenSourceCooking.Controllers.StandardControllers
{
    public class CommentVotesController : Controller
    {
        private OpenSourceCookingEntities db = new OpenSourceCookingEntities();

        #region Ajax Functions
        [Authorize]
        public async Task<JsonResult> AjaxCreateCommentVote(int commentId, bool isUpVote)
        {
            string AspNetId = User.Identity.GetUserId();
            int ReturnVoteValue;
            CommentVote CurrentCommentVote = await db.CommentVotes.FindAsync(commentId, AspNetId);
            if (CurrentCommentVote == null)
            {
                //Create New CurrentCommentVote
                CommentVote CommentVote = new CommentVote()
                {
                    CommentId = commentId,
                    VoterId = AspNetId,
                    VoteValue = isUpVote ? (short)1 : (short)-1
                };
                db.CommentVotes.Add(CommentVote);
                ReturnVoteValue = CommentVote.VoteValue;
            }
            else
            {
                //Update CurrentCommentVote
                if (CurrentCommentVote.VoteValue < 0 && isUpVote)//Changed DownVote to UpVote
                    CurrentCommentVote.VoteValue = 1;
                else if (CurrentCommentVote.VoteValue > 0 && !isUpVote)//Changed UpVote to DownVote
                    CurrentCommentVote.VoteValue = -1;
                else
                {
                    //Delete the CurrentCommentVote
                    int DeletedVoteValue = CurrentCommentVote.VoteValue * -1;
                    db.CommentVotes.Remove(CurrentCommentVote);
                    await db.SaveChangesAsync();
                    return Json("Deleted " + DeletedVoteValue, JsonRequestBehavior.AllowGet);
                }
                ReturnVoteValue = CurrentCommentVote.VoteValue * 2; //Vote was reversed so we need to doulbe the value the other way
            }
            await db.SaveChangesAsync();
            return Json(ReturnVoteValue, JsonRequestBehavior.AllowGet);
        }
        #endregion
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
