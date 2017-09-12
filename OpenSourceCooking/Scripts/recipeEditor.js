var DietaryRestrictions = [];
var IsAddDeleteMoveRecipeStepBusy = false; //Prevents spamining the AddStep button errors
var IsNextButtonBusy = false;
var MaxRecipeDescriptionLength = 300;
var StepNumber = 0;//Use ChangeStepNumber() to set
var VideoUpload = null;

//SaveTimers remeber to add it to the OnClick_CompleteRecipe check 
//(Should be converted to array to avoid needing to remeber)
var DietaryRangeSliderSaveTimer = null;
var DescriptionSaveTimer = null;
var RecipeNameSaveTimer = null;
var ServingSizeSaveTimer = null;

var EditingRecipeStep = {
    RecipeId: 0,
    StepNumber: 0,
    Comment: '',
    EstimatedTimeInSeconds: 0,
    RecipeStepsIngredients: [
        {
            Amount: null,
            IngredientName: null,
            MeasurementUnitName: null,
            RecipeId: null,
            StepNumber: null,
            ToAmount: null,
            RecipeStepsIngredientsIngredientModifiers: [
                {
                    Id: null,
                    IngredientName: null,
                    ModifierName: null,
                    RecipeId: null,
                    StepNumber: null,
                }
            ]
        },
    ]
};

var EditingRecipe = {
    Id: 0,
    Name: '',
    CreationStep: 0, //0-NotCreated, 1-Name Saved, 2-ServingSize
    VideoCloudFileId: 0,
};


