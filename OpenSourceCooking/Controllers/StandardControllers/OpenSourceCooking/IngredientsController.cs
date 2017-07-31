using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using OpenSourceCooking.Models;

namespace OpenSourceCooking.Controllers.StandardControllers
{
    public class IngredientsController : Controller
    {
        private OpenSourceCookingEntities db = new OpenSourceCookingEntities();

        // GET: Ingredients
        public async Task<ActionResult> Index()
        {
            var ingredients = db.Ingredients.Include(i => i.AspNetUser).Include(i => i.CloudFile);
            //This is to prevent img src breaking on nulls
            foreach (Ingredient ingredient in ingredients)
            {
                if (ingredient.CloudFile == null) 
                    ingredient.CloudFile = new CloudFile();
            }
            return View(await ingredients.ToListAsync());
        }

        // GET: Ingredients/Create
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Ingredients/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "IngredientName")] Ingredient ingredient, HttpPostedFileBase httpPostedFileBase)
        {
            string aspNetId = User.Identity.GetUserId();
            DateTime UTCTimeNow = DateTime.UtcNow;
            ingredient.CreateDate = UTCTimeNow;
            ingredient.CreatorId = aspNetId;
            if (ModelState.IsValid)
            {
                //Save Ingredient Cloud File
                if (httpPostedFileBase != null)
                {
                    int UploadedCloudFileId = await AzureCloudStorageWrapper.UploadCloudFile(AzureCloudStorageWrapper.AzureBlobContainer.ingredientcloudfiles, httpPostedFileBase, aspNetId, "Ingredient-" + ingredient.IngredientName, true);
                    if(UploadedCloudFileId == -1)//File size too big
                    {
                        ViewBag.FileWasTooBig = true;
                        return View(ingredient);
                    }                        
                    else
                        ingredient.CloudFileId = UploadedCloudFileId;
                }
                //Save Ingredient
                db.Ingredients.Add(ingredient);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(ingredient);
        }       

        protected override void Dispose(bool disposing)
        {
            if (disposing)            
                db.Dispose();            
            base.Dispose(disposing);
        }

        public JsonResult AjaxGetIngredients(int pageIndex, int pageSize)
        {
            List<IngredientDataTransferObject> Ingredients = (from i in db.Ingredients
                                               orderby i.IngredientName ascending
                                               select new IngredientDataTransferObject()
                                               {
                                                   IngredientName = i.IngredientName,
                                                   ImageUrl = i.CloudFile != null ? i.CloudFile.Url : "",
                                                   CreatorId = i.CreatorId,
                                                   CreateDate = i.CreateDate
                                               }).Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return Json(Ingredients, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public JsonResult AjaxIngredientsAutoComplete(string prefix)
        {
            //Using Anonymous here because the JQuery UI need the key/value named "label" & "val"
            var aIngredients = (db.Ingredients.Where(x => x.IngredientName.Contains(prefix)).OrderBy(x => x.IngredientName).Select(x => new
            {
                label = x.IngredientName,
                val = x.IngredientName
            })).Take(6).ToList();
            return Json(aIngredients, JsonRequestBehavior.AllowGet);
        }
    }
}
