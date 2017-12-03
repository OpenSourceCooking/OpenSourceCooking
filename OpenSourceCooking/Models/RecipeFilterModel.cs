namespace OpenSourceCooking.Models
{
    public class RecipeFilterModel
    {
        public bool? Drafts { get; set; }
        public bool? Follower { get; set; }
        public bool? FollowingChefs { get; set; }
        public bool? MyRecipes { get; set; }
        public bool? Public { get; set; }
        public int? RecipeId { get; set; }
        public bool? RecipeOwner { get; set; }
        public bool? Saved { get; set; }
        public bool? Secret { get; set; }
        public string SearchText { get; set; }
        public string SortingBy { get; set; }
        public bool? SortAscending { get; set; }

        public int? RecipesPageIndex { get; set; }
        public bool? ReturnJson { get; set; }
    }
}