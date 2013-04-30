﻿using Aurora.Framework;
using Aurora.Framework.DatabaseInterfaces;
using Aurora.Framework.Modules;
using Aurora.Framework.Servers.HttpServer;
using Aurora.Framework.Servers.HttpServer.Implementation;
using Aurora.Framework.Services;
using Aurora.Framework.Services.ClassHelpers.Profile;
using OpenMetaverse;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Aurora.Modules.Web
{
    public class AgentGroupsPage : IWebInterfacePage
    {
        public string[] FilePath
        {
            get
            {
                return new[]
                           {
                               "html/webprofile/groups.html"
                           };
            }
        }

        public bool RequiresAuthentication
        {
            get { return false; }
        }

        public bool RequiresAdminAuthentication
        {
            get { return false; }
        }

        public Dictionary<string, object> Fill(WebInterface webInterface, string filename, OSHttpRequest httpRequest,
                                               OSHttpResponse httpResponse, Dictionary<string, object> requestParameters,
                                               ITranslator translator, out string response)
        {
            response = null;
            var vars = new Dictionary<string, object>();

            string username = filename.Split('/').LastOrDefault();
            UserAccount account = null;
            if (httpRequest.Query.ContainsKey("userid"))
            {
                string userid = httpRequest.Query["userid"].ToString();

                account = webInterface.Registry.RequestModuleInterface<IUserAccountService>().
                                       GetUserAccount(null, UUID.Parse(userid));
            }
            else if (httpRequest.Query.ContainsKey("name") || username.Contains('.'))
            {
                string name = httpRequest.Query.ContainsKey("name") ? httpRequest.Query["name"].ToString() : username;
                name = name.Replace('.', ' ');
                account = webInterface.Registry.RequestModuleInterface<IUserAccountService>().
                                       GetUserAccount(null, name);
            }
            else
            {
                username = username.Replace("%20", " ");
                account = webInterface.Registry.RequestModuleInterface<IUserAccountService>().
                                       GetUserAccount(null, username);
            }

            if (account == null)
                return vars;

            vars.Add("UserName", account.Name);

            IUserProfileInfo profile = Framework.Utilities.DataManager.RequestPlugin<IProfileConnector>().
                                              GetUserProfile(account.PrincipalID);
            vars.Add("UserType", profile.MembershipGroup == "" ? "Resident" : profile.MembershipGroup);
            IWebHttpTextureService webhttpService =
                webInterface.Registry.RequestModuleInterface<IWebHttpTextureService>();
            if (profile != null)
            {
                if (profile.Partner != UUID.Zero)
                {
                    account = webInterface.Registry.RequestModuleInterface<IUserAccountService>().
                                           GetUserAccount(null, profile.Partner);
                    vars.Add("UserPartner", account.Name);
                }
                else
                    vars.Add("UserPartner", "No partner");
                vars.Add("UserAboutMe", profile.AboutText == "" ? "Nothing here" : profile.AboutText);
                string url = "../images/icons/no_picture.jpg";
                if (webhttpService != null && profile.Image != UUID.Zero)
                    url = webhttpService.GetTextureURL(profile.Image);
                vars.Add("UserPictureURL", url);
            }

            vars.Add("UsersGroupsText", translator.GetTranslatedString("UsersGroupsText"));

            IGroupsServiceConnector groupsConnector =
                Framework.Utilities.DataManager.RequestPlugin<IGroupsServiceConnector>();
            if (groupsConnector != null)
            {
                List<Dictionary<string, object>> groups = new List<Dictionary<string, object>>();
                foreach (var grp in groupsConnector.GetAgentGroupMemberships(account.PrincipalID, account.PrincipalID))
                {
                    var grpData = groupsConnector.GetGroupProfile(account.PrincipalID, grp.GroupID);
                    string url = "../images/icons/no_picture.jpg";
                    if (webhttpService != null && grpData.InsigniaID != UUID.Zero)
                        url = webhttpService.GetTextureURL(grpData.InsigniaID);
                    groups.Add(new Dictionary<string, object>
                                   {
                                       {"GroupPictureURL", url},
                                       {"GroupName", grp.GroupName}
                                   });
                }
                vars.Add("Groups", groups);
                vars.Add("GroupsJoined", groups.Count);
            }

            return vars;
        }

        public bool AttemptFindPage(string filename, ref OSHttpResponse httpResponse, out string text)
        {
            httpResponse.ContentType = "text/html";
            text = File.ReadAllText("html/webprofile/index.html");
            return true;
        }
    }
}