using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using static System.Decimal;

namespace Mynt.AspNetCore.Host.Helpers
{
    public static class FormatHelper
    {
        /// <summary>
        /// Check color for the given value green or red
        /// </summary>
        /// <param name="value">Has to be a decimal to decide if positive or not</param>
        /// <param name="decimalPlaces">If you want to round the decimal number on several digits after dot</param>
        /// <param name="signInEnd">Make something in the end of the value like % or blabla..</param>
        /// <param name="addClass">Add some classes</param>
        /// <returns></returns>
        public static IHtmlContent DecideColorFromValue(decimal value, int? decimalPlaces, string signInEnd, string addClass)
        {
            // create temp value
            var val = value;

            // Round value
            if (decimalPlaces != null && decimalPlaces > 0)
                val = Round(val, decimalPlaces.Value);

            // Create tag builder and fill in styles and value
            var coloredValue = new TagBuilder("div");
            if (val > 0)
                @coloredValue.AddCssClass("green");
            if (val < 0)
                @coloredValue.AddCssClass("red");
            if (val == 0)
                @coloredValue.AddCssClass("yellow");

            @coloredValue.AddCssClass(addClass);
            @coloredValue.InnerHtml.Append(val.ToString(CultureInfo.InvariantCulture) + signInEnd);

            return coloredValue;
        }
    }
}
    