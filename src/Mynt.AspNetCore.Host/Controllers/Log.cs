using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.AspNetCore.Host.Controllers
{
    public class Log
    {
        /// <summary>
        /// Reading log file
        /// </summary>
        /// <param name="filename">filename as string</param>
        /// <param name="lines">Get number of lines which should be red</param>
        /// <returns></returns>
        public static string[] ReadTail(string filename, int lines = 10, bool descending = true)
        {
            using (var fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                TextReader tr = new StreamReader(fs);
                return Tail(tr, lines, true);
            }
        }
        

        ///<summary>Returns the end of a text reader.</summary>
        ///<param name="reader">The reader to read from.</param>
        ///<param name="lineCount">The number of lines to return.</param>
        ///<returns>The last lneCount lines from the reader.</returns>
        public static string[] Tail(TextReader reader, int lineCount, bool descending = true)
        {
            var buffer = new List<string>(lineCount);
            string line;
            for (int i = 0; i < lineCount; i++)
            {
                line = reader.ReadLine();
                if (line == null) return buffer.ToArray();
                buffer.Add(line);
            }

            //The index of the last line read from the buffer.  Everything > this index was read earlier than everything <= this indes
            int lastLine = lineCount - 1;           

            while (null != (line = reader.ReadLine()))
            {
                lastLine++;
                if (lastLine == lineCount) lastLine = 0;
                buffer[lastLine] = line;
            }

            if (lastLine == lineCount - 1) return buffer.ToArray();
            var retVal = new string[lineCount];
            buffer.CopyTo(lastLine + 1, retVal, 0, lineCount - lastLine - 1);
            buffer.CopyTo(0, retVal, lineCount - lastLine - 1, lastLine + 1);
            if (descending)
                return retVal.OrderByDescending(l => l).ToArray();
            return retVal;
        }
    }
}
