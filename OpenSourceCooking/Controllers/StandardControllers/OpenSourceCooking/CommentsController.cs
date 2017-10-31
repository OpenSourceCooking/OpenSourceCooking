using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using OpenSourceCooking.Models;

namespace OpenSourceCooking.Controllers.StandardControllers
{
    public class CommentsController : Controller
    {        
        OpenSourceCookingEntities db = new OpenSourceCookingEntities();
        const int CommentsPageSize = 10;

        #region Ajax Functions
        [Authorize]
        public async Task<JsonResult> AjaxCreateRecipeComment(int recipeId, string text)
        {
            string AspNetId = User.Identity.GetUserId();
            Recipe Recipe = await db.Recipes.FindAsync(recipeId);
            Comment Comment = new Comment()
            {
                CreatorId = AspNetId,
                CreateDateUtc = DateTime.UtcNow,
                Text = ConvertLineBrakes(text),
            };
            Recipe.Comments.Add(Comment);
            db.Entry(Recipe).State = EntityState.Modified;
            await db.SaveChangesAsync();
            AspNetUser AspNetUser = await db.AspNetUsers.FindAsync(AspNetId);
            //Dont send an email notification if commenting on your own recipe and make sure notifications are enabled
            if(AspNetUser.Id != Recipe.CreatorId && AspNetUser.Chef.IsRecipeCommentEmailNotificationEnabled)
            {
                string BaseURL = Request.Url.Scheme + Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
                IdentityMessage IdentityMessage = new IdentityMessage
                {
                    Destination = Recipe.AspNetUser.Email,
                    Subject = AspNetUser.UserName.Truncate(30) + " commented on " + Recipe.Name.Truncate(30),
                    Body = "<p>" + AspNetUser.UserName + " commented on <a href=\"" + BaseURL + "/Recipes?recipeId="+Recipe.Id+"\">" + Recipe.Name + "</a></p><br/><p>" + text + "</p>"                    
                };
                EmailService EmailService = new EmailService();                
                await EmailService.SendAsync(IdentityMessage);
            }            
            CommentDataTransferObject RecipeCommentDataTransferObject = new CommentDataTransferObject()
            {
                Id = Comment.Id,
                CreateDateUtc = Comment.CreateDateUtc,
                CreatorId = Comment.CreatorId,
                EditDateUtc = Comment.EditDateUtc,
                Text = Comment.Text,
                PostedByChefName = AspNetUser.UserName,
                IsMyRecipe = true
            };
            return Json(RecipeCommentDataTransferObject, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> AjaxGetRecipeComments(int recipeId, int commentsPageIndex, int recipeCommentsSkipAdjust)
        {
            string AspNetId = User.Identity.GetUserId();
            List<CommentDataTransferObject> RecipeCommentDataTransferObjects = await db.Comments.Where(x => x.Recipe.Id == recipeId).OrderByDescending(x => x.CreateDateUtc).Skip((commentsPageIndex * CommentsPageSize) + recipeCommentsSkipAdjust).Take(CommentsPageSize).Select(x => new CommentDataTransferObject
            {
                Id = x.Id,
                CreateDateUtc = x.CreateDateUtc,
                CreatorId = x.CreatorId,
                EditDateUtc = x.EditDateUtc,
                Text = x.Text,
                ParentCommentId = x.ParentCommentId,
                CommentVotes = x.CommentVotes.Select(v => new CommentVoteDataTransferObject
                {
                    CommentId = v.CommentId,
                    VoteValue = v.VoteValue,
                    //VoterId = v.VoterId, Not needed for the view
                }).ToList(),
                MyVote = x.CommentVotes.Where(v => v.VoterId == AspNetId).Select(v => new CommentVoteDataTransferObject
                {
                    CommentId = v.CommentId,
                    VoteValue = v.VoteValue,
                    //VoterId = v.VoterId, Not needed for the view
                }).FirstOrDefault(),
                PostedByChefName = x.AspNetUser.UserName,
                IsMyRecipe = AspNetId == x.CreatorId ? true : false,
            }).ToListAsync();
            foreach (CommentDataTransferObject CommentDataTransferObjects in RecipeCommentDataTransferObjects)
                if (CommentDataTransferObjects.CommentVotes.Count() > 0)
                    CommentDataTransferObjects.TotalVoteValue = CommentDataTransferObjects.CommentVotes.Sum(v => v.VoteValue);
                else
                    CommentDataTransferObjects.TotalVoteValue = 0;
            return Json(RecipeCommentDataTransferObjects, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public async Task<JsonResult> AjaxDeleteComment(int commentId)
        {
            string AspNetId = User.Identity.GetUserId();
            Comment Comment = await db.Comments.FindAsync(commentId);
            if (AspNetId != Comment.CreatorId)
                return Json("Not your comment! Stop hacking please", JsonRequestBehavior.AllowGet);
            db.Comments.Remove(Comment);
            await db.SaveChangesAsync();
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public async Task<JsonResult> AjaxEditComment(int commentId, string text)
        {
            string AspNetId = User.Identity.GetUserId();
            Comment Comment = await db.Comments.FindAsync(commentId);
            if (AspNetId != Comment.CreatorId)
                return Json("Not your comment! Stop hacking please", JsonRequestBehavior.AllowGet);
            Comment.Text = ConvertLineBrakes(text);
            db.Entry(Comment).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return Json(Comment.Text, JsonRequestBehavior.AllowGet);
        }
        #endregion

        static string ConvertLineBrakes(string text)
        {
            return text.TrimEnd(Environment.NewLine.ToCharArray()).Replace("\n", "<br/>");
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