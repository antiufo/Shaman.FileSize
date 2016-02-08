using System;
using System.Globalization;

namespace Shaman.Types
{
    /// <summary>
    /// Represents an amount of binary data.
    /// </summary>
    public struct FileSize
    {

        private long bytes;

        /// <summary>
        /// The number of bytes.
        /// </summary>
        public long Bytes
        {
            get
            {
                return bytes;
            }
        }

        /// <summary>
        /// Returns a file size with the exact amount of specified bytes.
        /// </summary>
        /// <param name="bytes">The number of bytes.</param>
        public FileSize(long bytes)
        {
            this.bytes = bytes;
        }


        /// <summary>
        /// Parses a human-readable size.
        /// </summary>
        /// <param name="value">The string to parse (eg. 29.75 MB)</param>
        /// <param name="decimalDot">Specifies if a point (".") is used to separate decimal digits (comma otherwise).</param>
        /// <returns>The parsed size.</returns>
        public static FileSize Parse(string value, bool decimalDot = true)
        {
            return new FileSize(ApproxBytesFromFormattedText(value, decimalDot));
        }

        /// <summary>
        /// Returns a human-readable representation of the size.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return FormatSize(bytes);
        }

        public string ToString(string numericFormat = null, BytesSizeFormat bytesFormat = BytesSizeFormat.JEDEC)
        {
            return FormatSize(bytes, numericFormat, bytesFormat);

        }

        public static readonly FileSize Zero;

        public static bool operator ==(FileSize a, FileSize b)
        {
            return a.bytes == b.bytes;
        }

        public static bool operator !=(FileSize a, FileSize b)
        {
            return a.bytes != b.bytes;
        }


        public static bool operator <(FileSize a, FileSize b)
        {
            return a.bytes < b.bytes;
        }
        public static bool operator <=(FileSize a, FileSize b)
        {
            return a.bytes <= b.bytes;
        }
        public static bool operator >(FileSize a, FileSize b)
        {
            return a.bytes > b.bytes;
        }
        public static bool operator >=(FileSize a, FileSize b)
        {
            return a.bytes >= b.bytes;
        }



