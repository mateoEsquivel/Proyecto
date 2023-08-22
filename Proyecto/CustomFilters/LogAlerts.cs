using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Proyecto.CustomFilters
{
    public class LogAlerts
    {
       public void LogAlert(string message)
        {
            string vPath = HttpContext.Current.Server.MapPath("~/App_Data/log_Alert.txt");

            using (StreamWriter writer = new StreamWriter(vPath,true))
            {
                writer.WriteLine(message);
            }
        }

        public void LogError(string message)
        {
            string vPath = HttpContext.Current.Server.MapPath("~/App_Data/log_Error.txt");

            using (StreamWriter writer = new StreamWriter(vPath, true))
            {
                writer.WriteLine(message);
            }
        }
    }
}