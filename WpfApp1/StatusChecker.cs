using System;
using System.Threading;

namespace SendMail
{
    public class StatusChecker
    {
        public bool Status { get; set; }

        public StatusChecker()
        {
            Status = false;
        }

        public void CheckStatus(Object stateInfo)
        {
            AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;
            if (Status)
            {
                autoEvent.Set();
            }
        }
    }
}