        public override bool Equals(object obj)
        {
            if (obj is FileSize)
            {
                var other = (FileSize)obj;
                return other.bytes == this.bytes;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.bytes.GetHashCode();
        }







        private static string[] Units = new string[] { "K", "M", "G", "T", "P", "E", "Z", "Y" };

        //  <summary>
        //  Returns a formatted size in bytes or its multiples.
        //  </summary>
        //  <date>Apr 15, 2009</date>
        private static string FormatSize(long bytes, string numericFormat = null, BytesSizeFormat bytesFormat = BytesSizeFormat.JEDEC)
        {
            if ((bytes < 0))
            {
                return ("-" + FormatSize((bytes * -1), numericFormat, bytesFormat));
            }

            if (numericFormat == null) numericFormat = "0.0";
            if ((bytes == 1))
            {
                return "1 byte";
            }
            var mult = bytesFormat == BytesSizeFormat.SI ? 1000 : 1024;

            if ((bytes < mult))
            {
                return bytes.ToString() + " bytes";
            }
            long limit = 1;
            int unit = -1;
            while (limit * mult <= bytes)
            {
                limit *= mult;
                unit++;
            }

            return Math.Round((double)bytes / limit, 2).ToString(numericFormat) +
                " " +
                (unit == 0 && bytesFormat == BytesSizeFormat.SI ? "k" : Units[unit]) +
               (bytesFormat == BytesSizeFormat.IEC ? "i" : "B");

        }

        //  <summary>
        //  Determines the bytes notation to use. Default is JEDEC (1KB = 1024bytes)
        //  </summary>
        //  <remarks></remarks>
        //  <date>Apr 15, 2009</date>
        public enum BytesSizeFormat
        {
            // <summary>
            // 1 KB = 1000 bytes
            // </summary>
            SI = 1,

            // <summary>
            // 1 KB = 1024 bytes
            // </summary>
            JEDEC = 2,

            // <summary>
            // 1 KiB = 1024 bytes
            // </summary>
            IEC = 3
        }

        //  <summary>
        //  Returns a formatted speed of bytes transfer, eg: 45 KB / sec
        //  </summary>
        //  <param name="bytes">Bytes transmitted in the timespan</param>
        //  <param name="time">How much time the bytes was transmitted in</param>
        //  <date>Apr 15, 2009</date>
        public static string FormatSpeed(long bytes, TimeSpan time)
        {
#if SALTARELLE
            var result = (long)(bytes / time.TotalSeconds);
            if (!double.IsFinite(result)) return string.Empty;
            return FormatSpeed(result);
#else
            try
            {
                checked
                {
                    return FormatSpeed((long)(bytes / time.TotalSeconds));
                }
            }
            catch (OverflowException)
            {
                return String.Empty;
            }
#endif
        }

        //  <date>Apr 25, 2009</date>
        public static TimeSpan CalculateRemainingTime(long processedBytes, long totalBytes, TimeSpan elapsedTime)
        {
            checked
            {
                return new TimeSpan(totalBytes * (elapsedTime.Ticks / processedBytes) - elapsedTime.Ticks);
            }
        }

        //  <date>Apr 25, 2009</date>
        public static string FormatRemainingTime(long processedBytes, long totalBytes, TimeSpan elapsedTime)
        {
            TimeSpan time;
            try
            {
                time = CalculateRemainingTime(processedBytes, totalBytes, elapsedTime);
            }
            catch (Exception)
            {
                return "(unknown)";
            }
            return Format(time);
        }

        //  <summary>
        //  Returns a formatted speed of bytes transfer, eg: 45 KB / sec
        //  </summary>
        //  <param name="bytes">Bytes transmitted in 1 second</param>
        //  <date>Apr 15, 2009</date>
        public static string FormatSpeed(long bytesPerSecond)
        {

            return (FormatSize(bytesPerSecond, "0.0") + " / sec");

        }


        // <date>Apr 25, 2009</date>
        private static string Format(TimeSpan time)
        {
            var totalYears = (int)Math.Floor(time.TotalDays / 365);
            if (totalYears >= 2) return totalYears + (totalYears == 1 ? " year" : " years");

            var totalMonths = (int)Math.Floor(time.TotalDays / 30);
            if (totalMonths >= 2) return totalMonths + (totalMonths == 1 ? " month" : " months");

            var totalDays = (int)Math.Floor(time.TotalDays);
            if (totalDays >= 2) return totalDays + (totalDays == 1 ? " day" : " days");

            var totalHours = (int)Math.Floor(time.TotalHours);
            if (totalHours >= 2) return totalHours + (totalHours == 1 ? " hour" : " hours");

            var totalMinutes = (int)Math.Floor(time.TotalMinutes);
            if (totalMinutes >= 2) return totalMinutes + (totalMinutes == 1 ? " minute" : " minutes");

            var totalSeconds = (int)Math.Floor(time.TotalSeconds);
            return totalSeconds + (totalSeconds == 1 ? " second" : " seconds");

        }



        // <date>Sep 1, 2009</date>
        //  <param name="Text">The size expressed in JEDEC (12 KB = 12*1024 bytes)</param>
        //  <param name="DecimalDot">True: 123,456,789.45; False: 123.456.789,45</param>

        private static long ApproxBytesFromFormattedText(string text, bool decimalDot = true)
        {
            var data = text.Replace(decimalDot ? "," : ".", string.Empty).Replace(',', '.');
            var p = 'b';
            var pos = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (char.IsLetter(data[i]))
                {
                    p = char.ToUpper(data[i]);
                    pos = i;
                    break;
                }
            }

            var m = double.Parse(pos == 0 ? data : data.Substring(0, pos), CultureInfo.InvariantCulture);
            if (p != 'b')
            {
                m *= Math.Pow(1024, 1 + Array.IndexOf(Units, p.ToString()));
            }
            return (long)m;
        }













    }
}
