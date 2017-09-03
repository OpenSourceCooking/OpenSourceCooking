using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using OpenSourceCooking.Models;
using System.Collections.ObjectModel;
using System.Globalization;
using LinqKit;

namespace OpenSourceCooking.Controllers.StandardControllers
{
    public class RecipesController : Controller
    {
        OpenSourceCookingEntities db = new OpenSourceCookingEntities();
        const int PageSize = 24;

        public ActionResult Index(int? recipeId, string searchText)
        {
            ViewBag.RecipeId = recipeId ?? 0;
            ViewBag.SearchText = searchText;
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }

        [Authorize]
        public async Task<ActionResult> RecipeEditor(int? recipeId)
        {
            ViewBag.RecipeId = recipeId ?? 0;
            if (ViewBag.RecipeId > 0)
            {
                string AspNetId = User.Identity.GetUserId();
                string RecipeCreatorId = await db.Recipes.Where(x => x.Id == recipeId.Value).Select(x => x.CreatorId).FirstAsync();
                if (RecipeCreatorId != AspNetId)
                    return Json("Not your recipe! Stop hacking please", JsonRequestBehavior.AllowGet);
            }
            ViewBag.UnitTypes = new SelectList(db.MeasurementTypes, "MeasurementTypeName", "MeasurementTypeName");
            return View();
        }

