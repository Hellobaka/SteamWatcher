using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using {PluginID}.Sdk.Cqp.EventArgs;
using {PluginID}.PublicInfos;

namespace {PluginID}.Code
{
    public class Event_PrivateMessage
    {
        public static FunctionResult PrivateMessage(CQPrivateMessageEventArgs e)
        {
            FunctionResult result = new FunctionResult()
            {
                SendFlag = false
            };
            try
            {
                foreach (var item in MainSave.Instances.OrderByDescending(x => x.Priority)
                    .Where(item => item.CanExecute(e.Message.Text)))
                {
                    var r = item.Execute(e);
                    if (r.Result && r.SendFlag)
                    {
                        return r;
                    }
                }
                return result;
            }
            catch (Exception exc)
            {
                MainSave.CQLog.Info("异常抛出", exc.Message + exc.StackTrace);
                return result;
            }
        }
    }
}
