using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Mynt.Common.Enums.DisplayEnums;

namespace Mynt.Presentation.Helpers
{
    public static class HtmlCreator
    {
        public static MvcHtmlString ConstructAlert(BootstrapAlertSteps step, string message)
        {
            var div = new TagBuilder("div");
            var p = new TagBuilder("p");
            var strong = new TagBuilder("strong");

            div.AddCssClass("alert-" + step.ToString().ToLower());
            strong.SetInnerText(step + ": ");

            p.InnerHtml += strong;
            p.InnerHtml += message;
            div.InnerHtml += p;
            div.AddCssClass("alert");
            div.MergeAttribute("role", "alert");
            return new MvcHtmlString(div.ToString());
        }

        public static MvcHtmlString CheckedIfTrue(bool value)
        {
            if (value)
                return new MvcHtmlString(" checked=\"checked\"");
            return new MvcHtmlString("");
        }

        public static MvcHtmlString Image(this HtmlHelper helper, string url, string altText, object htmlAttributes)
        {
            TagBuilder builder = new TagBuilder("img");
            builder.Attributes.Add("src", url);
            builder.Attributes.Add("alt", altText);
            builder.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            return MvcHtmlString.Create(builder.ToString(TagRenderMode.Normal));
        }
    }
}