using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSourceCooking.Models
{
    public class CommentDataTransferObject
    {
        long id;
        string text;
        string creatorId;
        DateTime createDateUtc;
        DateTime? editDate;
        long? parentCommentId;
        ICollection<CommentVoteDataTransferObject> commentVotes;
        ICollection<CommentDataTransferObject> childComments;


        public long Id { get { return id; } set { id = value; } }
        public string Text { get { return text; } set { text = value; } }
        public string CreatorId { get { return creatorId; } set { creatorId = value; } }
        public DateTime CreateDateUtc { get { return createDateUtc; } set { createDateUtc = value; } }
        public DateTime? EditDateUtc { get { return editDate; } set { editDate = value; } }
        public long? ParentCommentId { get { return parentCommentId; } set { parentCommentId = value; } }
        public ICollection<CommentVoteDataTransferObject> CommentVotes { get { return commentVotes; } set { commentVotes = value; } }
        public ICollection<CommentDataTransferObject> ChildComments { get { return childComments; } set { childComments = value; } }

        //Extra Properties
        bool isMyRecipe;
        string potedByChefName;
        int totalVoteValue;
        CommentVoteDataTransferObject myVote;

        public bool IsMyRecipe { get { return isMyRecipe; } set { isMyRecipe = value; } }
        public string PostedByChefName { get { return potedByChefName; } set { potedByChefName = value; } }
        public int TotalVoteValue { get { return totalVoteValue; } set { totalVoteValue = value; } }
        public CommentVoteDataTransferObject MyVote { get { return myVote; } set { myVote = value; } }
    }
    public class CommentVoteDataTransferObject
    {
        long commentId;
        short voteValue;
        string voterId;

        public long CommentId { get { return commentId; } set { commentId = value; } }
        public short VoteValue { get { return voteValue; } set { voteValue = value; } }
        public string VoterId { get { return voterId; } set { voterId = value; } }
    }
    public class CloudFileDataTransferObject
    {
        int id;
        string url;
        string fileExtension;
        string creatorId;
        CloudFileThumbnailsDataTransferObject cloudFileThumbnailsDataTransferObject;

        public int Id { get { return id; } set { id = value; } }
        public string Url { get { return url; } set { url = value; } }
        public string FileExtension { get { return fileExtension; } set { fileExtension = value; } }
        public string CreatorId { get { return creatorId; } set { creatorId = value; } }
        public CloudFileThumbnailsDataTransferObject CloudFileThumbnailsDataTransferObject { get { return cloudFileThumbnailsDataTransferObject; } set { cloudFileThumbnailsDataTransferObject = value; } }
    }
    public class CloudFileThumbnailsDataTransferObject
    {
        int cloudFileId;
        string url;
        string fileExtension;

        public int CloudFileId { get { return cloudFileId; } set { cloudFileId = value; } }
        public string Url { get { return url; } set { url = value; } }
        public string FileExtension { get { return fileExtension; } set { fileExtension = value; } }
    }
    public class DietaryRestrictionDataTransferObject
    {
        string iconUrl;
        string name;

        public string IconUrl { get { return iconUrl; } set { iconUrl = value; } }
        public string Name { get { return name; } set { name = value; } }
    }
    public class IngredientDataTransferObject
    {
        string ingredientName;
        string imageUrl;
        string creatorId;
        DateTime createDateUtc;
        
        public string IngredientName { get { return ingredientName; } set { ingredientName = value; } }
        public string ImageUrl { get { return imageUrl; } set { imageUrl = value; } }
        public string CreatorId { get { return creatorId; } set { creatorId = value; } }
        public DateTime CreateDateUtc { get { return createDateUtc; } set { createDateUtc = value; } }
    }
    public class RecipeCloudFileDataTransferObject
    {
        int recipeId;
        int cloudFileId;
        string recipeCloudFileTypeName;
        CloudFileDataTransferObject cloudFileDataTransferObject;

        public int RecipeId { get { return recipeId; } set { recipeId = value; } }
        public int CloudFileId { get { return cloudFileId; } set { cloudFileId = value; } }
        public string RecipeCloudFileTypeName { get { return recipeCloudFileTypeName; } set { recipeCloudFileTypeName = value; } }
        public CloudFileDataTransferObject CloudFileDataTransferObject { get { return cloudFileDataTransferObject; } set { cloudFileDataTransferObject = value; } }
    }
    public class RecipeDataTransferObject
    {
        int id;
        DateTime? completeDateUtc;
        int creationStep;
        string creatorName;
        string description;
        DateTime lastEditDateUtc;
        string name;
        int servingSize;             
        string viewableType;
        ICollection<RecipeStepDataTransferObject> recipeStepDataTransferObjects;
        ICollection<DietaryRestrictionDataTransferObject> dietaryRestrictionDataTransferObjects;
        ICollection<RecipeCloudFileDataTransferObject> recipeCloudFileDataTransferObjects;

        public int Id { get { return id; } set { id = value; } }
        public DateTime? CompleteDateUtc { get { return completeDateUtc; } set { completeDateUtc = value; } }
        public int CreationStep { get { return creationStep; } set { creationStep = value; } }
        public string CreatorName { get { return creatorName; } set { creatorName = value; } }
        public string Description { get { return description; } set { description = value; } }
        public DateTime LastEditDateUtc { get { return lastEditDateUtc; } set { lastEditDateUtc = value; } }
        public string Name { get { return name; } set { name = value; } }
        public int ServingSize { get { return servingSize; } set { servingSize = value; } }
        public string ViewableType { get { return viewableType; } set { viewableType = value; } }
        public ICollection<RecipeStepDataTransferObject> RecipeStepDataTransferObjects { get { return recipeStepDataTransferObjects; } set { recipeStepDataTransferObjects = value; } }
        public ICollection<DietaryRestrictionDataTransferObject> DietaryRestrictionDataTransferObjects { get { return dietaryRestrictionDataTransferObjects; } set { dietaryRestrictionDataTransferObjects = value; } }
        public ICollection<RecipeCloudFileDataTransferObject> RecipeCloudFileDataTransferObjects { get { return recipeCloudFileDataTransferObjects; } set { recipeCloudFileDataTransferObjects = value; } }

        //Extra Properties
        bool isMyRecipe;
        bool isSaved;

        public long EstimatedTimeInSeconds { get { return RecipeStepDataTransferObjects == null ? 0 : RecipeStepDataTransferObjects.Sum(x => x.EstimatedTimeInSeconds); } }
        public bool IsMyRecipe { get { return isMyRecipe; } set { isMyRecipe = value; } }
        public bool IsSaved { get { return isSaved; } set { isSaved = value; } }

    }
    public class RecipeStepDataTransferObject
    {
        int recipeId;
        int stepNumber;
        long estimatedTimeInSeconds;
        string comment;
        ICollection<RecipeStepsCloudFileDataTransferObject> recipeStepsCloudFileDataTransferObjects;
        ICollection<RecipeStepsIngredientsDataTransferObject> recipeStepsIngredientsDataTransferObjects;

        public int RecipeId { get { return recipeId; } set { recipeId = value; } }
        public int StepNumber { get { return stepNumber; } set { stepNumber = value; } }
        public long EstimatedTimeInSeconds { get { return estimatedTimeInSeconds; } set { estimatedTimeInSeconds = value; } }
        public string Comment { get { return comment; } set { comment = value; } }
        public ICollection<RecipeStepsCloudFileDataTransferObject> RecipeStepsCloudFileDataTransferObjects { get { return recipeStepsCloudFileDataTransferObjects; } set { recipeStepsCloudFileDataTransferObjects = value; } }
        public ICollection<RecipeStepsIngredientsDataTransferObject> RecipeStepsIngredientsDataTransferObjects { get { return recipeStepsIngredientsDataTransferObjects; } set { recipeStepsIngredientsDataTransferObjects = value; } }
    }
    public class RecipeStepsCloudFileDataTransferObject
    {
        int recipeId;
        int stepNumber;
        int slotNumber;
        int cloudFileId;
        string url;

        public int RecipeId { get { return recipeId; } set { recipeId = value; } }
        public int StepNumber { get { return stepNumber; } set { stepNumber = value; } }
        public int SlotNumber { get { return slotNumber; } set { slotNumber = value; } }
        public int CloudFileId { get { return cloudFileId; } set { cloudFileId = value; } }
        public string Url { get { return url; } set { url = value; } }
    }
    public class RecipeStepsIngredientsDataTransferObject
    {
        int recipeId;
        int stepNumber;
        string amount;
        string ingredientName;
        string measurementUnitName;
        string measurementTypeName;
        string toAmount;

        public int RecipeId { get { return recipeId; } set { recipeId = value; } }
        public int StepNumber { get { return stepNumber; } set { stepNumber = value; } }
        public string Amount { get { return amount; } set { amount = value; } }
        public string IngredientName { get { return ingredientName; } set { ingredientName = value; } }
        public string MeasurementUnitName { get { return measurementUnitName; } set { measurementUnitName = value; } }
        public string MeasurementTypeName { get { return measurementTypeName; } set { measurementTypeName = value; } }
        public string ToAmount { get { return toAmount; } set { toAmount = value; } }
    }
}