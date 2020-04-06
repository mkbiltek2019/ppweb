<%@ WebHandler Language="C#" Class="UploadProfileImg" %>

using System;
using System.Web;
using System.IO;

public class UploadProfileImg : IHttpHandler {

    public void ProcessRequest (HttpContext context) {
        context.Response.ContentType = "text/plain";
        string userId = context.Request.Form["userid"];
        string clientId = context.Request.Form["clientid"];
        if (context.Request.Files.Count > 0) {
            HttpFileCollection files = context.Request.Files;
            for (int i = 0; i < files.Count; i++) {
                HttpPostedFile file = files[i];
                string fname = context.Server.MapPath(string.Format("~/upload/users/{0}/clients/{1}/profileimg/{2}", userId, clientId, file.FileName));
                if (!string.IsNullOrEmpty(file.FileName)) {
                    string folderPath = context.Server.MapPath(string.Format("~/upload/users/{0}/clients/{1}/profileimg", userId, clientId));
                    if (!Directory.Exists(folderPath)) {
                        Directory.CreateDirectory(folderPath);
                    }
                    file.SaveAs(fname);
                    context.Response.Write(string.Format("{0}?v={1}", file.FileName, DateTime.Now.Ticks));
                } else {
                    context.Response.Write("please choose a file to upload");
                }
            }
        }
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}