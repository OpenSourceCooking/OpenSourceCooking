//Global Variables - These should be moved to the JavascriptConfig.cshtml
var Colors = /*Pastels*/["#E3FBE9", "#F3F8F4", "#F1FEED", "#E7FFDF", "#F2FFEA", "#FFFFE3", "#FCFCE9", "#EEEEFF", "#ECF4FF", "#F9FDFF", "#E6FCFF", "#F2FFFE", "#CFFEF0", "#EAFFEF", "#FFECFF", "#F4D2F4", "#F9EEFF", "#F5EEFD", "#EFEDFC", "#EAF1FB", "#DBF0F7", "#FFECEC", "#FFEEFB", "#FFECF5", "#FFEEFD", "#FDF2FF", "#FAECFF", "#F1ECFF" /*,Bold&Bright "#66ffff", "#6666ff", "#ff66ff", "#ff6666", "#ffff66", "#66ff66", "#ffb366", "#cccccc"*/];
var CurrentRandomColor = null;
$(document).ready(function () {
    $('.UploadPhotoButton').attr('src', Config.Images.AddPhotoImageUrl);
    $('.UploadVideoButton').attr('src', Config.Images.AddVideoImageUrl);
    RefreshZoomImages();
});
function GetEstimatedTimeString(EstimatedTimeInSeconds) {
    EstimatedTimeInSeconds = EstimatedTimeInSeconds / 60;//Convert Sec to Min
    if (EstimatedTimeInSeconds > 59)
        return parseFloat((EstimatedTimeInSeconds / 60).toFixed(2)).toString() + " Hr(s)";
    else
        return parseFloat(EstimatedTimeInSeconds.toFixed(2)).toString() + " Min(s)";
}
function GetIngredientAmountString(recipeStepsIngredientsDataTransferObject) {
    var IngredientAmountString = recipeStepsIngredientsDataTransferObject.Amount + ' ';
    //If ToAmount
    if (recipeStepsIngredientsDataTransferObject.ToAmount !== null && recipeStepsIngredientsDataTransferObject.ToAmount !== undefined && recipeStepsIngredientsDataTransferObject.ToAmount.length > 0)
        IngredientAmountString = IngredientAmountString + 'to ' + recipeStepsIngredientsDataTransferObject.ToAmount + ' ';
    switch (recipeStepsIngredientsDataTransferObject.MeasurementUnitName) {
        case "Count":
            break;
        default:
            IngredientAmountString = IngredientAmountString + recipeStepsIngredientsDataTransferObject.MeasurementUnitName + ' of ';
            break;
    }
    IngredientAmountString = IngredientAmountString + recipeStepsIngredientsDataTransferObject.IngredientName;
    return IngredientAmountString;
}
function GetRandomColor()
{
    var RandomColor = Colors[Math.floor(Math.random() * Colors.length)];
    while (CurrentRandomColor === RandomColor) //Prevent same colors next to ea other                    
        RandomColor = Colors[Math.floor(Math.random() * Colors.length)];
    CurrentRandomColor = RandomColor;  
    return CurrentRandomColor;
}
function IsFileTypeAllowed(file, fileType) {
    //These IsFileTypes allowed functions are copied in C# for the server to validate with too
    //This should come from DB eventually
    if (file === null)
        return false;
    ExtensionsArray = null;
    switch (fileType) {
        case 'Image':
            {
                ExtensionsArray = new Array("PNG", "JPG", "JPEG");
                break;
            }
        case 'Video':
            {
                ExtensionsArray = new Array("MP4");
                break;
            }
    }
    var FileName = file.name.toUpperCase();
    var extension = FileName.substr(FileName.lastIndexOf(".") + 1);
    if (ExtensionsArray.indexOf(extension) > -1)
        return true;
    ShowPopUpModal('Validation', 'Bummer... We cant handle the ' + extension + ' image extension.  We currently only support JPEG, JPG, and PNG for images');
    return false;
}
function RefreshZoomImages()
{
    $('.zoomImage').hover(function () { $(this).addClass('imageZoomed'); }, function () { $(this).removeClass('imageZoomed'); });
}
function SetSrcFromLocalFile(file, HTMLElement)
{
    var URL = window.URL.createObjectURL(file);
    HTMLElement.attr('src', URL);
}
function ShowDietaryRestrictionsSymbolLegend(Table) {
    Table.empty();
    $.ajax({
        url: Config.AjaxUrls.AjaxGetDietaryRestrictions,
        type: "GET",
        cache: false,
        data: {},
        success: function (DietaryRestrictionDataTransferObjects) {
            $.each(DietaryRestrictionDataTransferObjects, function (i, DietaryRestrictionDataTransferObject) {
                Table.append('<tr><td>' + DietaryRestrictionDataTransferObject.Name + '</td><td><img class="img-fluid" style="max-width:100px;max-height:100px;" src="' + DietaryRestrictionDataTransferObject.IconUrl + '"></td></tr>');
            });
        },
        error: function (xhr, ajaxOptions, error) {
            ShowPopUpModal("Error", "Oops something bad happened.. " + xhr.status + ' ' + xhr.responseText);
            return;
        }
    });

}
function ShowPopUpModal(ModalType, Message) {
    switch (ModalType) {
        case "PopUp":
            $('#PopUpModal').modal('show');
            $('#PopUpModalMessage').text(Message);
            break;
        case "Validation":
            $('#PopUpModal').modal('show');
            $('#PopUpModalMessage').text(Message);
            $('#PopUpModalMessage').css('color', 'orangered');
            $('#PopUpModalCloseButton').text("Ugh.. I'll fix it");
            break;
        case "Error":
            $('#PopUpModal').modal('show');
            $('#PopUpModalMessage').text(Message);
            $('#PopUpModalMessage').css('color', 'red');
            break;
    }
}