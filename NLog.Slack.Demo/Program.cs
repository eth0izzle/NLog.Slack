using System;

namespace NLog.Slack.Demo
{
    public static class Program
    {
        //// ----------------------------------------------------------------------------------------------------------

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        //// ----------------------------------------------------------------------------------------------------------

        public static void Main(string[] args)
        {
            _logger.Info("This is a piece of useful information.");
            _logger.Debug("I be debuggin' all night long");
            _logger.Warn("Warning, warning... I'm UTF8 ready : 你好世界.");

            try
            {
                CreateBigStackTrace(20);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "KABOOM!");
            }

            Console.WriteLine("Done - check your Slack channel!");
            Console.ReadLine();
        }

        //// ----------------------------------------------------------------------------------------------------------

        private static void CreateBigStackTrace(int lines)
        {
            if (lines < 1)
            {
                throw new ApplicationException("I'm the exception to the rule.");
            }
            else
            {
                CreateBigStackTrace(lines - 1);
            }
        }

        //// ----------------------------------------------------------------------------------------------------------
    }
}