        #region Ajax Functions
        //Ajax Creates
        [Authorize]
        public async Task<JsonResult> AjaxCreateRecipe(string recipeName)
        {
            string AspNetId = User.Identity.GetUserId();
            Recipe Recipe = new Recipe()
            {
                CreateDateUtc = DateTime.UtcNow, //This indicates that its a recipe draft
                CreatorId = AspNetId,
                LastEditDateUtc = DateTime.UtcNow,
                Name = recipeName,
                ViewableType = "Secret",
                CreationStep = 1
            };
            db.Recipes.Add(Recipe);
            await db.SaveChangesAsync();
            return Json(Recipe, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public async Task<JsonResult> AjaxCreateRecipeStep(int recipeId, int stepNumber)
        {
            Recipe Recipe = await db.Recipes.FindAsync(recipeId);
            if (Recipe.CreatorId != User.Identity.GetUserId())
                return Json("Not your recipe! Stop hacking please", JsonRequestBehavior.AllowGet);
            Recipe.LastEditDateUtc = DateTime.UtcNow;
            db.Entry(Recipe).State = EntityState.Modified;
            RecipeStep RecipeStep = new RecipeStep()
            {
                RecipeId = recipeId,
                Comment = null,
                StepNumber = stepNumber,
                EstimatedTimeInSeconds = 0,
            };
            db.RecipeSteps.Add(RecipeStep);
            await db.SaveChangesAsync();
            RecipeStepDataTransferObject RecipeStepDataTransferObject = new RecipeStepDataTransferObject
            {
                RecipeStepsCloudFileDataTransferObjects = RecipeStep.RecipeStepsCloudFiles.Select(cf => new RecipeStepsCloudFileDataTransferObject
                {
                    CloudFileId = cf.CloudFileId,
                    RecipeId = cf.RecipeId,
                    SlotNumber = cf.SlotNumber,
                    StepNumber = cf.StepNumber,
                    Url = cf.CloudFile.CloudFilesThumbnail == null ? cf.CloudFile.Url : cf.CloudFile.CloudFilesThumbnail.Url
                }).ToList(),
                RecipeId = RecipeStep.RecipeId,
                Comment = RecipeStep.Comment ?? "",
                StepNumber = RecipeStep.StepNumber,
                EstimatedTimeInSeconds = RecipeStep.EstimatedTimeInSeconds
            };
            return Json(RecipeStepDataTransferObject, JsonRequestBehavior.AllowGet);
        }

        //Ajax Deletes
        [Authorize]
        public async Task<JsonResult> AjaxDeleteRecipe(int recipeId)
        {
            string AspNetId = User.Identity.GetUserId();
            Recipe recipe = await db.Recipes.FindAsync(recipeId);
            if (recipe.CreatorId != AspNetId)
                return Json("Not your recipe! Stop hacking please", JsonRequestBehavior.AllowGet);
            AzureCloudStorageWrapper.DeleteRecipeCloudFilesIfExist(recipe);
            db.CloudFiles.RemoveRange(recipe.RecipeCloudFiles.Select(x => x.CloudFile));
            db.CloudFiles.RemoveRange(recipe.RecipeSteps.SelectMany(x => x.RecipeStepsCloudFiles).Select(x => x.CloudFile));
            db.Recipes.Remove(recipe);
            await db.SaveChangesAsync();
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public async Task<JsonResult> AjaxDeleteRecipeStep(int recipeId, int stepNumber)
        {
            string AspNetId = User.Identity.GetUserId();
            Recipe Recipe = await db.Recipes.FindAsync(recipeId);
            if (Recipe.CreatorId != AspNetId)
                return Json("Not your recipe! Stop hacking please", JsonRequestBehavior.AllowGet);
            var CurrentRecipeSteps = db.RecipeSteps.Where(x => x.RecipeId == recipeId);
            Recipe.RecipeSteps.Clear();
            int NewStepNum = 1;
            foreach (RecipeStep CurrentRecipeStep in CurrentRecipeSteps.Where(x => x.StepNumber != stepNumber).ToList())
            {
                RecipeStep RecipeStep = new RecipeStep()
                {
                    Comment = CurrentRecipeStep.Comment,
                    EstimatedTimeInSeconds = CurrentRecipeStep.EstimatedTimeInSeconds,
                    RecipeId = recipeId,
                    StepNumber = NewStepNum,
                    RecipeStepsIngredients = CurrentRecipeStep.RecipeStepsIngredients.Select(x => new RecipeStepsIngredient()
                    {
                        RecipeId = x.RecipeId,
                        StepNumber = x.StepNumber,
                        Amount = x.Amount,
                        ToAmount = x.ToAmount,
                        IngredientName = x.IngredientName,
                        MeasurementUnitName = x.MeasurementUnitName
                    }).ToList(),
                    RecipeStepsCloudFiles = CurrentRecipeStep.RecipeStepsCloudFiles.Select(x => new RecipeStepsCloudFile()
                    {
                        CloudFileId = x.CloudFileId,
                        RecipeId = x.RecipeId,
                        StepNumber = x.StepNumber,
                        SlotNumber = x.SlotNumber,
                    }).ToList()
                };
                NewStepNum++;
                Recipe.RecipeSteps.Add(RecipeStep);
            }
            Recipe.LastEditDateUtc = DateTime.UtcNow;
            db.Entry(Recipe).State = EntityState.Modified;
            await db.SaveChangesAsync();
            int RecipeStepNumber = 1;
            if (Recipe.RecipeSteps != null)
                RecipeStepNumber = Recipe.RecipeSteps.Count();
            return Json(RecipeStepNumber, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public async Task<JsonResult> AjaxDeleteImageCloudFile(int recipeId, int stepNumber, int slotNumber)
        {
            string AspNetId = User.Identity.GetUserId();
            Recipe Recipe = await db.Recipes.FindAsync(recipeId);
            if (Recipe.CreatorId != AspNetId)
                return Json("Not your recipe! Stop hacking please", JsonRequestBehavior.AllowGet);
            if (stepNumber == -1) //-1 means main cloud file
            {
                CloudFile CloudFile = Recipe.RecipeCloudFiles.Where(x => x.RecipeCloudFileTypeName == "MainImage").First().CloudFile;
                AzureCloudStorageWrapper.DeleteCloudFile(CloudFile);
                db.CloudFiles.Remove(CloudFile);
            }
            else
            {
                RecipeStepsCloudFile RecipeStepsCloudFile = await db.RecipeStepsCloudFiles.FindAsync(recipeId, stepNumber, slotNumber);
                AzureCloudStorageWrapper.DeleteCloudFile(RecipeStepsCloudFile.CloudFile);
                db.CloudFiles.Remove(RecipeStepsCloudFile.CloudFile);
            }
            Recipe.LastEditDateUtc = DateTime.UtcNow;
            db.Entry(Recipe).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public async Task<JsonResult> AjaxDeleteVideoCloudFile(int recipeId)
        {
            string AspNetId = User.Identity.GetUserId();
            Recipe Recipe = await db.Recipes.FindAsync(recipeId);
            if (Recipe.CreatorId != AspNetId)
                return Json("Not your recipe! Stop hacking please", JsonRequestBehavior.AllowGet);
            CloudFile CloudFile = Recipe.RecipeCloudFiles.Where(x => x.RecipeCloudFileTypeName == "MainVideo").First().CloudFile;
            AzureCloudStorageWrapper.DeleteCloudFile(CloudFile);
            db.CloudFiles.Remove(CloudFile);
            Recipe.LastEditDateUtc = DateTime.UtcNow;
            db.Entry(Recipe).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        //Ajax Gets
        public async Task<JsonResult> AjaxGetRecipe(int recipeId)
        {
            string AspNetId = User.Identity.GetUserId();
            RecipeDataTransferObject RecipeDataTransferObject = await db.Recipes.Where(x => x.Id == recipeId).Select(Recipe => new RecipeDataTransferObject()
            {
                Id = Recipe.Id,
                CompleteDateUtc = Recipe.CompleteDateUtc,
                CreationStep = Recipe.CreationStep,
                CreatorName = Recipe.AspNetUser.Email,
                Description = Recipe.Description ?? "",
                DietaryRestrictionDataTransferObjects = Recipe.DietaryRestrictions.Select(x => new DietaryRestrictionDataTransferObject
                {
                    Name = x.Name,
                    IconUrl = x.IconUrl
                }).ToList(),
                LastEditDateUtc = Recipe.LastEditDateUtc,
                Name = Recipe.Name,
                IsMyRecipe = Recipe.AspNetUser.Id == AspNetId ? true : false,
                IsSaved = Recipe.SavedRecipes.Where(x => x.AspNetUserId == AspNetId).FirstOrDefault() == null ? false : true,
                RecipeCloudFileDataTransferObjects = Recipe.RecipeCloudFiles.Select(x => new RecipeCloudFileDataTransferObject
                {
                    CloudFileId = x.CloudFileId,
                    RecipeCloudFileTypeName = x.RecipeCloudFileTypeName,
                    RecipeId = x.RecipeId,
                    CloudFileDataTransferObject = x.CloudFile == null ? null : new CloudFileDataTransferObject()
                    {
                        CreatorId = x.CloudFile.CreatorId,
                        FileExtension = x.CloudFile.FileExtension,
                        Id = x.CloudFile.Id,
                        Url = x.CloudFile.Url,
                        CloudFileThumbnailsDataTransferObject = x.CloudFile.CloudFilesThumbnail == null ? null : new CloudFileThumbnailsDataTransferObject
                        {
                            CloudFileId = x.CloudFile.CloudFilesThumbnail.CloudFileId,
                            FileExtension = x.CloudFile.CloudFilesThumbnail.FileExtension,
                            Url = x.CloudFile.CloudFilesThumbnail.Url
                        }
                    }
                }).ToList(),
                RecipeStepDataTransferObjects = Recipe.RecipeSteps.Select(x => new RecipeStepDataTransferObject
                {
                    RecipeId = x.RecipeId,
                    RecipeStepsCloudFileDataTransferObjects = x.RecipeStepsCloudFiles.OrderBy(y => y.SlotNumber).Select(cf => new RecipeStepsCloudFileDataTransferObject
                    {
                        CloudFileId = cf.CloudFileId,
                        RecipeId = cf.RecipeId,
                        SlotNumber = cf.SlotNumber,
                        StepNumber = cf.StepNumber,
                        Url = cf.CloudFile.Url
                    }).ToList()
                }).ToList(),
                SavedRecipeDataTransferObjects = Recipe.SavedRecipes.Select(x => new SavedRecipeDataTransferObject
                {
                    RecipeId = x.RecipeId,
                    SavedDate = x.SavedDate
                }).ToList(),
                ServingSize = Recipe.ServingSize,
                ViewableType = Recipe.ViewableType,
            }).FirstOrDefaultAsync();
            return Json(RecipeDataTransferObject, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> AjaxGetRecipes(bool? myRecipes, bool? publicRecipes, int recipesPageIndex, bool? savedRecipes, string searchText, string sortingBy, bool? sortAscending)
        {
            string AspNetId = User.Identity.GetUserId();
            if (myRecipes != null && AspNetId == null)
                if (AspNetId == null)
                    return Json("Unauthorized", JsonRequestBehavior.AllowGet);
            IQueryable<Recipe> RecipesQuery = null;
            if (!sortAscending.HasValue || !sortAscending.Value)
                switch (sortingBy)
                {
                    case "1":
                        RecipesQuery = db.Recipes.OrderByDescending(r => r.CompleteDateUtc);
                        break;
                    case "2":
                        RecipesQuery = db.Recipes.OrderByDescending(r => r.Name);
                        break;
                    case "3":
                        RecipesQuery = db.Recipes.OrderByDescending(r => r.AspNetUser.UserName);
                        break;
                    default:
                        RecipesQuery = db.Recipes.OrderByDescending(r => r.LastEditDateUtc);
                        break;
                }
            else
                switch (sortingBy)
                {
                    case "1":
                        RecipesQuery = db.Recipes.OrderBy(r => r.CompleteDateUtc);
                        break;
                    case "2":
                        RecipesQuery = db.Recipes.OrderBy(r => r.Name);
                        break;
                    case "3":
                        RecipesQuery = db.Recipes.OrderBy(r => r.AspNetUser.UserName);
                        break;
                    default:
                        RecipesQuery = db.Recipes.OrderBy(r => r.LastEditDateUtc);
                        break;
                }
            var Predicate = PredicateBuilder.New<Recipe>(true);
            //searchText
            if (!String.IsNullOrEmpty(searchText))
                Predicate = Predicate.And(r => r.Name.Contains(searchText));
            //myRecipes
            if (myRecipes.HasValue)
                if (myRecipes.Value)                
                    Predicate = Predicate.And(r => r.CreatorId == AspNetId);                
                else                
                    Predicate = Predicate.And(r => r.CreatorId != AspNetId);
            //publicRecipes
            if (publicRecipes.HasValue)
                if (publicRecipes.Value)                
                    Predicate = Predicate.And(r => r.ViewableType == "Public");                
                else                
                    Predicate = Predicate.And(r => r.ViewableType != "Public");
            //savedRecipes
            if (savedRecipes.HasValue)
                if (savedRecipes.Value)
                    Predicate = Predicate.And(r => r.ViewableType == "Public");
                else
                    Predicate = Predicate.And(r => r.ViewableType != "Public");
            RecipesQuery = RecipesQuery.AsExpandable().Where(Predicate);

            //switch (myRecipes)
            //{
            //    case null:
            //        RecipesQuery = RecipesQuery.Where(r => r.CreatorId == AspNetId || r.ViewableType == "Public"); //Grabs all public and all of the current users recipes
            //        break;
            //    case "1":
            //        RecipesQuery = RecipesQuery.Where(r => r.CreatorId == AspNetId);
            //        break;
            //    case "2":
            //        RecipesQuery = RecipesQuery.Where(r => r.CreatorId != AspNetId && r.ViewableType == "Public");
            //        break;
            //}
            List<RecipeDataTransferObject> Recipes = await RecipesQuery.Skip(recipesPageIndex * PageSize).Take(PageSize).Select(Recipe => new RecipeDataTransferObject()
            {
                Id = Recipe.Id,
                CompleteDateUtc = Recipe.CompleteDateUtc,
                CreationStep = Recipe.CreationStep,
                Description = Recipe.Description ?? "",
                CreatorName = Recipe.AspNetUser.UserName,
                IsMyRecipe = Recipe.AspNetUser.Id == AspNetId ? true : false,
                IsSaved = Recipe.SavedRecipes.Where(x => x.AspNetUserId == AspNetId).FirstOrDefault() == null ? false : true,
                LastEditDateUtc = Recipe.LastEditDateUtc,
                Name = Recipe.Name,
                ServingSize = Recipe.ServingSize,
                ViewableType = Recipe.ViewableType,
                DietaryRestrictionDataTransferObjects = Recipe.DietaryRestrictions.Select(x => new DietaryRestrictionDataTransferObject
                {
                    Name = x.Name,
                    IconUrl = x.IconUrl
                }).ToList(),
                RecipeCloudFileDataTransferObjects = Recipe.RecipeCloudFiles.Select(x => new RecipeCloudFileDataTransferObject
                {
                    CloudFileId = x.CloudFileId,
                    RecipeCloudFileTypeName = x.RecipeCloudFileTypeName,
                    RecipeId = x.RecipeId,
                    CloudFileDataTransferObject = x.CloudFile == null ? null : new CloudFileDataTransferObject()
                    {
                        CreatorId = x.CloudFile.CreatorId,
                        FileExtension = x.CloudFile.FileExtension,
                        Id = x.CloudFile.Id,
                        Url = x.CloudFile.Url,
                        CloudFileThumbnailsDataTransferObject = x.CloudFile.CloudFilesThumbnail == null ? null : new CloudFileThumbnailsDataTransferObject
                        {
                            CloudFileId = x.CloudFile.CloudFilesThumbnail.CloudFileId,
                            FileExtension = x.CloudFile.CloudFilesThumbnail.FileExtension,
                            Url = x.CloudFile.CloudFilesThumbnail.Url
                        }
                    }
                }).ToList(),
                RecipeStepDataTransferObjects = Recipe.RecipeSteps.Select(x => new RecipeStepDataTransferObject
                {
                    Comment = x.Comment,
                    StepNumber = x.StepNumber,
                    EstimatedTimeInSeconds = x.EstimatedTimeInSeconds
                }).ToList(),
                SavedRecipeDataTransferObjects = Recipe.SavedRecipes.Select(x => new SavedRecipeDataTransferObject
                {
                    RecipeId = x.RecipeId,
                    SavedDate = x.SavedDate
                }).ToList(),
            }).ToListAsync();
            return Json(Recipes, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> AjaxGetRecipeSteps(int recipeId)
        {
            Recipe Recipe = await db.Recipes.FindAsync(recipeId);
            List<RecipeStepDataTransferObject> RecipeStepDataTransferObjects = Recipe.RecipeSteps.OrderBy(x => x.StepNumber).Select(x => new RecipeStepDataTransferObject()
            {
                RecipeId = x.RecipeId,
                Comment = x.Comment ?? "",
                EstimatedTimeInSeconds = x.EstimatedTimeInSeconds,
                StepNumber = x.StepNumber,
                RecipeStepsCloudFileDataTransferObjects = x.RecipeStepsCloudFiles.Select(cf => new RecipeStepsCloudFileDataTransferObject
                {
                    CloudFileId = cf.CloudFileId,
                    RecipeId = cf.RecipeId,
                    SlotNumber = cf.SlotNumber,
                    StepNumber = cf.StepNumber,
                    Url = cf.CloudFile.Url,
                }).ToList(),
                RecipeStepsIngredientsDataTransferObjects = x.RecipeStepsIngredients.Select(s => new RecipeStepsIngredientsDataTransferObject
                {
                    IngredientName = s.IngredientName,
                    MeasurementUnitName = s.MeasurementUnitName,
                    Amount = s.Amount,
                    ToAmount = s.ToAmount,
                }).ToList()
            }).OrderBy(x => x.StepNumber).ToList();

            return Json(RecipeStepDataTransferObjects, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> AjaxGetRecipeStep(int recipeId, int stepNumber)
        {
            RecipeStep RecipeStep = await db.RecipeSteps.FindAsync(recipeId, stepNumber);
            RecipeStepDataTransferObject RecipeStepDataTransferObject = new RecipeStepDataTransferObject
            {
                RecipeId = RecipeStep.RecipeId,
                Comment = RecipeStep.Comment ?? "",
                RecipeStepsCloudFileDataTransferObjects = RecipeStep.RecipeStepsCloudFiles.Select(cf => new RecipeStepsCloudFileDataTransferObject
                {
                    CloudFileId = cf.CloudFileId,
                    RecipeId = cf.RecipeId,
                    SlotNumber = cf.SlotNumber,
                    StepNumber = cf.StepNumber,
                    Url = cf.CloudFile.Url
                }).ToList(),
                EstimatedTimeInSeconds = RecipeStep.EstimatedTimeInSeconds,
                StepNumber = RecipeStep.StepNumber,
                RecipeStepsIngredientsDataTransferObjects = RecipeStep.RecipeStepsIngredients.Select(s => new RecipeStepsIngredientsDataTransferObject
                {
                    IngredientName = s.IngredientName,
                    MeasurementUnitName = s.MeasurementUnitName,
                    Amount = s.Amount,
                    ToAmount = s.ToAmount
                }).ToList()
            };
            return Json(RecipeStepDataTransferObject, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> AjaxGetRecipeStepsIngredients(int recipeId)
        {
            var RecipeStepsIngredientsDataTransferObjects = await db.RecipeStepsIngredients.Where(x => x.RecipeId == recipeId)
                .Select(s => new RecipeStepsIngredientsDataTransferObject
                {
                    IngredientName = s.IngredientName,
                    MeasurementUnitName = s.MeasurementUnitName,
                    Amount = s.Amount,
                    ToAmount = s.ToAmount
                }).ToListAsync();
            return Json(RecipeStepsIngredientsDataTransferObjects, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> AjaxGetUnits()
        {
            var MeasurementUnits = await db.MeasurementUnits.OrderBy(x => x.MeasurementUnitName).ThenBy(x => x.MeasurementTypeName).Select(x => new
            {
                Text = x.MeasurementUnitName,
                Value = x.MeasurementTypeName
            }).ToListAsync();
            return Json(MeasurementUnits, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public async Task<JsonResult> AjaxMoveRecipeStep(int recipeId, int stepNumber, int moveBy)
        {
            string AspNetId = User.Identity.GetUserId();
            Recipe Recipe = await db.Recipes.FindAsync(recipeId);
            if (Recipe.CreatorId != AspNetId)
                return Json("Not your recipe! Stop hacking please", JsonRequestBehavior.AllowGet);
            if (moveBy == 0)
                return Json(true, JsonRequestBehavior.AllowGet);
            List<RecipeStep> CurrentSteps = db.RecipeSteps.Where(x => x.RecipeId == recipeId).OrderBy(x => x.StepNumber).ToList();
            ObservableCollection<RecipeStep> RecipeSteps = new ObservableCollection<RecipeStep>(CurrentSteps);
            RecipeStep StepToMove = RecipeSteps.Where(x => x.StepNumber == stepNumber).First();
            int CurrentIndex = RecipeSteps.IndexOf(StepToMove);
            int NewIndex = CurrentIndex + moveBy;
            if (moveBy > 0)
            {
                if (NewIndex <= RecipeSteps.Count() - 1)//It would be nice to check this client side so theres not a pointless request 
                    RecipeSteps.Move(CurrentIndex, NewIndex);
                else
                    return Json(false, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (NewIndex > -1)//It would be nice to check this client side so theres not a pointless request                
                    RecipeSteps.Move(CurrentIndex, NewIndex);
                else
                    return Json(false, JsonRequestBehavior.AllowGet);
            }
            int NewStepNumber = 1;
            foreach (RecipeStep OldStep in RecipeSteps)
            {
                RecipeStep NewRecipeStep = new RecipeStep()
                {
                    Comment = OldStep.Comment,
                    EstimatedTimeInSeconds = OldStep.EstimatedTimeInSeconds,
                    RecipeId = OldStep.RecipeId,
                    Recipe = OldStep.Recipe,
                    StepNumber = NewStepNumber,
                };
                foreach (RecipeStepsCloudFile RecipeStepsCloudFile in OldStep.RecipeStepsCloudFiles.ToList())
                {
                    NewRecipeStep.RecipeStepsCloudFiles.Add(new RecipeStepsCloudFile()
                    {
                        CloudFileId = RecipeStepsCloudFile.CloudFileId,
                        RecipeId = RecipeStepsCloudFile.RecipeId,
                        SlotNumber = RecipeStepsCloudFile.SlotNumber,
                        StepNumber = NewStepNumber,
                    });
                }
                foreach (RecipeStepsIngredient RecipeStepsIngredient in OldStep.RecipeStepsIngredients.ToList())
                {
                    NewRecipeStep.RecipeStepsIngredients.Add(new RecipeStepsIngredient()
                    {
                        RecipeId = RecipeStepsIngredient.RecipeId,
                        StepNumber = NewStepNumber,
                        Amount = RecipeStepsIngredient.Amount,
                        IngredientName = RecipeStepsIngredient.IngredientName,
                        MeasurementUnitName = RecipeStepsIngredient.MeasurementUnitName,
                        ToAmount = RecipeStepsIngredient.ToAmount
                    });
                }
                db.RecipeSteps.Add(NewRecipeStep);
                NewStepNumber++;
            }
            db.RecipeSteps.RemoveRange(CurrentSteps);
            Recipe.LastEditDateUtc = DateTime.UtcNow;
            db.Entry(Recipe).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public async Task<JsonResult> AjaxIncrementCreationStep(int recipeId)
        {
            string AspNetId = User.Identity.GetUserId();
            Recipe Recipe = await db.Recipes.FindAsync(recipeId);
            if (Recipe.CreatorId != AspNetId)
                return Json("Not your recipe! Stop hacking please", JsonRequestBehavior.AllowGet);
            if (Recipe.CreationStep < 4)
                Recipe.CreationStep++;
            db.Entry(Recipe).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return Json(Recipe.CreationStep, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> AjaxToggleSaveRecipe(int recipeId)
        {
            string AspNetId = User.Identity.GetUserId();
            if (String.IsNullOrEmpty(AspNetId))
                return Json("No AspNetId", JsonRequestBehavior.AllowGet);
            string RecipeCreatorId = await db.Recipes.Where(x => x.Id == recipeId).Select(x => x.CreatorId).FirstOrDefaultAsync();
            if (RecipeCreatorId == AspNetId)
                throw new Exception("You cant save your own recipe");
            bool IsSaved = true;
            SavedRecipe SavedRecipe = await db.SavedRecipes.FindAsync(AspNetId, recipeId);
            if (SavedRecipe == null)
                db.SavedRecipes.Add(new SavedRecipe { AspNetUserId = AspNetId, RecipeId = recipeId, SavedDate = DateTime.UtcNow });
            else
            {
                IsSaved = false;
                db.SavedRecipes.Remove(SavedRecipe);
            }
            await db.SaveChangesAsync();
            return Json(IsSaved, JsonRequestBehavior.AllowGet);
        }

        //Ajax Updates
        [Authorize]
        public async Task<JsonResult> AjaxUpdateDescription(int recipeId, string description)
        {
            string AspNetId = User.Identity.GetUserId();
            Recipe Recipe = await db.Recipes.FindAsync(recipeId);
            if (Recipe.CreatorId != AspNetId)
                return Json("Not your recipe! Stop hacking please", JsonRequestBehavior.AllowGet);
            Recipe.Description = description;
            Recipe.LastEditDateUtc = DateTime.UtcNow;
            db.Entry(Recipe).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public async Task<JsonResult> AjaxUpdateDietaryRestrictions(int recipeId, Collection<string> dietaryRestrictionNames)
        {
            string AspNetId = User.Identity.GetUserId();
            Recipe Recipe = await db.Recipes.FindAsync(recipeId);
            if (Recipe.CreatorId != AspNetId)
                return Json("Not your recipe! Stop hacking please", JsonRequestBehavior.AllowGet);
            Recipe.DietaryRestrictions.Clear();
            //Add New
            if (dietaryRestrictionNames != null)
                foreach (DietaryRestriction DietaryRestriction in db.DietaryRestrictions.Where(x => dietaryRestrictionNames.Contains(x.Name)))
                    Recipe.DietaryRestrictions.Add(DietaryRestriction);
            Recipe.LastEditDateUtc = DateTime.UtcNow;
            db.Entry(Recipe).State = EntityState.Modified;
            await db.SaveChangesAsync();
            var DietaryRestrictionDataTransferObjects = Recipe.DietaryRestrictions.Select(x => new DietaryRestrictionDataTransferObject
            {
                Name = x.Name,
                IconUrl = x.IconUrl
            });
            return Json(DietaryRestrictionDataTransferObjects, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public async Task<JsonResult> AjaxUpdateName(int recipeId, string recipeName)
        {
            string AspNetId = User.Identity.GetUserId();
            Recipe Recipe = await db.Recipes.FindAsync(recipeId);
            if (Recipe.CreatorId != AspNetId)
                return Json("Not your recipe! Stop hacking please", JsonRequestBehavior.AllowGet);
            Recipe.Name = recipeName;
            Recipe.LastEditDateUtc = DateTime.UtcNow;
            db.Entry(Recipe).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return Json(Recipe.Name, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpPost]
        public async Task<JsonResult> AjaxUpdateRecipeStep(RecipeStep recipeStep)
        {
            string AspNetId = User.Identity.GetUserId();
            Recipe Recipe = await db.Recipes.FindAsync(recipeStep.RecipeId);
            if (Recipe.CreatorId != AspNetId)
                return Json("Not your recipe! Stop hacking please", JsonRequestBehavior.AllowGet);
            Recipe.LastEditDateUtc = DateTime.UtcNow;
            db.Entry(Recipe).State = EntityState.Modified;
            RecipeStep OldRecipeStep = Recipe.RecipeSteps.Where(x => x.RecipeId == recipeStep.RecipeId && x.StepNumber == recipeStep.StepNumber).First();
            OldRecipeStep.Comment = String.IsNullOrEmpty(recipeStep.Comment) ? null : recipeStep.Comment;
            OldRecipeStep.EstimatedTimeInSeconds = recipeStep.EstimatedTimeInSeconds;
            OldRecipeStep.RecipeStepsIngredients.Clear();
            foreach (RecipeStepsIngredient RecipeStepsIngredient in recipeStep.RecipeStepsIngredients)
            {
                //Create Ingredient if it doesnt exist
                Ingredient Ingredient = await db.Ingredients.FindAsync(RecipeStepsIngredient.IngredientName);
                if (Ingredient == null)
                {
                    Ingredient = new Ingredient
                    {
                        IngredientName = RecipeStepsIngredient.IngredientName,
                        CreatorId = AspNetId,
                        CreateDateUtc = DateTime.UtcNow
                    };
                    db.Ingredients.Add(Ingredient);
                }
                //Remove Leading 0s
                if (RecipeStepsIngredient.Amount != null)
                {
                    RecipeStepsIngredient.Amount = RecipeStepsIngredient.Amount.TrimStart('0');
                    RecipeStepsIngredient.Amount = RecipeStepsIngredient.Amount.Length > 0 ? RecipeStepsIngredient.Amount : "0";
                }
                if (RecipeStepsIngredient.ToAmount != null)
                {
                    RecipeStepsIngredient.ToAmount = RecipeStepsIngredient.ToAmount.TrimStart('0');
                    RecipeStepsIngredient.ToAmount = RecipeStepsIngredient.ToAmount.Length > 0 ? RecipeStepsIngredient.ToAmount : "0";
                }
                OldRecipeStep.RecipeStepsIngredients.Add(RecipeStepsIngredient);
            }
            await db.SaveChangesAsync();
            return Json("true", JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public async Task<JsonResult> AjaxUpdateViewableType(int recipeId, string viewableTypeName)
        {
            string AspNetId = User.Identity.GetUserId();
            Recipe Recipe = await db.Recipes.FindAsync(recipeId);
            if (Recipe.CreatorId != AspNetId)
                return Json("Not your recipe! Stop hacking please", JsonRequestBehavior.AllowGet);
            if (Recipe.CompleteDateUtc == null)
            {
                Recipe.CreationStep = 5;
                Recipe.CompleteDateUtc = DateTime.UtcNow;
            }
            Recipe.ViewableType = viewableTypeName;
            db.Entry(Recipe).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return Json(Recipe.ViewableType, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public async Task<JsonResult> AjaxUpdateServingSize(int recipeId, int servingSize)
        {
            string AspNetId = User.Identity.GetUserId();
            Recipe Recipe = await db.Recipes.FindAsync(recipeId);
            if (Recipe.CreatorId != AspNetId)
                return Json("Not your recipe! Stop hacking please", JsonRequestBehavior.AllowGet);
            Recipe.ServingSize = servingSize;
            if (Recipe.CreationStep < 2)
                Recipe.CreationStep = 2;
            Recipe.LastEditDateUtc = DateTime.UtcNow;
            db.Entry(Recipe).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return Json(Recipe.ServingSize, JsonRequestBehavior.AllowGet);
        }

        //Ajax Uploads
        [Authorize]
        [HttpPost]
        public async Task<JsonResult> AjaxUploadRecipeImage(int recipeId)
        {
            string AspNetId = User.Identity.GetUserId();
            Recipe Recipe = await db.Recipes.FindAsync(recipeId);
            if (Recipe.CreatorId != AspNetId)
                return Json("Not your recipe! Stop hacking please", JsonRequestBehavior.AllowGet);
            HttpPostedFile HttpPostedFile = System.Web.HttpContext.Current.Request.Files["UploadedFile"];
            int UploadedCloudFileId = await AzureCloudStorageWrapper.UploadCloudFile(AzureCloudStorageWrapper.AzureBlobContainer.recipecloudfiles, new HttpPostedFileWrapper(HttpPostedFile), AspNetId, "Recipe" + Recipe.Id + "MainImage", true);
            if (UploadedCloudFileId == -1)//File size too big            
                return Json(-1, JsonRequestBehavior.AllowGet);
            Recipe.RecipeCloudFiles.Add(new RecipeCloudFile { RecipeId = recipeId, CloudFileId = UploadedCloudFileId, RecipeCloudFileTypeName = "MainImage" });
            Recipe.LastEditDateUtc = DateTime.UtcNow;
            db.Entry(Recipe).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpPost]
        public async Task<JsonResult> AjaxUploadRecipeStepImage(int recipeId, int stepNumber, int slotNumber)
        {
            string AspNetId = User.Identity.GetUserId();
            Recipe Recipe = await db.Recipes.FindAsync(recipeId);
            if (Recipe.CreatorId != AspNetId)
                return Json("Not your recipe! Stop hacking please", JsonRequestBehavior.AllowGet);
            HttpPostedFile HttpPostedFile = System.Web.HttpContext.Current.Request.Files["UploadedFile"];
            int UploadedCloudFileId = await AzureCloudStorageWrapper.UploadCloudFile(AzureCloudStorageWrapper.AzureBlobContainer.recipecloudfiles, new HttpPostedFileWrapper(HttpPostedFile), AspNetId, "Recipe" + Recipe.Id + "Step" + stepNumber + "Slot" + slotNumber, true);
            if (UploadedCloudFileId == -1)//File size too big            
                return Json(-1, JsonRequestBehavior.AllowGet);
            //Get the correct recipe step to add the media files too
            RecipeStep RecipeStep = Recipe.RecipeSteps.Where(x => x.StepNumber == stepNumber).First();
            RecipeStepsCloudFile RecipeStepsCloudFiles = new RecipeStepsCloudFile() { RecipeId = recipeId, StepNumber = stepNumber, SlotNumber = slotNumber, CloudFileId = UploadedCloudFileId };
            RecipeStep.RecipeStepsCloudFiles.Add(RecipeStepsCloudFiles);
            Recipe.LastEditDateUtc = DateTime.UtcNow;
            db.Entry(Recipe).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        //Ajax Uploads
        [Authorize]
        [HttpPost]
        public async Task<JsonResult> AjaxUploadRecipeVideo(int recipeId)
        {
            string AspNetId = User.Identity.GetUserId();
            Recipe Recipe = await db.Recipes.FindAsync(recipeId);
            if (Recipe.CreatorId != AspNetId)
                return Json("Not your recipe! Stop hacking please", JsonRequestBehavior.AllowGet);
            HttpPostedFile HttpPostedFile = System.Web.HttpContext.Current.Request.Files["UploadedFile"];
            int UploadedCloudFileId = await AzureCloudStorageWrapper.UploadCloudFile(AzureCloudStorageWrapper.AzureBlobContainer.recipecloudfiles, new HttpPostedFileWrapper(HttpPostedFile), AspNetId, "Recipe" + Recipe.Id + "Video", false);
            if (UploadedCloudFileId == -1)//File size too big            
                return Json(-1, JsonRequestBehavior.AllowGet);
            Recipe.RecipeCloudFiles.Add(new RecipeCloudFile { RecipeId = recipeId, CloudFileId = UploadedCloudFileId, RecipeCloudFileTypeName = "MainVideo" });
            Recipe.LastEditDateUtc = DateTime.UtcNow;
            db.Entry(Recipe).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AjaxValidateAmountsThisShouldBeDoneClientSide(string amount, string toAmount)
        {
            //Remove Leading 0s
            if (amount != null)
            {
                amount = amount.TrimStart('0');
                amount = amount.Length > 0 ? amount : "0";
            }
            if (toAmount != null)
                toAmount = toAmount.TrimStart('0');
            //To validate Amount we make sure it can convert to a double before saving the fraction string
            double Amount = 0;
            double ToAmount = 0;
            try { Amount = Convert.ToDouble(amount, CultureInfo.InvariantCulture); }
            catch (FormatException)
            {
                try { ToAmount = UtilityFunctionLib.FractionToDouble(amount); }
                catch (FormatException) { return Json("Amount is not in a valid format", JsonRequestBehavior.AllowGet); }
            }
            //To validate OptionalAmount we make sure it can convert to a double before saving the string
            if (!String.IsNullOrEmpty(toAmount))
            {
                try { ToAmount = Convert.ToDouble(toAmount, CultureInfo.InvariantCulture); }
                catch (FormatException)
                {
                    try { ToAmount = UtilityFunctionLib.FractionToDouble(toAmount); }
                    catch (FormatException) { return Json("The \"to\" amount is not in a valid format", JsonRequestBehavior.AllowGet); }
                }
                if (Amount > ToAmount)
                    return Json("The \"to\" amount has to be greater", JsonRequestBehavior.AllowGet);
            }
            RecipeStepsIngredientsDataTransferObject RecipeStepsIngredientsDataTransferObject = new RecipeStepsIngredientsDataTransferObject
            {
                Amount = amount,
                ToAmount = toAmount
            };
            return Json(RecipeStepsIngredientsDataTransferObject, JsonRequestBehavior.AllowGet);
        }
        #endregion        
    }
}