$(document).ready(function () {
    $('#DescriptionInput').attr('maxlength', MaxRecipeDescriptionLength);
    var rangeSlider = function () {
        var slider = $(".range-slider"),
            range = $(".range-slider-range"),
            value = $(".range-slider-value");
        slider.each(function () {
            value.each(function () {
                var value = $(this).prev().attr("value");
                $(this).html(value);
            });
            range.on("input", function () {
                UpdateDietaryRestrictions();
            });
        });
    };
    rangeSlider();
    if (ViewBagRecipeId != 0) {
        EditingRecipe.Id = ViewBagRecipeId;
        //Populate Inputs
        $.ajax({
            url: Config.AjaxUrls.AjaxGetRecipe,
            type: "GET",
            cache: false,
            data: { RecipeId: EditingRecipe.Id },
            success: function (Recipe) {
                var MainCloudFileUrl = null;
                var MainCloudFileThumbUrl = null;
                var MainVideoUrl = null;
                $.each(Recipe.RecipeCloudFileDataTransferObjects, function (i, RecipeCloudFileDataTransferObject) {
                    if (RecipeCloudFileDataTransferObject.RecipeCloudFileTypeName === "MainImage") {
                        MainCloudFileUrl = RecipeCloudFileDataTransferObject.CloudFileDataTransferObject.Url;
                        if (RecipeCloudFileDataTransferObject.CloudFileDataTransferObject.CloudFileThumbnailsDataTransferObject !== null)
                            MainCloudFileThumbUrl = RecipeCloudFileDataTransferObject.CloudFileDataTransferObject.CloudFileThumbnailsDataTransferObject.Url;
                    }
                    else if (RecipeCloudFileDataTransferObject.RecipeCloudFileTypeName === "MainVideo")
                        MainVideoUrl = RecipeCloudFileDataTransferObject.CloudFileDataTransferObject.Url;
                }); 

                EditingRecipe.Id = Recipe.Id;
                EditingRecipe.CreationStep = Recipe.CreationStep;
                EditingRecipe.VideoCloudFileId = Recipe.VideoCloudFileId;
                $("#RecipeNameInput").val(Recipe.Name);
                $('#RecipeDescriptionCharactersLeftSpan').text(MaxRecipeDescriptionLength - Recipe.Description.length);
                if (Recipe.CreationStep > -1) {
                    $('#RecipeServingSizeDiv').show();
                    $("#CompleteButton").fadeIn("slow");
                }
                if (Recipe.CreationStep > 0) {
                    if (Recipe.ServingSize > 0)
                        $('#ServingSizeInput').val(Recipe.ServingSize);
                }
                if (Recipe.CreationStep > 1) {
                    $('#ServingSizeInput').val(Recipe.ServingSize);
                    $('#DietaryRestrictionsSliderDiv').fadeIn('slow');
                    $('#dietary-restrictions-div').fadeIn('slow');
                    $.each(Recipe.DietaryRestrictionDataTransferObjects, function (i, DietaryRestrictionDataTransferObject) {
                        DietaryRestrictions.push(DietaryRestrictionDataTransferObject.Name);
                        switch (DietaryRestrictionDataTransferObject.Name) {
                            case 'Dairy Free':
                                $('#IsDairyFreeCheckbox').prop('checked', true);
                                break;
                            case 'Gluten Free':
                                $('#IsGlutenCheckbox').prop('checked', true);
                                break;
                            case 'GMO Free':
                                $('#IsGMOFreeCheckbox').prop('checked', true);
                                break;
                            case 'Vegan':
                                $('#dietary-range-slider-range').attr('value', 2);
                                break;
                            case 'Vegetarian':
                                $('#dietary-range-slider-range').attr('value', 1);
                                break;
                            default:
                                break;
                        }
                    });
                    RefreshDietarySliderCss(DietaryRestrictions);
                }
                if (Recipe.CreationStep > 2) {
                    $("#DescriptionInput").val(Recipe.Description);
                    $('#RecipeDescriptionDiv').fadeIn('slow');
                }
                if (Recipe.CreationStep > 3) {
                    PopulateStepsTable(Recipe.Id);
                    $('#MainPhotoDiv').fadeIn("slow");
                    $('#RecipeStepsDiv').fadeIn("slow");
                    $('#StepsHeaderDiv1').fadeIn("slow");
                    $('#StepsHeaderDiv2').fadeIn("slow");
                    $("#CompleteButton").removeClass('btn-danger').addClass('btn-info');
                    $("#CompleteButton").text("Complete Recipe!");
                    $('#NextButton').fadeOut('fast');
                }
                if (Recipe.CreationStep > 4) {
                }
                if (MainCloudFileUrl) {                   
                    $('#PosterImageUploadButton').attr('src', MainCloudFileUrl);
                    $('#PosterImageUploadButton').fadeIn('slow');
                }
                else
                    $('#PosterImageUploadButton').attr('src', Config.Images.AddPhotoImageUrl);
                if (MainVideoUrl) {
                    $('#MainVideo').attr('src', MainVideoUrl);
                    $('#VideoUploadButton').hide();
                    $('#MainVideo').fadeIn('slow');
                }
                else
                    $('#VideoUploadButton').attr('src', Config.Images.AddVideoImageUrl);
            },
            error: function (xhr, ajaxOptions, error) {
                ShowPopUpModal("Error", "Oops something bad happened.. " + xhr.status + ' ' + xhr.responseText);
            }
        });
    }
    $('.AutoSaveInput').keyup(function (event) {
        InputId = $(this).attr('id');        
        var keycode = (event.keyCode ? event.keyCode : event.which);
        if (keycode === 13) {//User Pressed Enter
            switch (InputId) {
                case 'DescriptionInput':
                    $('#RecipeDescriptionCharactersLeftSpan').text(MaxRecipeDescriptionLength - $('#DescriptionInput').val().length);
                    UpdateDescription();
                    break;
                case 'RecipeNameInput':
                    CreateOrEditRecipeName();
                    break;
                case 'ServingSizeInput':
                    UpdateServingSize();
                    break;
            }
        }
        else {
            switch (InputId) {
                case 'DescriptionInput':
                    $('#RecipeDescriptionCharactersLeftSpan').text(MaxRecipeDescriptionLength - $('#DescriptionInput').val().length);
                    $('#DescriptionInputImage').attr("src", Config.Icons.TypingIconUrl);
                    var sec = 1
                    clearInterval(DescriptionSaveTimer);
                    DescriptionSaveTimer = setInterval(function () {
                        sec--;
                        if (sec === 0) {
                            UpdateDescription();
                        }
                    }, 1000);
                    break;
                case 'RecipeNameInput':
                    $('#RecipeNameInputImage').attr("src", Config.Icons.TypingIconUrl);
                    var sec = 1
                    clearInterval(RecipeNameSaveTimer);
                    RecipeNameSaveTimer = setInterval(function () {
                        sec--;
                        if (sec === 0) {
                            CreateOrEditRecipeName();
                        }
                    }, 1000);
                    break;
                case 'ServingSizeInput':
                    $('#ServingSizeInputImage').attr("src", Config.Icons.TypingIconUrl);
                    var sec = 1
                    clearInterval(ServingSizeSaveTimer);
                    ServingSizeSaveTimer = setInterval(function () {
                        sec--;
                        if (sec === 0) {
                            UpdateServingSize();
                        }
                    }, 1000);
                    break;
            }
        }
    });
    $("#IngredientSearch").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: Config.AjaxUrls.AjaxIngredientsAutoComplete,
                data: "{ 'prefix': '" + request.term + "'}", //Not sure why the syntax has to be like this, but it does
                dataType: "json",
                type: "POST",
                cache: false,
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    response($.map(data, function (item) {
                        return item;
                    }));
                },
                error: function (xhr, ajaxOptions, error) {
                    ShowPopUpModal("Error", "Oops something bad happened.. " + xhr.status + ' ' + xhr.responseText);
                }
            });
        },
        select: function (e, i) {
            //alert(i.item.val + ' & ' + i.item.label); Leaving for example
        },
        minLength: 1
    });
    $('#SetRecipeViewableTypePublic').on('click', function (e) {
        UpdateViewableType('Public');
    });
    $('#SetRecipeViewableTypeFollowers').on('click', function (e) {
        UpdateViewableType('Followers');
    });
    $('#SetRecipeViewableTypeSecret').on('click', function (e) {
        UpdateViewableType('Secret');
    });
});

