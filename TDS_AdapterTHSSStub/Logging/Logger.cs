using log4net;
using System;

namespace Logging
{
    public class Logger : ILogger
    {
        private static ILog _log = LogManager.GetLogger("AdapterTHSSStubLogger");

        public void Debug(string msg)
        {
            _log.Debug(msg);
        }

        public void Error(Exception ex)
        {
            _log.Error(ex);
        }

        public void Error(string msg, Exception ex)
        {
            _log.Error(msg, ex);
        }

        public void Info(object msg)
        {
            _log.Info(msg);
        }
    }
    
}
