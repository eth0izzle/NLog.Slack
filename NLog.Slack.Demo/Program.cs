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
            _logger.Warn("Warning, warning...");

            try
            {
                throw new ApplicationException("I'm the exception to the rule.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "KABOOM!");
            }

            Console.WriteLine("Done - check your Slack channel!");
            Console.ReadLine();
        }

        //// ----------------------------------------------------------------------------------------------------------
    }
}