function AddIngredientButtonClicked() {
    $('#AddStepModal').modal('hide');
    $('#AddIngredientToStepModal').modal('show');
    GetUnits();
}
function AddIngredientToRecipeStepIngredientsTable(recipeStepsIngredientsDataTransferObject) {    
    var IngredientRow = '<tr id="' + recipeStepsIngredientsDataTransferObject.IngredientName + '+">'; //Adding a plus symbol to guarantee Unique HTML ID
    IngredientRow = IngredientRow + '<td>' + GetIngredientAmountString(recipeStepsIngredientsDataTransferObject) + '</td>'
        + '<td class="AddedIngredientRow"><a class="fa fa-minus-square-o fa-lg" style="color:red;" aria-hidden="true"></a></td>'
        //These hidden TDs cant have their order rearanged. The column index is used in AjaxUpdateIngredientToStep
        + '<td style="display:none;">' + recipeStepsIngredientsDataTransferObject.Amount + '</td>'
        + '<td style="display:none;">' + recipeStepsIngredientsDataTransferObject.ToAmount + '</td>'
        + '<td style="display:none;">' + recipeStepsIngredientsDataTransferObject.MeasurementUnitName + '</td>'
        + '<td style="display:none;" class="IngredientNameTD">' + recipeStepsIngredientsDataTransferObject.IngredientName + '</td>'
        + '</tr>';
    $('#RecipeStepIngredientsTable > tbody').append(IngredientRow);
    $('.AddedIngredientRow').off('click'); //Remove Event Handlers before adding another
    $('.AddedIngredientRow').on('click', function () {
        var ClickedColIndex = $(this).index();
        var row = $(this).closest('tr');
        var rowIngredientName = row.attr('id').slice(0, -1);
        row.remove();
    });
}
function ChangeStepNumber(StepNum) {
    StepNumber = StepNum;
    $('.StepNumberSpan').each(function (i, obj) {
        obj.innerHTML = StepNumber;
    });
}
function ClearAddIngredientToStepModal() {
    $('.UnitTypeButton').removeClass('btn-success').addClass('btn-info');
    $('#IngredientSearch').val('');
    $('#MeasurementTypeDropDown').val('');
    $('#SelectMeasurementTypeToShowUnitsSpan').fadeIn();
    $('#MeasurementAmount').val('');
    $('#OptionalToAmount').val('');
}
function ClearAddStepModal() {
    $('#AddStepComment').val('');
    $('#RecipeStepIngredientsTable').find('tbody').empty();
    $('#EstimatedStepTime').val('');
    $('#Slot1UploadButton').attr("src", Config.Images.AddPhotoImageUrl);
    $('#Slot2UploadButton').attr("src", Config.Images.AddPhotoImageUrl);
}
function CompleteHandler() {
    $('#pleaseWaitDialog').modal('hide');
}
function CreateOrEditRecipeName() {
    clearInterval(RecipeNameSaveTimer);
    var RecipeNameInputImage = $('#RecipeNameInputImage');
    var RecipeNameInput = $("#RecipeNameInput");
    if (!RecipeNameInput.val()) {
        ShowPopUpModal('Validation', "Recipe name missing");
        RecipeNameInputImage.attr("src", Config.Icons.ErrorIconUrl);
        return;
    }
    else {
        if (RecipeNameInput.val().length > 64) {
            ShowPopUpModal('Validation', "Recipe name can only be 64 characters");
            RecipeNameInputImage.attr("src", Config.Icons.ErrorIconUrl);
            return;
        }
    }
    RecipeNameInputImage.attr("src", Config.Icons.LoadingIconUrl);
    if (EditingRecipe.Id === 0) {
        EditingRecipe.Id = -1; //This is to make sure multiple create request cant fire async
        $.ajax({
            url: Config.AjaxUrls.AjaxCreateRecipe,
            type: "GET",
            cache: false,
            data: { RecipeName: RecipeNameInput.val() },
            success: function (Recipe) {
                RecipeNameSaveTimer = null;
                RecipeNameInput.val(Recipe.Name);
                EditingRecipe.CreationStep = Recipe.CreationStep;
                $("#CompleteButton").fadeIn("slow");
                EditingRecipe.Id = Recipe.Id;
                $('#RecipeServingSizeDiv').fadeIn('slow');
                RecipeNameInputImage.attr("src", "/Content/Images/Icons/Checkmark.png");
            },
            error: function (xhr, ajaxOptions, error) {
                ShowPopUpModal("Error", "Oops something bad happened.. " + xhr.status + ' ' + xhr.responseText);
                RecipeNameInputImage.attr("src", Config.Icons.ErrorIconUrl);
                return;
            }
        });
    }
    else {
        IsNextButtonBusy = true;
        $.ajax({
            url: Config.AjaxUrls.AjaxUpdateName,
            type: "GET",
            cache: false,
            data: { recipeId: EditingRecipe.Id, recipeName: RecipeNameInput.val() },
            success: function (recipeName) {
                RecipeNameSaveTimer = null;
                RecipeNameInput.val(recipeName);                
                RecipeNameInputImage.attr("src", "/Content/Images/Icons/Checkmark.png");
                IsNextButtonBusy = false;
            },
            error: function (xhr, ajaxOptions, error) {
                ShowPopUpModal("Error", "Oops something bad happened.. " + xhr.status + ' ' + xhr.responseText);
                RecipeNameInputImage.attr("src", Config.Icons.ErrorIconUrl);
                IsNextButtonBusy = false;
                return;
            }
        });
    }
}
function FileUploadInput_Changed(Input, Img, RecipeId, StepNum, SlotNum) {
    var files = Input.get(0).files;
    if (!IsFileTypeAllowed(files[0], 'Image'))
        return;
    if (files[0].size > 31457280)//30MB
        ShowPopUpModal('Validation', 'File size is too big');
    $('#pleaseWaitDialog').modal('show');
    var formData = new FormData();
    ajax = new XMLHttpRequest();
    ajax.onreadystatechange = function () {
        if (this.readyState === 4 && this.status === 200) {
            $('#pleaseWaitDialog').modal('show');
            var bool = JSON.parse(this.responseText);
            if (bool === -1)
                ShowPopUpModal('Validation', 'File size is too big');
            else {
                SetSrcFromLocalFile(files[0], Img);
                Img.fadeIn();
            }
            Input.val('');
            $('#pleaseWaitDialog').modal('hide');
        }
    };
    ajax.upload.addEventListener("progress", ProgressHandler, false);
    ajax.addEventListener("load", CompleteHandler, false);
    if (files !== null && files.length > 0) {
        formData.append("UploadedFile", files[0]);
        formData.append("RecipeId", RecipeId);
        formData.append("StepNumber", StepNum);
        formData.append("slotNumber", SlotNum);
        ajax.open("POST", Config.AjaxUrls.AjaxUploadRecipeStepImage);
        ajax.send(formData);
    }
}
function GetUnits() {
    var MeasurementUnitDropDown = $('#MeasurementUnitDropDown');
    $.ajax({
        url: Config.AjaxUrls.AjaxGetUnits,
        data: {},
        dataType: "json",
        type: "GET",
        cache: false,
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            MeasurementUnitDropDown.empty();
            $('#SelectMeasurementTypeToShowUnitsSpan').hide();
            $.each(data, function (i, MeasurementUnit) {
                MeasurementUnitDropDown.append($('<option>', { value: MeasurementUnit.Text }).text(MeasurementUnit.Text));
            });
        },
        error: function (xhr, ajaxOptions, error) {
            ShowPopUpModal("Error", "Oops something bad happened.. " + xhr.status + ' ' + xhr.responseText);
        }
    });
}
function IncrementCreationStep() {
    if (IsNextButtonBusy)
        return; 
    $('#NextButton').fadeOut('fast');
    if (EditingRecipe.CreationStep === 0) {        
        CreateOrEditRecipeName();
        $('#NextButton').fadeIn('fast');
        return;
    }
    else if (EditingRecipe.CreationStep === 1) {
        UpdateServingSize();
        $('#NextButton').fadeIn('fast');
        return;
    }    
    $.ajax({
        url: Config.AjaxUrls.AjaxIncrementCreationStep,
        type: "GET",
        cache: false,
        data: { recipeId: EditingRecipe.Id },
        success: function (CreationStep) {          
            EditingRecipe.CreationStep = CreationStep;
            switch (EditingRecipe.CreationStep) {                
                case 3: //1 - Show Description Div
                    {
                        $('#NextButton').fadeIn('fast');
                        $('#RecipeDescriptionDiv').fadeIn('slow');
                        break;
                    }
                case 4: // 2 - Show Everything Else
                    {
                        PopulateStepsTable(EditingRecipe.Id);
                        $('#MainPhotoDiv').fadeIn("slow");
                        $('#StepsHeaderDiv1').fadeIn("slow");
                        $('#StepsHeaderDiv2').fadeIn("slow");
                        $("#CompleteButton").hide();
                        $("#CompleteButton").removeClass('btn-danger').addClass('btn-info');
                        $("#CompleteButton").text("Complete Recipe!");
                        $("#CompleteButton").fadeIn("slow");
                        break;
                    }
                default:
                    break;
            }
        },
        error: function (xhr, ajaxOptions, error) {
            ShowPopUpModal("Error", "Oops something bad happened.. " + xhr.status + ' ' + xhr.responseText);
        }
    });

}
function MainImageInput_Changed(Input, Img, RecipeId) {
    var File = Input.get(0).files[0];
    if (!IsFileTypeAllowed(File, 'Image'))
        return;
    if (File.size > 31457280)//30MB
        ShowPopUpModal('Validation', 'File size is too big');
    $('#pleaseWaitDialog').modal('show');
    var formData = new FormData();
    ajax = new XMLHttpRequest();
    ajax.onreadystatechange = function () {
        if (this.readyState === 4 && this.status === 200) {
            $('#pleaseWaitDialog').modal('show');
            var bool = JSON.parse(this.responseText);
            if (bool === -1)
                ShowPopUpModal('Validation', 'File size is too big');
            else {
                SetSrcFromLocalFile(File, Img);
                Img.fadeIn();
            }
            Input.val('');
            $('#pleaseWaitDialog').modal('hide');
        }
    };
    ajax.upload.addEventListener("progress", ProgressHandler, false);
    ajax.addEventListener("load", CompleteHandler, false);
    if (File !== null) {
        formData.append("UploadedFile", File);
        formData.append("RecipeId", RecipeId);
        ajax.open("POST", Config.AjaxUrls.AjaxUploadRecipeImage);
        ajax.send(formData);
    }
}
function MainVideoClicked(recipeId, imageButton) {
    $('#RemoveCloudFileModalHeader').text('Remove Video?');
    $('#RemoveCloudFileModal').modal('show');
    $("#RemoveCloudFileModalRemoveButton").click(function () {
        $.ajax({
            url: Config.AjaxUrls.AjaxDeleteVideoCloudFile,
            type: "GET",
            cache: false,
            data: { RecipeId: recipeId },
            success: function (bool) {
                imageButton.attr('src', Config.Images.AddVideoImageUrl);
                $("#RemoveCloudFileModalRemoveButton").unbind();
                $('#RemoveCloudFileModal').modal('hide');
                $('#MainVideo').hide();
                imageButton.fadeIn();
            },
            error: function (xhr, ajaxOptions, error) {
                ShowPopUpModal("Error", "Oops something bad happened.. " + xhr.status + ' ' + xhr.responseText);
            }
        });
    });
}
function MoveRecipeStep(RecipeId, StepNum, MoveByCount) {
    if (IsAddDeleteMoveRecipeStepBusy === true)
        return;
    IsAddDeleteMoveRecipeStepBusy = true;
    $.ajax({
        url: Config.AjaxUrls.AjaxMoveRecipeStep,
        type: "GET",
        cache: false,
        data: { RecipeId: RecipeId, StepNumber: StepNum, MoveBy: MoveByCount },
        success: function (bool) {
            PopulateStepsTable(RecipeId);
        },
        error: function (xhr, ajaxOptions, error) {
            ShowPopUpModal("Error", "Oops something bad happened.. " + xhr.status + ' ' + xhr.responseText);
        }
    });
}
function OnClicked_AddIngredientToStepModalClose() {
    $('#AddStepModal').modal('show');
    $('#AddIngredientToStepModal').modal('hide');
}
function OnClicked_AddStep(RecipeId) {    
    if (IsAddDeleteMoveRecipeStepBusy === true)
        return;
    IsAddDeleteMoveRecipeStepBusy = true;
    $(this).hide();
    console.log(IsAddDeleteMoveRecipeStepBusy);
    $('#AddRecipeStepButton').fadeOut('fast');
    var rowCount = $('.RecipeStep').length;
    ChangeStepNumber(rowCount + 1);
    $.ajax({
        url: Config.AjaxUrls.AjaxCreateRecipeStep,
        type: "GET",
        cache: false,
        data: { RecipeId: RecipeId, StepNumber: StepNumber },
        success: function (RecipeStep) {
            PopulateStepsTable(RecipeId);
            $('#AddRecipeStepButton').fadeIn('fast');
            $(this).fadeIn('fast');           
        },
        error: function (xhr, ajaxOptions, error) {
            ShowPopUpModal("Error", "Oops something bad happened.. " + xhr.status + ' ' + xhr.responseText);
            $(this).fadeIn('fast');
            IsAddDeleteMoveRecipeStepBusy = false;
        }
    });
}
function OnClick_CompleteRecipe(RecipeId) {
    if (DescriptionSaveTimer !== null || DietaryRangeSliderSaveTimer !== null || RecipeNameSaveTimer !== null || ServingSizeSaveTimer !== null) {
        console.log('DescriptionSaveTimer = ' + DescriptionSaveTimer);
        console.log('DietaryRangeSliderSaveTimer = ' + DietaryRangeSliderSaveTimer);
        console.log('RecipeNameSaveTimer = ' + RecipeNameSaveTimer);
        console.log('ServingSizeSaveTimer = ' + ServingSizeSaveTimer);
        ShowPopUpModal("PopUp", "Saving things, hold up!");
        return;
    }
    if (EditingRecipe.CreationStep === 4) {
        $('#RecipeViewableTypeModalHeader').text('Should ' + $('#RecipeNameInput').val() + ' be public, followers only, or secret?');
        $('#RecipeViewableTypeModal').modal('show');
    }
    else //Still a draft
        window.location.href = Config.Urls.RecipesIndex;
}
function PopulateStepsTable(RecipeId) {
    RecipeStepsDiv = $('#RecipeStepsDiv')   
    $.ajax({
        url: Config.AjaxUrls.AjaxGetRecipeSteps,
        type: "GET",
        cache: false,
        data: { recipeId: RecipeId },
        success: function (RecipeSteps) {
            RecipeStepsDiv.empty();
            RecipeStepsDiv.fadeIn('slow');
            var TotalEstimatedTime = 0;
            $.each(RecipeSteps, function (i, RecipeStep) {
                TotalEstimatedTime = TotalEstimatedTime + RecipeStep.EstimatedTimeInSeconds;
                var AppendString = '<div class="row RecipeStep"style="margin-bottom:4px;border-radius:28px;background-color:' + GetRandomColor() + '">'
                    + '<div class="col-4">'
                    + '<label style="font-size:24px;">Step ' + (i + 1) + '</label>'
                    + '</div>'
                    + '<div class="col-8 text-right" style="padding-top:6px;">'
                    + '<div class="btn-group btn-group-sm">';
                //Move Edit Delete Buttons
                if (RecipeStep.StepNumber > 1)
                    AppendString += '<button class="btn btn-sm btn-primary btn-bordered" onclick="MoveRecipeStep(' + RecipeStep.RecipeId + ',' + (i + 1) + ', -1)"><i class="fa fa-arrow-up"></i></button>';
                if (RecipeStep.StepNumber != RecipeSteps.length)
                    AppendString += '<button class="btn btn-sm btn-primary btn-bordered" onclick="MoveRecipeStep(' + RecipeStep.RecipeId + ',' + (i + 1) + ', 1)"><i class="fa fa-arrow-down"></i></button>';
                AppendString += '<button class="btn btn-sm btn-info btn-bordered StepEditButton" id="Edit' + (i + 1) + '">Edit</button>'
                    + '<button class="btn btn-sm btn-danger btn-bordered StepDeleteButton" id="Delete' + (i + 1) + '">Delete</button>'
                    + '</div>'
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
                    AppendString += '<div class="col-6" style="padding-left:4px;padding-right:4px;"><img src="' + RecipeStepsCloudFileDataTransferObject.Url + '" class="mx-auto img-fluid" style="border-radius:28px;max-height:180px;"></div>';
                });
                AppendString += '</div>'
                    + '</div>';
                RecipeStepsDiv.append(AppendString);
                $.each(RecipeStep.RecipeStepsIngredientsDataTransferObjects, function (i, RecipeStepsIngredientsDataTransferObject) {
                    $('#JustAddedStepIngredientTable tbody').append('<tr><td align="right">' + GetIngredientAmountString(RecipeStepsIngredientsDataTransferObject) + '</td></tr>');
                });
                $('#JustAddedStepPhotoTable').removeAttr('id');
                $('#JustAddedStepIngredientTable').removeAttr('id');
                $('#OptionalPhotoVideoSpan').removeAttr('id');
            });
            $('#TotalEstimatedTimeDiv').html(GetEstimatedTimeString(TotalEstimatedTime) + ' to make');
            ChangeStepNumber(0);
            $('.StepDeleteButton').on('click', function () {
                if (IsAddDeleteMoveRecipeStepBusy === true)
                    return;
                IsAddDeleteMoveRecipeStepBusy = true;
                var StepNum = $(this).attr('id').replace("Delete", "");
                $.ajax({
                    url: Config.AjaxUrls.AjaxDeleteRecipeStep,
                    type: "GET",
                    cache: false,
                    data: { RecipeId: RecipeId, StepNumber: StepNum },
                    success: function (RecipeStep) {
                        ChangeStepNumber(0);
                        PopulateStepsTable(RecipeId);
                        $('#AddStepModal').modal('hide');
                    },
                    error: function (xhr, ajaxOptions, error) {
                        ShowPopUpModal("Error", "Oops something bad happened.. " + xhr.status + ' ' + xhr.responseText);
                    }
                });
            });
            $('.StepEditButton').on('click', function () {
                var StepNum = $(this).attr('id').replace("Edit", "");
                ClearAddStepModal();
                $.ajax({
                    url: Config.AjaxUrls.AjaxGetRecipeStep,
                    type: "GET",
                    cache: false,
                    data: { RecipeId: RecipeId, StepNumber: StepNum },
                    success: function (RecipeStepDataTransferObject) {
                        $.each(RecipeStepDataTransferObject.RecipeStepsIngredientsDataTransferObjects, function (i, RecipeStepsIngredientsDataTransferObject) {
                            AddIngredientToRecipeStepIngredientsTable(RecipeStepsIngredientsDataTransferObject);
                        });
                        $('#AddStepComment').val(RecipeStepDataTransferObject.Comment);
                        $('#EstimatedStepTime').val(RecipeStepDataTransferObject.EstimatedTimeInSeconds / 60);
                        $.each(RecipeStepDataTransferObject.RecipeStepsCloudFileDataTransferObjects, function (i, RecipeStepsCloudFileDataTransferObject) {
                            if (RecipeStepsCloudFileDataTransferObject.SlotNumber === 1)
                                $('#Slot1UploadButton').attr("src", RecipeStepsCloudFileDataTransferObject.Url);
                            if (RecipeStepsCloudFileDataTransferObject.SlotNumber === 2)
                                $('#Slot2UploadButton').attr("src", RecipeStepsCloudFileDataTransferObject.Url);
                        });
                    },
                    error: function (xhr, ajaxOptions, error) {
                        ShowPopUpModal("Error", "Oops something bad happened.. " + xhr.status + ' ' + xhr.responseText);
                    }
                });
                ChangeStepNumber(StepNum);
                $('#AddStepModal').modal('show');
            });
            IsAddDeleteMoveRecipeStepBusy = false;
        },
        error: function (xhr, ajaxOptions, error) {
            ShowPopUpModal("Error", "Oops something bad happened.. " + xhr.status + ' ' + xhr.responseText);
        }
    });
}
function ProgressHandler(event) {
    var percent = (event.loaded / event.total) * 100;
    $('#UploadProgressBar').attr('style', 'width: ' + Math.round(percent) + '%');
}
function RefreshDietarySliderCss(DietaryRestrictionNames) {  
    $('#DairyFreeCheckboxSpan').css({ 'color': 'black', 'font-weight': 'normal' });
    $('#GlutenCheckboxSpan').css({ 'color': 'black', 'font-weight': 'normal' });
    $('#GMOFreeCheckboxSpan').css({ 'color': 'black', 'font-weight': 'normal' });
    $('#DietaryRestrictionsSliderDivNoneDiv').css({ 'color': 'black', 'font-weight': 'normal' });
    $('#DietaryRestrictionsSliderDivVeganDiv').css({ 'color': 'black', 'font-weight': 'normal' });
    $('#DietaryRestrictionsSliderDivVegetarianDiv').css({ 'color': 'black', 'font-weight': 'normal' });

      //Set Slider Styles
    if (jQuery.inArray("Vegan", DietaryRestrictionNames) !== -1)
        $('#DietaryRestrictionsSliderDivVeganDiv').css({ 'color': '#28a745', 'font-weight': 'bold' });
    else if (jQuery.inArray("Vegetarian", DietaryRestrictionNames) !== -1)
        $('#DietaryRestrictionsSliderDivVegetarianDiv').css({ 'color': '#28a745', 'font-weight': 'bold' });
    else
        $('#DietaryRestrictionsSliderDivNoneDiv').css({ 'color': '#28a745', 'font-weight': 'bold' });
     //Checkbox Styles    
    if (jQuery.inArray("Dairy Free", DietaryRestrictionNames) !== -1)
        $('#DairyFreeCheckboxSpan').css({ 'color': '#28a745', 'font-weight': 'bold' });
    if (jQuery.inArray("Gluten Free", DietaryRestrictionNames) !== -1)
        $('#GlutenCheckboxSpan').css({ 'color': '#28a745', 'font-weight': 'bold' });
    if (jQuery.inArray("GMO Free", DietaryRestrictionNames) !== -1)
        $('#GMOFreeCheckboxSpan').css({ 'color': '#28a745', 'font-weight': 'bold' });    
}
function ShowRemoveCloudFilePopUp(imageButton, RecipeId, StepNum, SlotNum) {
    $('#RemoveCloudFileModalHeader').text('Remove Photo?');
    $('#RemoveCloudFileModal').modal('show');
    $("#RemoveCloudFileModalRemoveButton").click(function () {
        $.ajax({
            url: Config.AjaxUrls.AjaxDeleteImageCloudFile,
            type: "GET",
            cache: false,
            data: { RecipeId: RecipeId, StepNumber: StepNum, SlotNumber: SlotNum },
            success: function (bool) {
                imageButton.attr('src', Config.Images.AddPhotoImageUrl);
                $("#RemoveCloudFileModalRemoveButton").unbind();
                $('#RemoveCloudFileModal').modal('hide');
                imageButton.fadeIn();
            },
            error: function (xhr, ajaxOptions, error) {
                ShowPopUpModal("Error", "Oops something bad happened.. " + xhr.status + ' ' + xhr.responseText);
            }
        });
    });
}
function UpdateDietaryRestrictions() {
    var sec = 1;
    clearInterval(DietaryRangeSliderSaveTimer);
    DietaryRangeSliderSaveTimer = setInterval(function () {
        sec--;
        if (sec === 0) {
            //Clear all DietaryRestrictions
            DietaryRestrictions = [];
            //Re-Add DietaryRestrictions
            if ($('#IsDairyFreeCheckbox').is(":checked"))
                DietaryRestrictions.push('Dairy Free');
            if ($('#IsGlutenCheckbox').is(":checked"))
                DietaryRestrictions.push('Gluten Free');
            if ($('#IsGMOFreeCheckbox').is(":checked"))
                DietaryRestrictions.push('GMO Free');
            switch ($('#dietary-range-slider-range').val()) {
                case '1':
                    DietaryRestrictions.push('Vegetarian');
                    break;
                case '2':
                    DietaryRestrictions.push('Vegan');
                    break;
                default:
                    break;
            }
            $.ajax({
                url: Config.AjaxUrls.AjaxUpdateDietaryRestrictions,
                data: { recipeId: EditingRecipe.Id, dietaryRestrictionNames: DietaryRestrictions },
                dataType: "json",
                type: "GET",
                cache: false,
                traditional: true,
                success: function (DietaryRestrictionDataTransferObjects) {
                    DietaryRangeSliderSaveTimer = null;
                    DietaryRestrictions = [];
                    $.each(DietaryRestrictionDataTransferObjects, function (i, DietaryRestrictionDataTransferObject) {
                        DietaryRestrictions.push(DietaryRestrictionDataTransferObject.Name);
                    });
                    RefreshDietarySliderCss(DietaryRestrictions)
                },
                error: function (xhr, ajaxOptions, error) {
                    ShowPopUpModal("Error", "Oops something bad happened.. " + xhr.status + ' ' + xhr.responseText);
                }
            });
        }
    }, 1000);
}
function UpdateDescription() {
   
    var Description = $('#DescriptionInput').val();
    if (Description.length > MaxRecipeDescriptionLength) {
        ShowPopUpModal('Validation', "Description can only be " + MaxRecipeDescriptionLength + " characters");
        return;
    }
    var DescriptionInputImage = $('#DescriptionInputImage');
    DescriptionInputImage.attr("src", Config.Icons.LoadingIconUrl);
    $.ajax({
        url: Config.AjaxUrls.AjaxUpdateDescription,
        type: "GET",
        cache: false,
        data: { recipeId: EditingRecipe.Id, description: Description},
        success: function (bool) {
            DescriptionInputImage.attr("src", "/Content/Images/Icons/Checkmark.png");
            DescriptionSaveTimer = null;
        },
        error: function (xhr, ajaxOptions, error) {
            ShowPopUpModal("Error", "Oops something bad happened.. " + xhr.status + ' ' + xhr.responseText);
            ServingSizeInputImage.attr("src", Config.Icons.ErrorIconUrl);
        }
    });
}
function UpdateIngredientToStep(RecipeId) {
    var Amount = $('#MeasurementAmount').val();
    var IngredientName = $('#IngredientSearch').val();
    var MeasurementUnit = $('#MeasurementUnitDropDown').val();
    var OptionalToAmount = $('#OptionalToAmount').val();
    //Validation
    if (IngredientName === null || IngredientName === undefined || IngredientName.length <= 0) {
        ShowPopUpModal('Validation', 'Ingredient name missing');
        return;
    }
    if (MeasurementUnit === null || MeasurementUnit === undefined || MeasurementUnit.length <= 0) {
        ShowPopUpModal('Validation', 'Measurement missing');
        return;
    }
    if (Amount === null || Amount === undefined || Amount.length <= 0) {
        ShowPopUpModal('Validation', 'The amount is missing');
        return;
    }
    var AlreadyExist = false;
    $('#RecipeStepIngredientsTable tr td').each(function () {
        CurrentTD = $(this);
        if (CurrentTD.hasClass('IngredientNameTD') && CurrentTD.html() == IngredientName) {
            ShowPopUpModal('Validation', 'Ingredient already exist in this step');
            return;
        }
    });
    $.ajax({
        url: Config.AjaxUrls.AjaxValidateAmountsThisShouldBeDoneClientSide,
        type: "GET",
        cache: false,
        data: { Amount: Amount, toAmount: OptionalToAmount },
        success: function (RecipeStepsIngredientsDataTransferObject) {
            if (RecipeStepsIngredientsDataTransferObject === 'Amount is not in a valid format' || RecipeStepsIngredientsDataTransferObject === 'The "to" amount is not in a valid format' || RecipeStepsIngredientsDataTransferObject === 'The "to" amount has to be greater')
                ShowPopUpModal('Validation', RecipeStepsIngredientsDataTransferObject);
            else {
                RecipeStepsIngredientsDataTransferObject.RecipeId = RecipeId;
                RecipeStepsIngredientsDataTransferObject.StepNumber = StepNumber;
                RecipeStepsIngredientsDataTransferObject.IngredientName = IngredientName;
                RecipeStepsIngredientsDataTransferObject.MeasurementUnitName = MeasurementUnit;
                $('#AddIngredientToStepModal').modal('hide');
                AddIngredientToRecipeStepIngredientsTable(RecipeStepsIngredientsDataTransferObject);
                ClearAddIngredientToStepModal();
                $('#AddStepModal').modal('show');
            }
        },
        error: function (xhr, ajaxOptions, error) {
            ShowPopUpModal("Error", "Oops something bad happened.. " + xhr.status + ' ' + xhr.responseText);
        }
    });
}
function UpdateRecipeStep(RecipeId) {     

    var RecipeStepsIngredientsArray = new Array();

    $('#RecipeStepIngredientsTable tr').each(function () {
        var tds = $(this).find("td");
        var RecipeStepIngredient = { 'RecipeId': RecipeId, 'StepNumber': StepNumber, 'IngredientName': $(tds[5]).html(), 'MeasurementUnitName': $(tds[4]).html(), 'Amount': $(tds[2]).html(), 'ToAmount': $(tds[3]).html() };
        if (RecipeStepIngredient.ToAmount === null || RecipeStepIngredient.ToAmount === "null" || RecipeStepIngredient.ToAmount.length <= 0)
            RecipeStepIngredient.ToAmount = undefined;
        RecipeStepsIngredientsArray.push(RecipeStepIngredient);
    });
    var Comment = $('#AddStepComment').val();
    var EstimatedTimeInSeconds = $('#EstimatedStepTime').val() * 60;
    if (EstimatedTimeInSeconds < 0) {
        ShowPopUpModal('Validation', "Estimated time has to be positive");
        return;
    }
    if (Comment.length > 500) {
        ShowPopUpModal('Validation', "Directions can only be 200 characters");
        return;
    }
    if (Comment === null || Comment === "null" || Comment.length <= 0)
        Comment === undefined;
    EditingRecipeStep.RecipeId = RecipeId;
    EditingRecipeStep.StepNumber = StepNumber;
    EditingRecipeStep.Comment = Comment;
    EditingRecipeStep.EstimatedTimeInSeconds = EstimatedTimeInSeconds;
    EditingRecipeStep.RecipeStepsIngredients = RecipeStepsIngredientsArray;
    var Data = JSON.stringify(EditingRecipeStep);
    $.ajax({
        url: Config.AjaxUrls.AjaxUpdateRecipeStep,
        type: "POST",
        cache: false,
        contentType: 'application/json; charset=utf-8',
        data: Data,
        success: function (Response) {
            $('#AddStepModal').modal('hide');
            PopulateStepsTable(RecipeId);
        },
        error: function (xhr, ajaxOptions, error) {
            ShowPopUpModal("Error", "Oops something bad happened.. " + xhr.status + ' ' + xhr.responseText);
        }
    });
}
function UpdateServingSize() {
    clearInterval(ServingSizeSaveTimer);
    var ServingSizeInputImage = $('#ServingSizeInputImage');
    var ServingSizeInput = $("#ServingSizeInput");
    if (EditingRecipe.CreationStep > 0) {
        if (ServingSizeInput.val() === undefined || ServingSizeInput.val() === null || ServingSizeInput.val() < 1) {
            ShowPopUpModal('Validation', "ServingSize cant be less than 1");
            ServingSizeInputImage.attr("src", Config.Icons.ErrorIconUrl);
            return;
        }
    }
    ServingSizeInputImage.attr("src", Config.Icons.LoadingIconUrl);
    IsNextButtonBusy = true;
    $.ajax({
        url: Config.AjaxUrls.AjaxUpdateServingSize,
        type: "GET",
        cache: false,
        data: { recipeId: EditingRecipe.Id, servingSize: ServingSizeInput.val() },
        success: function (servingSize) {
            ServingSizeInput.val(servingSize);
            ServingSizeSaveTimer = null;
            servingSize 
            if (EditingRecipe.CreationStep < 2)
                EditingRecipe.CreationStep = 2;
            $('#DietaryRestrictionsSliderDiv').fadeIn('slow');
            $('#dietary-restrictions-div').fadeIn('slow');
            ServingSizeInputImage.attr("src", "/Content/Images/Icons/Checkmark.png");
            IsNextButtonBusy = false;
        },
        error: function (xhr, ajaxOptions, error) {
            ShowPopUpModal("Error", "Oops something bad happened.. " + xhr.status + ' ' + xhr.responseText);
            ServingSizeInputImage.attr("src", "/Content/Images/Icons/Error.png");
            IsNextButtonBusy = false;
        }
    });
}
function UpdateViewableType(viewableTypeName)
{
    $.ajax({
        url: Config.AjaxUrls.AjaxUpdateViewableType,
        type: "GET",
        cache: false,
        data: { recipeId: EditingRecipe.Id, viewableTypeName: viewableTypeName },
        success: function (ViewableType) {
            window.location.href = Config.Urls.RecipesIndex;
        },
        error: function (xhr, ajaxOptions, error) {
            ShowPopUpModal("Error", "Oops something bad happened.. " + xhr.status + ' ' + xhr.responseText);
        }
    });
    
}
function UploadImageButtonClicked(Input, imageButton, RecipeId, StepNum, SlotNum) {
    if (imageButton.attr('src').indexOf(Config.Images.AddPhotoImageUrl) >= 0) //Check for default img source
        Input.click();
    else
        ShowRemoveCloudFilePopUp(imageButton, RecipeId, StepNum, SlotNum);
}
function UploadVideoButtonClicked(Input, imageButton, RecipeId) {
    Input.click();
}
function VideoUploadInput_Changed(Input, video, RecipeId) {
    var File = Input.get(0).files[0];
    if (!IsFileTypeAllowed(File, 'Video'))
        return;
    if (File.size > 31457280)//30MB
        ShowPopUpModal('Validation', 'File size is too big');
    $('#pleaseWaitDialog').modal('show');
    var formData = new FormData();
    ajax = new XMLHttpRequest();
    ajax.onreadystatechange = function () {
        if (this.readyState === 4 && this.status === 200) {
            $('#VideoUploadButton').hide();
            $('#pleaseWaitDialog').modal('show');
            var bool = JSON.parse(this.responseText);
            if (bool === -1)
                ShowPopUpModal('Validation', 'File size is too big');
            else {
                SetSrcFromLocalFile(File, video);
                video.fadeIn();
            }
            Input.val('');
            $('#pleaseWaitDialog').modal('hide');
        }
    };
    ajax.upload.addEventListener("progress", ProgressHandler, false);
    ajax.addEventListener("load", CompleteHandler, false);
    if (File !== null) {
        formData.append("UploadedFile", File);
        formData.append("RecipeId", RecipeId);
        ajax.open("POST", Config.AjaxUrls.AjaxUploadRecipeVideo);
        ajax.send(formData);
    }
}