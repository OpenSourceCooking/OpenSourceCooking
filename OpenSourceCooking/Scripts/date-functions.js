var months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
function ConvertJSONDateToString(DateString)
{
    var D = new Date();
    D.setTime(DateString.replace("/Date(", "").replace(")/", ""));
    return D.getDate() + ' ' + months[D.getMonth()] + ' ' + D.getFullYear();
}