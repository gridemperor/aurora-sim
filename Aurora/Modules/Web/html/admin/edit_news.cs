﻿using Aurora.Framework;
using Aurora.Framework.DatabaseInterfaces;
using Aurora.Framework.Servers.HttpServer;
using Aurora.Framework.Servers.HttpServer.Implementation;
using OpenMetaverse;
using System.Collections.Generic;

namespace Aurora.Modules.Web
{
    public class EditNewPage : IWebInterfacePage
    {
        public string[] FilePath
        {
            get
            {
                return new[]
                           {
                               "html/admin/edit_news.html"
                           };
            }
        }

        public bool RequiresAuthentication
        {
            get { return true; }
        }

        public bool RequiresAdminAuthentication
        {
            get { return true; }
        }

        public Dictionary<string, object> Fill(WebInterface webInterface, string filename, OSHttpRequest httpRequest,
                                               OSHttpResponse httpResponse, Dictionary<string, object> requestParameters,
                                               ITranslator translator, out string response)
        {
            response = null;
            var vars = new Dictionary<string, object>();
            IGenericsConnector connector = Framework.Utilities.DataManager.RequestPlugin<IGenericsConnector>();
            GridNewsItem news;
            if (requestParameters.ContainsKey("Submit"))
            {
                string title = requestParameters["NewsTitle"].ToString();
                string text = requestParameters["NewsText"].ToString();
                string id = requestParameters["NewsID"].ToString();
                news = connector.GetGeneric<GridNewsItem>(UUID.Zero, "WebGridNews", id);
                connector.RemoveGeneric(UUID.Zero, "WebGridNews", id);
                GridNewsItem item = new GridNewsItem {Text = text, Time = news.Time, Title = title, ID = int.Parse(id)};
                connector.AddGeneric(UUID.Zero, "WebGridNews", id, item.ToOSD());
                response = "<h3>News item editted successfully, redirecting to main page</h3>" +
                           "<script language=\"javascript\">" +
                           "setTimeout(function() {window.location.href = \"index.html?page=news_manager\";}, 0);" +
                           "</script>";
                return null;
            }


            news = connector.GetGeneric<GridNewsItem>(UUID.Zero, "WebGridNews", httpRequest.Query["newsid"].ToString());
            vars.Add("NewsTitle", news.Title);
            vars.Add("NewsText", news.Text);
            vars.Add("NewsID", news.ID.ToString());

            vars.Add("NewsItemTitle", translator.GetTranslatedString("NewsItemTitle"));
            vars.Add("NewsItemText", translator.GetTranslatedString("NewsItemText"));
            vars.Add("EditNewsText", translator.GetTranslatedString("EditNewsText"));
            vars.Add("Submit", translator.GetTranslatedString("Submit"));
            return vars;
        }

        public bool AttemptFindPage(string filename, ref OSHttpResponse httpResponse, out string text)
        {
            text = "";
            return false;
        }
    }
}