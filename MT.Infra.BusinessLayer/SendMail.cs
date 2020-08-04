using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MT.Infra.Common;
using Dapper;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace MT.Infra.BusinessLayer
{
    public class SendMail
    {
        DapperRepository dao = null;
        public SendMail()
        {

            dao = new DapperRepository();

        }

        public class MailSendQueue
        {
            public int ID { get; set; }
            public string toAddress { get; set; }
            public string mailBody { get; set; }
            public string subject { get; set; }
        }

        public IEnumerable<MailSendQueue> GetMailQueues()
        {
            string storedProc = "sp_GetQueueMail";

            return dao.GetItems<MailSendQueue>(System.Data.CommandType.StoredProcedure, sql: storedProc);
        }

    }
    
}
