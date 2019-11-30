// Adapted from https://stackoverflow.com/a/2181248

using System;
using System.Collections.Specialized;
using System.Threading;
using System.Windows.Forms;

namespace XorberaxMapEditor.Utilities
{
    internal class ThreadedClipboardUtility
    {
        private bool _containsFileDropList;
        private bool _containsTextResult;
        private StringCollection _getFileDropListResult;
        private string _getTextResult;

        private void ThreadedGetText(object format)
        {
            try
            {
                if (format == null)
                    _getTextResult = Clipboard.GetText();
                else
                    _getTextResult = Clipboard.GetText((TextDataFormat)format);
            }
            catch (Exception)
            {
                _getTextResult = string.Empty;
            }
        }

        public string GetText()
        {
            var instance = new ThreadedClipboardUtility();
            var staThread = new Thread(instance.ThreadedGetText);
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
            return instance._getTextResult;
        }

        public string GetText(TextDataFormat format)
        {
            var instance = new ThreadedClipboardUtility();
            var staThread = new Thread(instance.ThreadedGetText);
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start(format);
            staThread.Join();
            return instance._getTextResult;
        }

        private void ThreadedContainsText(object format)
        {
            try
            {
                if (format == null)
                    _containsTextResult = Clipboard.ContainsText();
                else
                    _containsTextResult = Clipboard.ContainsText((TextDataFormat)format);
            }
            catch (Exception)
            {
                _containsTextResult = false;
            }
        }

        public bool ContainsText()
        {
            var instance = new ThreadedClipboardUtility();
            var staThread = new Thread(instance.ThreadedContainsFileDropList);
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
            return instance._containsTextResult;
        }

        public bool ContainsText(object format)
        {
            var instance = new ThreadedClipboardUtility();
            var staThread = new Thread(instance.ThreadedContainsFileDropList);
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start(format);
            staThread.Join();
            return instance._containsTextResult;
        }

        private void ThreadedContainsFileDropList(object format)
        {
            try
            {
                _containsFileDropList = Clipboard.ContainsFileDropList();
            }
            catch (Exception)
            {
                _containsFileDropList = false;
            }
        }

        public bool ContainsFileDropList()
        {
            var instance = new ThreadedClipboardUtility();
            var staThread = new Thread(instance.ThreadedContainsFileDropList);
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
            return instance._containsFileDropList;
        }

        private void ThreadedGetFileDropList()
        {
            try
            {
                _getFileDropListResult = Clipboard.GetFileDropList();
            }
            catch (Exception)
            {
                _getFileDropListResult = null;
            }
        }

        public StringCollection GetFileDropList()
        {
            var instance = new ThreadedClipboardUtility();
            var staThread = new Thread(instance.ThreadedGetFileDropList);
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
            return instance._getFileDropListResult;
        }
    }
}
