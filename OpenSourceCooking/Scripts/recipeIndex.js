var $RecipesDiv = null;
var ClickedRecipeId = 0;
var ClickedViewableTypeButton = null;
var IsGettingRecipes = false;
var IsVoteOnCommentBusy = false;
var MaxRecipeCommentLength = 300;
var RecipeCommentsPageIndex = 0;
var RecipeCommentsSkipAdjust = 0;
var RecipesArray = [];
var RecipeId = null;
var SavedBorderColor = "#007bff";//Blue
var ShowingRecipeSteps = false;
var ShowingRecipeComments = false;

$(document).ready(function () {
    $RecipesDiv = $('#RecipesDiv');
    // layout Isotope after each image loads
    $RecipesDiv.imagesLoaded().progress(function () {
        $RecipesDiv.isotope('layout');
    });
    // init
    $RecipesDiv.isotope({
        // options
        itemSelector: '.box',
        masonry: {
            transformsEnabled: false,
            columnWidth: 268,
            isFitWidth: true
        }
    });
    var RecipeCommentTextArea = $('#RecipeCommentTextArea');
    RecipeCommentTextArea.attr('maxlength', MaxRecipeCommentLength);
    $('#RecipeCommentCharactersLeftSpan').text(MaxRecipeCommentLength);
    RecipeCommentTextArea.keyup(function (event) {
        $('#RecipeCommentCharactersLeftSpan').text(MaxRecipeCommentLength - RecipeCommentTextArea.val().length);
    });
    if (ViewBagRecipeOwner)
        GetFilterByKey('RecipeOwner').Value = ViewBagRecipeOwner;    
    if (ViewBagPublicRecipes)
        GetFilterByKey('PublicRecipes').Value = ViewBagPublicRecipes; 
    if (ViewBagRecipeId)
        RecipeId = ViewBagRecipeId;
    if (ViewBagRecipesPageIndex)
        RecipesPageIndex = ViewBagRecipesPageIndex;
    if (ViewBagSavedRecipes)
        GetFilterByKey('SavedRecipes').Value = ViewBagSavedRecipes;
    if (ViewBagSearchText) {
        GetFilterByKey('SearchText').Value = ViewBagSearchText;
        $('#SearchTextInput').val(ViewBagSearchText);
        $('#NavbarSearchButton').removeClass('btn-primary').addClass('btn-danger').html('X');
    }  
    if (ViewBagSortingBy)
        GetFilterByKey('SortingBy').Value = ViewBagSortingBy;
    if (ViewBagSortAscending)
        GetFilterByKey('SortAscending').Value = ViewBagSortAscending;
    RefreshFiltersRecipeOwnerRadios();
    AjaxGetRecipes();
    window.onscroll = function (ev) {
        if (window.innerHeight + window.pageYOffset + 20 >= document.body.offsetHeight) {
            window.scrollBy(0, -16);
            AjaxGetRecipes();
        }
    };    
    $('#SetRecipeViewableTypePublic').on('click', function (e) {
        SetRecipeViewableType('Public', ClickedRecipeId);
    });
    $('#SetRecipeViewableTypeFollowers').on('click', function (e) {
        SetRecipeViewableType('Followers', ClickedRecipeId);
    });
    $('#SetRecipeViewableTypeSecret').on('click', function (e) {
        SetRecipeViewableType('Secret', ClickedRecipeId);
    });
    RefreshFiltersSortingTable();
    if (RecipeId)//This should be last
        PreviewRecipe(RecipeId);
});
function AddRecipeCommentDiv(prepend, RecipeCommentDataTransferObject, RecipeCommentsDiv) {
    var CommentHTML = '<div id="RecipeComment' + RecipeCommentDataTransferObject.Id + '" class="row" style="padding-top:4px;margin:0;margin-top:4px;border-radius:12px;background-color:' + GetRandomColor() + '">'
        + '<div id="CommentText' + RecipeCommentDataTransferObject.Id + '" class="col-12" style=padding-top:4px;font-size:18px;>'
        + RecipeCommentDataTransferObject.Text
        + '</div>'
        + '<div class="col-12 text-right" style="font-size:14px;">';
    if (RecipeCommentDataTransferObject.IsMyRecipe)
        CommentHTML += '<a id="EditOrSaveCommentButton' + RecipeCommentDataTransferObject.Id + '" href="javascript:EditRecipeComment(' + RecipeCommentDataTransferObject.Id + ')">Edit</a> '
            + '| <a id="DeleteOrCancelCommentButton' + RecipeCommentDataTransferObject.Id + '" href="javascript:DeleteRecipeComment(' + RecipeCommentDataTransferObject.Id + ');">Delete</a>';
    CommentHTML += '</div>'
        + '<div class="col-12" style="font-size:14px;">'
        + '<div class="row">'
        + '<div class="col-4" style="padding-left:6px;padding-right:0;">'
        + '<span id="RecipeCommentVotes' + RecipeCommentDataTransferObject.Id + '">' + RecipeCommentDataTransferObject.TotalVoteValue + ' </span>';
    if (RecipeCommentDataTransferObject.MyVote === null) {
        CommentHTML += '<input id="RecipeCommentLikeButton' + RecipeCommentDataTransferObject.Id+'" onclick="CreateCommentVote(' + RecipeCommentDataTransferObject.Id + ', true)" type="image" src="/Content/Images/LikeWhite.png"style="vertical-align:middle;width:20px;height:20px;margin-bottom:4px;"/>'
        CommentHTML += '<input id="RecipeCommentDislikeButton' + RecipeCommentDataTransferObject.Id +'" onclick="CreateCommentVote(' + RecipeCommentDataTransferObject.Id + ', false)" type="image" src="/Content/Images/DislikeWhite.png"style="vertical-align:middle;width:20px;height:20px;margin-left:8px;margin-bottom:4px;"/>'
    }
    else if (RecipeCommentDataTransferObject.MyVote.VoteValue > 0) {
        CommentHTML += '<input id="RecipeCommentLikeButton' + RecipeCommentDataTransferObject.Id +'" onclick="CreateCommentVote(' + RecipeCommentDataTransferObject.Id + ', true)" type="image" src="/Content/Images/LikeLightGreen.png"style="vertical-align:middle;width:20px;height:20px;margin-bottom:4px;"/>'
        CommentHTML += '<input id="RecipeCommentDislikeButton' + RecipeCommentDataTransferObject.Id +'" onclick="CreateCommentVote(' + RecipeCommentDataTransferObject.Id + ', false)" type="image" src="/Content/Images/DislikeWhite.png"style="vertical-align:middle;width:20px;height:20px;margin-left:8px;margin-bottom:4px;"/>'
    }
    else {
        CommentHTML += '<input id="RecipeCommentLikeButton' + RecipeCommentDataTransferObject.Id +'" onclick="CreateCommentVote(' + RecipeCommentDataTransferObject.Id + ', true)" type="image" src="/Content/Images/LikeWhite.png"style="vertical-align:middle;width:20px;height:20px;margin-bottom:4px;"/>'
        CommentHTML += '<input id="RecipeCommentDislikeButton' + RecipeCommentDataTransferObject.Id +'" onclick="CreateCommentVote(' + RecipeCommentDataTransferObject.Id + ', false)" type="image" src="/Content/Images/DislikeLightRed.png"style="vertical-align:middle;width:20px;height:20px;margin-left:8px;margin-bottom:4px;"/>'
    }
    CommentHTML += '</div>'
        + '<div class="col-8 text-right" style="padding-left:0;padding-right:6px;">'
        + RecipeCommentDataTransferObject.PostedByChefName + ' ' + ConvertJSONDateToString(RecipeCommentDataTransferObject.CreateDateUtc)
        + '</div>'
        + '</div>'
        + '</div>'
        + '</div>';
    if (prepend === true)
        RecipeCommentsDiv.prepend(CommentHTML);
    else
        RecipeCommentsDiv.append(CommentHTML);
}
function AjaxGetRecipes() {
    RecipesArray = [];//Clear Recipe Array
    $('#NoMoreDiv').text('Loading...');
    if (IsGettingRecipes)
        return;
    IsGettingRecipes = true;
    ajax = new XMLHttpRequest();
    ajax.upload.addEventListener("progress", AjaxGetRecipesProgressHandler, false);
    ajax.addEventListener("load", AjaxGetRecipesCompleteHandler, false);
    ajax.open("GET", "/Recipes?ReturnJson=true&recipesPageIndex=" + RecipesPageIndex + "&" + GenerateFiltersQueryString() + "&R=" + new Date().getTime()); //This R param is in place to prevent caching in the ajax call
    ajax.onreadystatechange = function () {
        if (this.readyState === 4 && this.status === 200) {
            var Recipes = JSON.parse(this.responseText);
            if (Recipes.length === 0)
                $('#NoMoreDiv').text('No more recipes');
            if (Recipes === "Unauthorized") {
                ShowPopUpModal("Error", "You must be singed in to use these filters");
                return;
            }
            for (var i = 0; i < Recipes.length; i++) {
                RecipesArray.push(Recipes[i]);
                var CurrentRecipe = RecipesArray[i];
                CurrentRecipe.CompleteDateUtc = ConvertJSONDateToString(CurrentRecipe.CompleteDateUtc);
                CurrentRecipe.LastEditDateUtc = ConvertJSONDateToString(CurrentRecipe.LastEditDateUtc);
                var isDraft = false;
                var BackgroundColor = GetRandomColor();
                var BorderColor = '';
                var Description = CurrentRecipe.Description;
                var RecipeName = CurrentRecipe.Name;
                var MainCloudFileUrl = null;
                var MainCloudFileThumbUrl = null;
                var MainVideoUrl = null;
                var RecipeDivHTMLString = '';
                $.each(CurrentRecipe.RecipeCloudFileDataTransferObjects, function (i, RecipeCloudFileDataTransferObject) {
                    if (RecipeCloudFileDataTransferObject.RecipeCloudFileTypeName === "MainImage") {
                        MainCloudFileUrl = RecipeCloudFileDataTransferObject.CloudFileDataTransferObject.Url;
                        if (RecipeCloudFileDataTransferObject.CloudFileDataTransferObject.CloudFileThumbnailsDataTransferObject !== null)
                            MainCloudFileThumbUrl = RecipeCloudFileDataTransferObject.CloudFileDataTransferObject.CloudFileThumbnailsDataTransferObject.Url;
                    }
                    else if (RecipeCloudFileDataTransferObject.RecipeCloudFileTypeName === "MainVideo")
                        MainVideoUrl = RecipeCloudFileDataTransferObject.CloudFileDataTransferObject.Url;
                });
                if (CurrentRecipe.Description === undefined || CurrentRecipe.Description === null || CurrentRecipe.Description.length < 1)
                    Description = '';
                if (CurrentRecipe.CompleteDateUtc !== null) //If CompleteDateUtc is null it means its a draft                
                    BorderColor = GetRandomColor();                
                else
                {
                    isDraft = true;
                    BorderColor = '#ff2b2b';//Red
                    RecipeName = '(Draft) ' + CurrentRecipe.Name;
                }  
                //if (CurrentRecipe.IsSaved == true)
                //    BorderColor = SavedBorderColor;
                RecipeDivHTMLString += '<div class="ClickableRecipeDiv box" id="ClickableRecipeDiv' + CurrentRecipe.Id + '">'
                    + '<div id="ClickableRecipeDivBorder' + CurrentRecipe.Id + '" class="zoomImage" style="padding-bottom:15px;border:2px solid;border-radius:20px;border-color:' + BorderColor + ';background-color:white;">'
                    + '<h3 class="text-center" style="padding-top:4px;padding-bottom:2px;margin:10px;font-weight:bold;border:2px solid;border-color:' + BorderColor +';border-radius:10px;">' + RecipeName + '</h3>';
                if (MainCloudFileThumbUrl)
                    RecipeDivHTMLString += '<div class="text-center" style="padding:2px;"><img class="rounded img-fluid" src="' + MainCloudFileThumbUrl + '" style="max-height:400px;"></div>';
                else if (MainCloudFileUrl)
                    RecipeDivHTMLString += '<div class="text-center" style="padding:2px;"><img class="rounded img-fluid" src="' + MainCloudFileUrl + '" style="max-height:400px;"></div>';
                else
                    RecipeDivHTMLString += '<div class="text-center" style="padding:2px;"><img class="rounded img-fluid" src="' + Config.Images.OpenSourceCookingImageUrl + '" style="max-height:400px;"></div>';
                RecipeDivHTMLString += '<div class="col-12 text-center" style= "padding:2px;" id="SymbolsDiv' + CurrentRecipe.Id + '""></div >';
                RecipeDivHTMLString += '<div style="padding:4px;">'
                    + '<div style="display:none;" id="CompleteDateUtcDiv' + CurrentRecipe.Id + '">' + CurrentRecipe.CompleteDateUtc + '</div>'
                    + '<h6 style="padding:4px;font-weight:bold;">Chef ' + CurrentRecipe.CreatorName + '</h6>'
                    + '<div class="text-right">';
                $.each(Config.FiltersKeyValueList, function (i, FiltersKeyValue) {
                    if (FiltersKeyValue.Key === 'SortingBy') {
                        //0-LastEditDateUtc, 1-CompleteDateUtc, 2-RecipeName, 3-Username
                        switch (FiltersKeyValue.Value) {
                            case 0:
                                {
                                    //RecipeDivHTMLString += 'Edited ' + CurrentRecipe.LastEditDateUtc;
                                    break;
                                }
                            case 1:
                                {
                                    //if (CurrentRecipe.CompleteDateUtc === null)
                                    //    RecipeDivHTMLString += 'Created ' + CurrentRecipe.LastEditDateUtc;
                                    //else
                                    //    RecipeDivHTMLString += 'Created ' + CurrentRecipe.CompleteDateUtc;
                                    break;
                                }
                            case 2:
                                {
                                    //RecipeName always shows
                                    break;
                                }
                            case 3:
                                {
                                    //Chef Name always shows
                                    break;
                                }
                        }
                    }
                });
                +'</div >'
                    + '</div>';
                if (CurrentRecipe.IsMyRecipe === true) {                    
                    RecipeDivHTMLString += '<div>'
                        + '<div class="btn-group btn-group-sm btn-group-justified" style="padding:4px;">';
                    if (!isDraft) //Drafts cant be set to anything but secret
                        switch (CurrentRecipe.ViewableType) {
                            case "Public":
                                RecipeDivHTMLString += '<a class="btn btn-sm btn-success btn-bordered PublicOrsecretButton" href="javascript:null" id="PublicOrsecretButton' + CurrentRecipe.Id + '">Public</a>';
                                break;
                            case "Followers":
                                RecipeDivHTMLString += '<a class="btn btn-sm btn-warning btn-bordered PublicOrsecretButton" href="javascript:null" id="PublicOrsecretButton' + CurrentRecipe.Id + '">Followers</a>';
                                break;
                            case "Secret":
                                RecipeDivHTMLString += '<a class="btn btn-sm btn-secondary btn-bordered PublicOrsecretButton" href="javascript:null" id="PublicOrsecretButton' + CurrentRecipe.Id + '">secret</a>';
                                break;
                        }
                    if (!isDraft)
                        RecipeDivHTMLString += '<a class="btn btn-sm btn-info btn-bordered StopPropagationLink" href="javascript:EditRecipe(' + CurrentRecipe.Id + ', ' + CurrentRecipe.SavedByCount + ')">Edit</a>';
                    else
                        RecipeDivHTMLString += '<a class="btn btn-sm btn-info btn-bordered StopPropagationLink" href="' + Config.Urls.RecipeEditor + '?RecipeId=' + CurrentRecipe.Id + '">Continue</a>';
                    RecipeDivHTMLString += '<a class="btn btn-sm btn-danger btn-bordered StopPropagationLink" href="javascript:DeleteRecipe(' + CurrentRecipe.Id + ', ' + CurrentRecipe.SavedByCount + ');">Delete</a>'
                        + '</div >'
                        + '</div >';
                }
                else
                {
                    RecipeDivHTMLString += '<div class="col-12"><a class="StopPropagationLink" href="javascript:OnClick_ReportRecipe(' + CurrentRecipe.Id + ');">Report</a></div>';
                    if (CurrentRecipe.IsSaved == true)
                        RecipeDivHTMLString += '<a id="SaveRecipeButton' + CurrentRecipe.Id + '" class="btn btn-sm btn-block btn-primary btn-bordered StopPropagationLink" href="javascript:ToggleSaveRecipe(' + CurrentRecipe.Id + ');"><i class="fa fa-star" style="color:yellow;"></i> Unsave</a>';
                    else
                        RecipeDivHTMLString += '<a id="SaveRecipeButton' + CurrentRecipe.Id + '" class="btn btn-sm btn-block btn-primary btn-bordered StopPropagationLink" href="javascript:ToggleSaveRecipe(' + CurrentRecipe.Id + ');"><i class="fa fa-star-o"></i> Save</a>';
                }
                RecipeDivHTMLString += '</div>'
                    + '</div>'
                    + '</div>';
                var $items = $(RecipeDivHTMLString);
                //Add RecipesDiv to isotope and lay out newly appended items
                $('#RecipesDiv').append($items).isotope('appended', $items);
                PopulateSymbolsDiv($('#SymbolsDiv' + CurrentRecipe.Id), CurrentRecipe.DietaryRestrictionDataTransferObjects, false);
            }
        }
        else {
            $('#NoMoreDiv').text('No more recipes');
        }
        $('.zoomImage').hover(function () { $(this).addClass('imageZoomed'); }, function () { $(this).removeClass('imageZoomed'); });
        $('.ClickableRecipeDiv').off();
        $('.ClickableRecipeDiv').on('click', function (e) {
            ClickedRecipeId = $(this).closest('.ClickableRecipeDiv').attr('id').split("ClickableRecipeDiv").pop();
            PreviewRecipe(ClickedRecipeId);
            e.stopPropagation();
        });
        $('.StopPropagationLink').off();
        $('.StopPropagationLink').on('click', function (e) {
            e.stopPropagation();
        });
        $('.PublicOrsecretButton').off();
        $('.PublicOrsecretButton').on('click', function (e) {
            ClickedViewableTypeButton = $(this);
            ClickedRecipeId = $(this).closest('.ClickableRecipeDiv').attr('id').split("ClickableRecipeDiv").pop();
            $('#RecipeViewableTypeModalHeader').text('Should ' + GetRecipeById(ClickedRecipeId).Name + ' be public or secret?');
            $('#RecipeViewableTypeModal').modal('show');
            e.stopPropagation();
        });
        //Add RecipesDiv to Isotope layout after each image loads
        $('#RecipesDiv').imagesLoaded().progress(function () {
            $('#RecipesDiv').isotope('layout');
        });
    };
    ajax.send(null);
    RecipesPageIndex++;
}
function AjaxGetRecipesCompleteHandler() {
    $('#pleaseWaitDialog').modal('hide');
    IsGettingRecipes = false;
}
function AjaxGetRecipesProgressHandler(event) {
    var percent = event.loaded / event.total * 100;
    $('#UploadProgressBar').attr('style', 'width: ' + Math.round(percent) + '%');
}
function CancelEditingComment(commentId, CurrentText) {
    var CommentTextDiv = $('#CommentText' + commentId);
    CommentTextDiv.empty();
    CommentTextDiv.append(CurrentText);
    var DeleteOrCancelCommentButton = $('#DeleteOrCancelCommentButton' + commentId);
    DeleteOrCancelCommentButton.attr('href', 'javascript:DeleteRecipeComment(' + commentId + ')');
    DeleteOrCancelCommentButton.html('Delete');
    var EditOrSaveCommentButton = $('#EditOrSaveCommentButton' + commentId);
    EditOrSaveCommentButton.attr('href', 'javascript:EditRecipeComment(' + commentId + ')');
    EditOrSaveCommentButton.html('Edit');

}
function CreateCommentVote(commentId, isUpVote) {
    if (IsVoteOnCommentBusy === true)
        return;
    IsVoteOnCommentBusy = true;
    $.ajax({
        url: Config.AjaxUrls.AjaxCreateCommentVote,
        type: "GET",
        cache: false,
        data: { commentId: commentId, isUpvote: isUpVote },
        success: function (VoteValue) {
            IsVoteOnCommentBusy = false;
            var RecipeCommentVotesSpan = $('#RecipeCommentVotes' + commentId);
            var TotalVoteValue = +(RecipeCommentVotesSpan.text()) + VoteValue;
            if (VoteValue > 0) {
                $('#RecipeCommentLikeButton' + commentId).attr("src", "/Content/Images/LikeLightGreen.png");
                $('#RecipeCommentDislikeButton' + commentId).attr("src", "/Content/Images/DislikeWhite.png");
            }
            else if (VoteValue < 0) {
                $('#RecipeCommentLikeButton' + commentId).attr("src", "/Content/Images/LikeWhite.png");
                $('#RecipeCommentDislikeButton' + commentId).attr("src", "/Content/Images/DislikeLightRed.png");
            }
            else {
                TotalVoteValue = +(RecipeCommentVotesSpan.text()) + +(VoteValue.split(' ')[1]);
                // 0 means they already voted, which means they are undoing their vote
                $('#RecipeCommentLikeButton' + commentId).attr("src", "/Content/Images/LikeWhite.png");
                $('#RecipeCommentDislikeButton' + commentId).attr("src", "/Content/Images/DislikeWhite.png");
            }
            RecipeCommentVotesSpan.text(TotalVoteValue + ' ');
        },
        error: function (er) {
            ShowPopUpModal("Error", er);
        }
    });
}
function DeleteRecipe(recipeId, SavedByCount) {
    if (SavedByCount !== 0) {
        ShowPopUpModal('This recipe is saved by other users and can no longer be deleted');
        return;
    }
    $('#ConfirmRecipeDeleteModal').modal('show');
    $('#ConfirmRecipeDeleteModalDeleteButton').off();
    $('#ConfirmRecipeDeleteModalDeleteButton').on('click', function (e) {
        $.ajax({
            url: Config.AjaxUrls.AjaxDeleteRecipe,
            type: "GET",
            cache: false,
            data: { recipeId: recipeId },
            success: function (bool) {
                if (bool === false) {
                    ShowPopUpModal('Error', 'Recipe id ' + recipeId + ' was unable to be deleted');
                    return;
                }
                $('#ConfirmRecipeDeleteModal').modal('hide');
                //Remove the recipe from the isotope layout
                $RecipesDiv.isotope('remove', $('#ClickableRecipeDiv' + recipeId)).isotope('layout');
            },
            error: function (er) {
                ShowPopUpModal("Error", er);
            }
        });
    });
}
function DeleteRecipeComment(commentId) {
    $.ajax({
        url: Config.AjaxUrls.AjaxDeleteComment,
        type: "GET",
        cache: false,
        data: { commentId: commentId },
        success: function (bool) {
            RecipeCommentsSkipAdjust--;
            if (bool === true)
                $('#RecipeComment' + commentId).remove();
            else
                ShowPopUpModal("Error", "Oops something bad happend, try refreshing the page or current menu" + er);
        },
        error: function (er) {
            ShowPopUpModal("Error", "Oops something bad happend :( Try again later..." + er);
        }
    });
}
function EditRecipe(recipeId, SavedByCount) {
    if (SavedByCount !== 0)
    {
        ShowPopUpModal('This recipe is saved by other users and can no longer be edited');
        return;
    }
    window.location.href = Config.Urls.RecipeEditor + '?RecipeId=' + recipeId;             
}
function EditRecipeComment(commentId) {
    var CommentTextDiv = $('#CommentText' + commentId);
    var CurrentText = CommentTextDiv.html();
    CommentTextDiv.empty();
    CommentTextDiv.append('<textarea id="EditingCommentTextArea' + commentId + '" class="form-control" rows="3" placeholder="Add Comment (300 character limit)" id="EditingCommentTextArea">' + CurrentText + '</textarea>');
    var EditOrSaveCommentButton = $('#EditOrSaveCommentButton' + commentId);
    EditOrSaveCommentButton.attr('href', 'javascript:SaveCommentChanges(' + commentId + ')');
    EditOrSaveCommentButton.html('Save Changes');
    var DeleteOrCancelCommentButton = $('#DeleteOrCancelCommentButton' + commentId);
    DeleteOrCancelCommentButton.attr('href', 'javascript:CancelEditingComment(' + commentId + ', "' + CurrentText + '")');
    DeleteOrCancelCommentButton.html('Cancel');
}
//function GetDocHeight() {
//    return Math.max(
//        $(document).height(),
//        $(window).height(),
//        document.documentElement.clientHeight
//    );
//}
function FlagRecipe(recipeId, flagName) {
    $.ajax({
        url: Config.AjaxUrls.AjaxCreateRecipeFlag,
        type: "GET",
        cache: false,
        data: { recipeId: recipeId, flagName: flagName },
        success: function (result) {
            $('#ReportModal').modal('hide');
            if (result === 'Already Reported')
            {
                ShowPopUpModal("PopUp", "You've already reported this recipe");
                return
            }            
            ShowPopUpModal("PopUp", "Thanks for reporting. The Recipe has been flaged for review");
            $RecipesDiv.isotope('remove', $('#ClickableRecipeDiv' + recipeId)).isotope('layout');
        },
        error: function (er) {
            ShowPopUpModal("Error", "Oops something bad happend :( Try again later..." + er);
        }
    });
}
function GetRecipeById(recipeId) {    
    var r = null;
    $.each(RecipesArray, function (i, Recipe) {        
        if (Recipe.Id === parseInt(recipeId))
            r = Recipe;
    });
    return r;
}
function HideRecipeComments() {
    RecipeCommentsPageIndex = 0;
    RecipeCommentsSkipAdjust = 0;
    ShowingRecipeComments = false;
    $('#RecipeCommentsAreaDiv').hide();
    $('#ShowRecipeCommentsButton').html('Show comments');
    $('#ShowRecipeCommentsButton').removeClass('btn-info').addClass('btn-outline-info');
    $('#RecipeComments').empty();
}
function HideRecipeSteps() {
    ShowingRecipeSteps = false;
    $('#ShowRecipeStepsButton').html('Show Steps');
    $('#ShowRecipeStepsButton').removeClass('btn-info').addClass('btn-outline-info');
    $('#RecipeStepsDiv').empty();
}
function OnClick_FilterModalClearButton() {
    $.each(Config.FiltersKeyValueList, function (i, FiltersKeyValue) {
        FiltersKeyValue.Value = null;
    });
    SearchRecipes();
}
function OnClick_ReportRecipe(recipeId) {
    $.ajax({
        url: Config.AjaxUrls.AjaxGetFlags,
        type: "GET",
        cache: false,
        data: {},
        success: function (FlagNames) {
            var ReportModalBodyDiv = $('#ReportModalBody');
            ReportModalBodyDiv.empty();
            $.each(FlagNames, function (i, FlagName) {
                ReportModalBodyDiv.append('<button type="button" class="btn btn-primary btn-block btn-bordered" onclick="FlagRecipe(\'' + recipeId + '\',\'' + FlagName + '\')">' + FlagName + '</button>');
            });
            $('#ReportModal').modal('show');
        },
        error: function (er) {
            ShowPopUpModal("Error", "Oops something bad happend :( Try again later..." + er);
        }
    });
}
function Onclick_ShowComments() {
    if (ShowingRecipeComments === true)
        HideRecipeComments();
    else
        ShowRecipeComments();
}
function Onclick_ShowRecipeSteps() {
    if (ShowingRecipeSteps === true)
        HideRecipeSteps();
    else
        ShowRecipeSteps();
}
function Onclick_SubmitComment() {
    var Text = $('#RecipeCommentTextArea').val();
    if (Text.length < 1) {
        return;
    }
    if (Text.length > 300) {
        ShowPopUpModal('Validation', "Comment can only be 300 characters");
        return;
    }
    $('#SubmitCommentButton').hide();
    $.ajax({
        url: Config.AjaxUrls.AjaxCreateRecipeComment,
        type: "GET",
        cache: false,
        data: { recipeId: ClickedRecipeId, text: Text },
        success: function (RecipeCommentDataTransferObject) {
            RecipeCommentsSkipAdjust++;
            AddRecipeCommentDiv(true, RecipeCommentDataTransferObject, $('#RecipeComments'));
            $('#RecipeCommentTextArea').val('');
            $('#SubmitCommentButton').fadeIn('slow');
        },
        error: function (er) {
            ShowPopUpModal("Error", "Oops something bad happend :( Try again later..." + er);
        }
    });
    $('#RecipeCommentCharactersLeftSpan').text(MaxRecipeCommentLength);
}
function PopulateSymbolsDiv(Div, DietaryRestrictions, OnClickOpensSymbolsLegend) {
    Div.empty();
    $.each(DietaryRestrictions, function (i, DietaryRestriction) {
        Div.append('<span><img class="img-fluid" style="width:56px;height:56px;" src="' + DietaryRestriction.IconUrl + '" /></span>');
    });
}
function PreviewRecipe(recipeId) {
    ClickedRecipeId = recipeId;
    $('#RecipePreviewerIngredientSummaryList').empty();
    //Get Ingredients Async   
    $.ajax({
        url: Config.AjaxUrls.AjaxGetRecipeStepsIngredients,
        type: "GET",
        cache: false,
        data: { recipeId: recipeId },
        success: function (RecipeStepsIngredientsDataTransferObjects) {
            $.each(RecipeStepsIngredientsDataTransferObjects, function (i, RecipeStepsIngredientsDataTransferObject) {
                $('#RecipePreviewerIngredientSummaryList').append('<li>' + GetIngredientAmountString(RecipeStepsIngredientsDataTransferObject) + '</li>');
            });
        },
        error: function (er) {
            ShowPopUpModal("Error", er);
        }
    });

    $.ajax({
        url: Config.AjaxUrls.AjaxGetRecipe,
        type: "GET",
        cache: false,
        data: { recipeId: recipeId },
        success: function (RecipeDataTransferObject) {
            HideRecipeSteps();
            HideRecipeComments();
            $('#ModalMainRecipeVideo').hide();
            $('#ModalMainRecipeImg').hide();

            //Populate the Recipe Previewer
            $('#ModalRecipeHeader').text(RecipeDataTransferObject.Name);
            $('#ModalCreatorName').text('Chef ' + RecipeDataTransferObject.CreatorName);
            $('#ModalCompleteDateUtc').text('Created ' + ConvertJSONDateToString(RecipeDataTransferObject.CompleteDateUtc));
            $('#ModalEditDate').text('Edited ' + ConvertJSONDateToString(RecipeDataTransferObject.LastEditDateUtc));
            $('#ModalRecipeDescriptionSpan').text(RecipeDataTransferObject.Description);
            $('#RecipePreviewerEstMinutes').text(GetEstimatedTimeString(RecipeDataTransferObject.EstimatedTimeInSeconds));

            //Populate Photo & Video
            var MainCloudFileUrl = null;
            var MainCloudFileThumbUrl = null;
            var MainVideoUrl = null;
            $.each(RecipeDataTransferObject.RecipeCloudFileDataTransferObjects, function (i, RecipeCloudFileDataTransferObject) {
                if (RecipeCloudFileDataTransferObject.RecipeCloudFileTypeName === "MainImage") {
                    MainCloudFileUrl = RecipeCloudFileDataTransferObject.CloudFileDataTransferObject.Url;
                    if (RecipeCloudFileDataTransferObject.CloudFileDataTransferObject.CloudFileThumbnailsDataTransferObject !== null)
                        MainCloudFileThumbUrl = RecipeCloudFileDataTransferObject.CloudFileDataTransferObject.CloudFileThumbnailsDataTransferObject.Url;
                }
                else if (RecipeCloudFileDataTransferObject.RecipeCloudFileTypeName === "MainVideo")
                    MainVideoUrl = RecipeCloudFileDataTransferObject.CloudFileDataTransferObject.Url;
            });            
            
            if (MainVideoUrl) {
                $('#ModalMainRecipeVideo').attr('src', MainVideoUrl);
                if (MainCloudFileUrl)
                    $('#ModalMainRecipeVideo').attr('poster', MainCloudFileUrl);
                else
                    $('#ModalMainRecipeVideo').attr('poster', Config.Images.OpenSourceCookingImageUrl);
                $('#ModalMainRecipeVideo').fadeIn();
            }
            else if (MainCloudFileUrl) {
                $('#ModalMainRecipeImg').attr('src', MainCloudFileUrl);
                $('#ModalMainRecipeImg').fadeIn();
            }
            else
            {
                $('#ModalMainRecipeImg').attr('src', Config.Images.OpenSourceCookingImageUrl);
                $('#ModalMainRecipeImg').fadeIn();
            }
            //Populate SymbolsDiv
            var RecipePreviewerSymbolsDiv = $('#RecipePreviewerSymbolsDiv').empty();
            $.each(RecipeDataTransferObject.DietaryRestrictionDataTransferObjects, function (i, DietaryRestriction) {
                RecipePreviewerSymbolsDiv.append('<img class="img-fluid zoomImage" onclick="ShowDietaryRestrictionsSymbolLegend($(\'#SymbolsLegendModalSymbolsTable\'));$(\'#SymbolsLegendModal\').modal(\'show\');" style="padding-right:3px;width:56px;height:56px;" src="' + DietaryRestriction.IconUrl + '" />');       
                RefreshZoomImages();
            });
            $('#RecipePreviewer').modal('show');
        },
        error: function (er) {
            ShowPopUpModal("Error", er);
        }
    });    
}
function SaveCommentChanges(commentId) {
    var NewText = $('#EditingCommentTextArea' + commentId).val();
    var CommentTextDiv = $('#CommentText' + commentId);
    CommentTextDiv.empty();
    $.ajax({
        url: Config.AjaxUrls.AjaxEditComment,
        type: "GET",
        cache: false,
        data: { commentId: commentId, text: NewText },
        success: function (Text) {
            CommentTextDiv.append(Text);
            var DeleteOrCancelCommentButton = $('#DeleteOrCancelCommentButton' + commentId);
            DeleteOrCancelCommentButton.attr('href', 'javascript:DeleteRecipeComment(' + commentId + ')');
            DeleteOrCancelCommentButton.html('Delete');
            var EditOrSaveCommentButton = $('#EditOrSaveCommentButton' + commentId);
            EditOrSaveCommentButton.attr('href', 'javascript:EditRecipeComment(' + commentId + ')');
            EditOrSaveCommentButton.html('Edit');
        },
        error: function (er) {
            ShowPopUpModal("Error", "Oops something bad happend :( Try again later..." + er);
        }
    });
}
function ToggleSaveRecipe(recipeId) {
    var SaveRecipeButton = $('#SaveRecipeButton' + recipeId);
    SaveRecipeButton.hide();
    $.ajax({
        url: Config.AjaxUrls.AjaxToggleSaveRecipe,
        type: "GET",
        cache: false,
        data: { recipeId: recipeId },
        success: function (IsSaved) {
            if (IsSaved === 'No AspNetId')            
                ShowPopUpModal('You must be logged in');            
            if (IsSaved === true)            
                SaveRecipeButton.html('<i class="fa fa-star" style="color:yellow;"></i> Unsave</a>');
            else
            {
                SaveRecipeButton.html("<i class='fa fa-star-o'></i> Save</a>");
                $('#ClickableRecipeDivBorder' + recipeId).css('border-color', GetRandomColor());
            }
            SaveRecipeButton.css('color', 'white');
            SaveRecipeButton.fadeIn('slow');
        },
        error: function (er) {
            ShowPopUpModal("Error", "Oops something bad happend :( Try again later..." + er);
            SaveRecipeButton.fadeIn('slow');
        }
    });
}
function RefreshFiltersRecipeOwnerRadios() {
    if (GetFilterByKey('RecipeOwner').Value === 'NotMine')
        $('#FiltersNotMyRecipesRadio').prop('checked', true);
    else if (GetFilterByKey('RecipeOwner').Value === 'Mine')
        $('#FiltersMyRecipesRadio').prop('checked', true);
    else
        $('#FiltersBothRecipesRadio').prop('checked', true);
}
function RefreshFiltersSortingTable() {
    var FiltersSortByTable = $('#FiltersSortByTable');
    FiltersSortByTable.empty();  
    $.each(Config.OrderByOptions, function (i, OrderByOption) {
        var trHTMLString = '<tr><td>' + OrderByOption.Key + '</td><td><div class="btn-group btn-group-sm" style="padding-bottom:4px;width:100%;">';
        //Date OrderBy is inverted
        if (OrderByOption.Key === 'Create Date' || OrderByOption.Key === 'Edit Date') {
            trHTMLString += '<button class="SortByButton btn btn-bordered" id="SortByButtonFALSE' + OrderByOption.Value.toString().toUpperCase() + '" style="padding-top:4px; padding-bottom:2px;" onclick="SwitchSortBy(' + OrderByOption.Value + ',false);"><i class="fa fa-arrow-down" aria-hidden="true"></i></button>';
            trHTMLString += '<button class="SortByButton btn btn-bordered" id="SortByButtonTRUE' + OrderByOption.Value.toString().toUpperCase() + '" style="padding-top:4px;padding-bottom:2px;" onclick="SwitchSortBy(' + OrderByOption.Value + ',true);"><i class="fa fa-arrow-up" aria-hidden="true"></i></button></div></td></tr>';
        }
        else {
            trHTMLString += '<button class="SortByButton btn btn-bordered" id="SortByButtonTRUE' + OrderByOption.Value.toString().toUpperCase() + '" style="padding-top:4px; padding-bottom:2px;" onclick="SwitchSortBy(' + OrderByOption.Value + ',true);"><i class="fa fa-arrow-down" aria-hidden="true"></i></button>';
            trHTMLString += '<button class="SortByButton btn btn-bordered" id="SortByButtonFALSE' + OrderByOption.Value.toString().toUpperCase() + '" style="padding-top:4px;padding-bottom:2px;" onclick="SwitchSortBy(' + OrderByOption.Value + ',false);"><i class="fa fa-arrow-up" aria-hidden="true"></i></button></div></td></tr>';
        }        
        FiltersSortByTable.append(trHTMLString);
    });
    var sortAscending = GetFilterByKey('SortAscending').Value
    if (!sortAscending)
        sortAscending = 'FALSE';
    var SortingByValue = GetFilterByKey('SortingBy').Value
    if (!SortingByValue)
        SortingByValue = '0';
    $('.SortByButton').removeClass('btn-info').addClass('btn-light');   
    $('#SortByButton' + sortAscending.toString().toUpperCase() + SortingByValue.toString().toUpperCase()).removeClass("btn-light").addClass("btn-info");
}
function SetRecipeViewableType(viewableTypeName, recipeId) {
    $.ajax({
        url: Config.AjaxUrls.AjaxUpdateViewableType,
        type: "GET",
        cache: false,
        data: { recipeId: recipeId, viewableTypeName: viewableTypeName },
        success: function (TypeName) {
            ClickedViewableTypeButton.removeClass('btn-success btn-warning btn-secondary');
            switch (TypeName) {
                case "Public":
                    ClickedViewableTypeButton.addClass('btn-success');
                    ClickedViewableTypeButton.text('Public');
                    break;
                case "Followers":
                    ClickedViewableTypeButton.addClass('btn-warning');
                    ClickedViewableTypeButton.text('Followers');
                    break;
                case "Secret":
                    ClickedViewableTypeButton.addClass('btn-secondary');
                    ClickedViewableTypeButton.text('Secret');
                    break;
                default:
                    ShowPopUpModal("Error", "TypeName not valid");
            }
            $('#RecipeViewableTypeModal').modal('hide');
        },
        error: function (er) {
            ShowPopUpModal("Error", er);
        }
    });
}
function ShowMoreRecipeComments() {
    RecipeCommentsPageIndex++;
    ShowRecipeComments();
}
function ShowRecipeSteps() {
    ShowingRecipeSteps = true;
    $.ajax({
        url: Config.AjaxUrls.AjaxGetRecipeSteps,
        type: "GET",
        cache: false,
        data: { recipeId: ClickedRecipeId },
        success: function (RecipeSteps) {            
            $('#ShowRecipeStepsButton').html('Hide Recipe Steps');
            $('#ShowRecipeStepsButton').removeClass('btn-outline-info').addClass('btn-info');
            var RecipeStepsDiv = $('#RecipeStepsDiv');
            RecipeStepsDiv.empty();
            $.each(RecipeSteps, function (i, RecipeStep) {
                var AppendString = '<div class="row RecipeStep" style="padding-top:4px;margin:6px;border-radius:12px;background-color:' + GetRandomColor() + '">'
                    + '<div class="col-12">'
                    + '<label style="font-size:20px;font-weight:bold;">Step ' + (i + 1) + '</label>'
                    + '</div>'
                    + '<div class="col-6">'
                    + '<label>' + RecipeStep.Comment + '</label>'
                    + '</div>'
                    + '<div class="col-6">'
                    + '<div class="row">';
                if (RecipeStep.EstimatedTimeInSeconds > 0)
                    AppendString += '<div class="col-12 text-right" style="font-size:20px;">' + GetEstimatedTimeString(RecipeStep.EstimatedTimeInSeconds) + '</div>';
                if (RecipeStep.RecipeStepsIngredientsDataTransferObjects.length > 0)
                    AppendString += '<div class="col-12 text-right" style="font-size:20px;">Ingredients</div>';
                AppendString += '<div class="col-12">'
                    + '<table style="width:100%;" id="JustAddedStepIngredientTable">'
                    + '<tbody></tbody>'
                    + '</table>'
                    + '</div>'
                    + '</div>'
                    + '</div>'
                    + '<div class="col-12 text-center" style="padding-bottom:6px;">'
                    + '<div class="row">';                
                //Step Photos
                $.each(RecipeStep.RecipeStepsCloudFileDataTransferObjects, function (i, RecipeStepsCloudFileDataTransferObject) {
                    $('#OptionalPhotoVideoSpan').fadeIn('slow');
                    AppendString = AppendString + '<div class="col-6" style="padding-left:4px;padding-right:4px;"><img src="' + RecipeStepsCloudFileDataTransferObject.Url + '" class="img-fluid zoomImage FullscreenableImg" style="border-radius:28px;max-height:180px;"></div>';
                });         
                AppendString = AppendString + '</div>'
                    + '</div>';
                RecipeStepsDiv.append(AppendString);
                RefreshZoomImages();
                RefreshFullScreenImages();
                $.each(RecipeStep.RecipeStepsIngredientsDataTransferObjects, function (i, RecipeStepsIngredientsDataTransferObject) {
                    $('#JustAddedStepIngredientTable tbody').append('<tr><td align="right">' + GetIngredientAmountString(RecipeStepsIngredientsDataTransferObject) + '</td></tr>');
                });
                $('#JustAddedStepPhotoTable').removeAttr('id');
                $('#JustAddedStepIngredientTable').removeAttr('id');
                $('#OptionalPhotoVideoSpan').removeAttr('id');    
                RecipeStepsDiv.show();
            });
        },
        error: function (er) {
            ShowingRecipeSteps = false;
            ShowPopUpModal("Error", "Oops something bad happend :( Try again later..." + er);
        }
    });
}
function ShowRecipeComments() {
    ShowingRecipeComments = true;
    $('#ShowRecipeCommentsButton').html('Hide Comments');
    $('#ShowRecipeCommentsButton').removeClass('btn-outline-info').addClass('btn-info');
    var RecipeCommentsDiv = $('#RecipeComments');
    if (RecipeCommentsPageIndex === 0)
        RecipeCommentsDiv.empty();
    $.ajax({
        url: Config.AjaxUrls.AjaxGetRecipeComments,
        type: "GET",
        cache: false,
        data: { recipeId: ClickedRecipeId, commentsPageIndex: RecipeCommentsPageIndex, recipeCommentsSkipAdjust: RecipeCommentsSkipAdjust },
        success: function (RecipeCommentDataTransferObjects) {
            $('#RecipeCommentsAreaDiv').show();
            if (RecipeCommentDataTransferObjects.length < 10)
                $('#ShowMoreRecipeCommentsButton ').html('No More Comments');
            else
                $('#ShowMoreRecipeCommentsButton').html('Show More Comments');
            $.each(RecipeCommentDataTransferObjects, function (i, RecipeCommentDataTransferObject) {
                AddRecipeCommentDiv(false, RecipeCommentDataTransferObject, RecipeCommentsDiv);
            });
        },
        error: function (er) {
            ShowingRecipeComments = false;
            ShowPopUpModal("Error", er);
        }
    });
}
function SwitchSortBy(sortByOptionValue, sortAscending) {    
    if (sortByOptionValue !== null)
        GetFilterByKey('SortingBy').Value = sortByOptionValue;
    GetFilterByKey('SortAscending').Value = sortAscending;  
    RefreshFiltersSortingTable();
}