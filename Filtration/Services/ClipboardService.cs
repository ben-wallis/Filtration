using System;
using System.Threading;
using System.Windows;
using NLog;

namespace Filtration.Services
{
    internal interface IClipboardService
    {
        void SetClipboardText(string inputText);
        string GetClipboardText();
    }

    internal class ClipboardService : IClipboardService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void SetClipboardText(string inputText)
        {
            for (var i = 0; i < 10; i++)
            {
                try
                {
                    Clipboard.SetText(inputText);
                    return;
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }

                Thread.Sleep(10);
            }

            throw new Exception("Failed to copy to clipboard");
        }

        public string GetClipboardText()
        {
            return Clipboard.GetText();
        }
    }
}
