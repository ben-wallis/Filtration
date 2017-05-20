// Taken from JonSkeet's StackOverflow answer here: http://stackoverflow.com/questions/286533/filestream-streamreader-problem-in-c-sharp/286598#286598
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Filtration.Common.Utilities
{

    public sealed class LineReader : IEnumerable<string>
    {
        /// <summary>
        /// Means of creating a TextReader to read from.
        /// </summary>
        private readonly Func<TextReader> _dataSource;

        /// <summary>
        /// Creates a LineReader from a stream source. The delegate is only
        /// called when the enumerator is fetched. UTF-8 is used to decode
        /// the stream into text.
        /// </summary>
        /// <param name="streamSource">Data source</param>
        [DebuggerStepThrough]
        public LineReader(Func<Stream> streamSource)
            : this(streamSource, Encoding.UTF8)
        {
        }

        /// <summary>
        /// Creates a LineReader from a stream source. The delegate is only
        /// called when the enumerator is fetched.
        /// </summary>
        /// <param name="streamSource">Data source</param>
        /// <param name="encoding">Encoding to use to decode the stream
        /// into text</param>
        [DebuggerStepThrough]
        public LineReader(Func<Stream> streamSource, Encoding encoding)
            : this(() => new StreamReader(streamSource(), encoding))
        {
        }

        /// <summary>
        /// Creates a LineReader from a filename. The file is only opened
        /// (or even checked for existence) when the enumerator is fetched.
        /// UTF8 is used to decode the file into text.
        /// </summary>
        /// <param name="filename">File to read from</param>
        [DebuggerStepThrough]
        public LineReader(string filename)
            : this(filename, Encoding.UTF8)
        {
        }

        /// <summary>
        /// Creates a LineReader from a filename. The file is only opened
        /// (or even checked for existence) when the enumerator is fetched.
        /// </summary>
        /// <param name="filename">File to read from</param>
        /// <param name="encoding">Encoding to use to decode the file
        /// into text</param>
        [DebuggerStepThrough]
        public LineReader(string filename, Encoding encoding)
            : this(() => new StreamReader(filename, encoding))
        {
        }

        /// <summary>
        /// Creates a LineReader from a TextReader source. The delegate
        /// is only called when the enumerator is fetched
        /// </summary>
        /// <param name="dataSource">Data source</param>
        [DebuggerStepThrough]
        public LineReader(Func<TextReader> dataSource)
        {
            _dataSource = dataSource;
        }

        /// <summary>
        /// Enumerates the data source line by line.
        /// </summary>
        [DebuggerStepThrough]
        public IEnumerator<string> GetEnumerator()
        {
            using (TextReader reader = _dataSource())
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        /// <summary>
        /// Enumerates the data source line by line.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
