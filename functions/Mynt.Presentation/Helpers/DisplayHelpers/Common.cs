using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using Mynt.Common.Enums.DisplayEnums;
using Mynt.Common.Framework.Logging;

namespace Mynt.Presentation.Helpers.DisplayHelpers
{
    public static partial class DisplayHelper
    {
        public static MvcHtmlString Log()
        {
            var items = LogHelper.Instance.GetEntries();
            var res = "";
            foreach (var item in items)
            {
                var step = BootstrapAlertSteps.Info;
                var message = item.Message;
                if (!String.IsNullOrEmpty(item.Details))
                    message += ". Details: " + item.Details;
                if (!String.IsNullOrEmpty(item.Namespace))
                    message += ". Namespace: " + item.Namespace;

                switch (item.LogLevel)
                {
                    case LogLevel.Debug:
                        step = BootstrapAlertSteps.Info;
                        break;
                    case LogLevel.Info:
                        step = BootstrapAlertSteps.Success;
                        break;
                    case LogLevel.Warn:
                        step = BootstrapAlertSteps.Warning;
                        break;
                    case LogLevel.Error:
                        step = BootstrapAlertSteps.Danger;
                        break;
                    case LogLevel.Fatal:
                        step = BootstrapAlertSteps.Danger;
                        message = "Dieser Fehler beinträchtigt womöglich die Ausführung der Anwendung: " + message;
                        break;
                }
                res += HtmlCreator.ConstructAlert(step, message);
            }

            var div = new TagBuilder("div");
            div.AddCssClass("log-content");
            div.InnerHtml = res;

            return new MvcHtmlString(div.ToString());
        }

        public static MvcHtmlString CheckBoxValue(bool? value)
        {
            if (value.HasValue && value.Value)
                return new MvcHtmlString(" checked=\"checked\" ");
            return new MvcHtmlString("");
        }

        public static string CleanStringCharacters(string str)
        {
            string text = str;
            if (!string.IsNullOrEmpty(text))
            {
                text = RemoveGermanAccent(text);
                text = RemoveSpecialCharacters(text);

                return text;
            }
            return str;
        }

        public static string RemoveGermanAccent(string str)
        {
            var map = new Dictionary<char, string>() {
              { 'ä', "ae" },
              { 'ö', "oe" },
              { 'ü', "ue" },
              { 'Ä', "Ae" },
              { 'Ö', "Oe" },
              { 'Ü', "Ue" },
              { 'ß', "ss" }
            };

            var res = str.Aggregate(new StringBuilder(), (sb, c) =>
            {
                string r;
                if (map.TryGetValue(c, out r))
                    return sb.Append(r);
                else
                    return sb.Append(c);
            }).ToString();
            return res;
        }


        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '-' || c == '_' || char.IsWhiteSpace(c))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static string FixDoubleQuotes(string str)
        {
            str = str.Replace("\"", "");
            return str;
        }
    }
}