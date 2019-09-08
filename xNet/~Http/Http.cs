using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using Microsoft.Win32;
using System.Net.Security;

namespace Better_xNet
{
    /// <summary>
    /// Представляет статический класс, предназначенный для помощи в работе с HTTP-протоколом.
    /// </summary>
    public static class Http
    {
        #region Константы (открытые)

        /// <summary>
        /// Обозначает новую строку в HTTP-протоколе.
        /// </summary>
        public const string NewLine = "\r\n";

        /// <summary>
        /// Метод делегата, который принимает все сертификаты SSL.
        /// </summary>
        public static readonly RemoteCertificateValidationCallback AcceptAllCertificationsCallback;

        #endregion


        #region Статические поля (внутренние)

        internal static readonly Dictionary<HttpHeader, string> Headers = new Dictionary<HttpHeader, string>()
        {
            { HttpHeader.Accept, "Accept" },
            { HttpHeader.AcceptCharset, "Accept-Charset" },
            { HttpHeader.AcceptLanguage, "Accept-Language" },
            { HttpHeader.AcceptDatetime, "Accept-Datetime" },
            { HttpHeader.CacheControl, "Cache-Control" },
            { HttpHeader.ContentType, "Content-Type" },
            { HttpHeader.Date, "Date" },
            { HttpHeader.Expect, "Expect" },
            { HttpHeader.From, "From" },
            { HttpHeader.IfMatch, "If-Match" },
            { HttpHeader.IfModifiedSince, "If-Modified-Since" },
            { HttpHeader.IfNoneMatch, "If-None-Match" },
            { HttpHeader.IfRange, "If-Range" },
            { HttpHeader.IfUnmodifiedSince, "If-Unmodified-Since" },
            { HttpHeader.MaxForwards, "Max-Forwards" },
            { HttpHeader.Pragma, "Pragma" },
            { HttpHeader.Range, "Range" },
            { HttpHeader.Referer, "Referer" },
            { HttpHeader.Upgrade, "Upgrade" },
            { HttpHeader.UserAgent, "User-Agent" },
            { HttpHeader.Via, "Via" },
            { HttpHeader.Warning, "Warning" },
            { HttpHeader.DNT, "DNT" },
            { HttpHeader.AccessControlAllowOrigin, "Access-Control-Allow-Origin" },
            { HttpHeader.AcceptRanges, "Accept-Ranges" },
            { HttpHeader.Age, "Age" },
            { HttpHeader.Allow, "Allow" },
            { HttpHeader.ContentEncoding, "Content-Encoding" },
            { HttpHeader.ContentLanguage, "Content-Language" },
            { HttpHeader.ContentLength, "Content-Length" },
            { HttpHeader.ContentLocation, "Content-Location" },
            { HttpHeader.ContentMD5, "Content-MD5" },
            { HttpHeader.ContentDisposition, "Content-Disposition" },
            { HttpHeader.ContentRange, "Content-Range" },
            { HttpHeader.ETag, "ETag" },
            { HttpHeader.Expires, "Expires" },
            { HttpHeader.LastModified, "Last-Modified" },
            { HttpHeader.Link, "Link" },
            { HttpHeader.Location, "Location" },
            { HttpHeader.P3P, "P3P" },
            { HttpHeader.Refresh, "Refresh" },
            { HttpHeader.RetryAfter, "Retry-After" },
            { HttpHeader.Server, "Server" },
            { HttpHeader.TransferEncoding, "Transfer-Encoding" }
        };

        #endregion


        #region Статические поля (закрытые)

        [ThreadStatic] private static Random _rand;
        private static Random Rand
        {
            get
            {
                if (_rand == null)
                    _rand = new Random();
                return _rand;
            }
        }

        #endregion


        static Http()
        {
            AcceptAllCertificationsCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);
        }


        #region Статические методы (открытые)

        /// <summary>
        /// Кодирует строку для надёжной передачи HTTP-серверу.
        /// </summary>
        /// <param name="str">Строка, которая будет закодирована.</param>
        /// <param name="encoding">Кодировка, применяемая для преобразования данных в последовательность байтов. Если значение параметра равно <see langword="null"/>, то будет использовано значение <see cref="System.Text.Encoding.UTF8"/>.</param>
        /// <returns>Закодированная строка.</returns>
        public static string UrlEncode(string str, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            encoding = encoding ?? Encoding.UTF8;

            byte[] bytes = encoding.GetBytes(str);

            int spaceCount = 0;
            int otherCharCount = 0;

            #region Подсчёт символов

            for (int i = 0; i < bytes.Length; i++)
            {
                char c = (char)bytes[i];

                if (c == ' ')
                {
                    ++spaceCount;
                }
                else if (!IsUrlSafeChar(c))
                {
                    ++otherCharCount;
                }
            }

            #endregion

            // Если в строке не присутствуют символы, которые нужно закодировать.
            if ((spaceCount == 0) && (otherCharCount == 0))
            {
                return str;
            }

            int bufferIndex = 0;
            byte[] buffer = new byte[bytes.Length + (otherCharCount * 2)];

            for (int i = 0; i < bytes.Length; i++)
            {
                char c = (char)bytes[i];

                if (IsUrlSafeChar(c))
                {
                    buffer[bufferIndex++] = bytes[i];
                }
                else if (c == ' ')
                {
                    buffer[bufferIndex++] = (byte)'+';
                }
                else
                {
                    buffer[bufferIndex++] = (byte)'%';
                    buffer[bufferIndex++] = (byte)IntToHex((bytes[i] >> 4) & 15);
                    buffer[bufferIndex++] = (byte)IntToHex(bytes[i] & 15);
                }
            }

            return Encoding.ASCII.GetString(buffer);
        }

        /// <summary>
        /// Преобразует параметры в строку запроса.
        /// </summary>
        /// <param name="parameters">Параметры.</param>
        /// <param name="dontEscape">Указывает, нужно ли кодировать значения параметров.</param>
        /// <returns>Строка запроса.</returns>
        /// <exception cref="System.ArgumentNullException">Значение параметра <paramref name="parameters"/> равно <see langword="null"/>.</exception>
        public static string ToQueryString(IEnumerable<KeyValuePair<string, string>> parameters, bool dontEscape)
        {
            #region Проверка параметров

            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            #endregion

            var queryBuilder = new StringBuilder();

            foreach (var param in parameters)
            {
                if (string.IsNullOrEmpty(param.Key))
                {
                    continue;
                }

                queryBuilder.Append(param.Key);
                queryBuilder.Append('=');

                if (dontEscape)
                {
                    queryBuilder.Append(param.Value);
                }
                else
                {
                    queryBuilder.Append(
                        Uri.EscapeDataString(param.Value ?? string.Empty));
                }

                queryBuilder.Append('&');
            }

            if (queryBuilder.Length != 0)
            {
                // Удаляем '&' в конце.
                queryBuilder.Remove(queryBuilder.Length - 1, 1);
            }

            return queryBuilder.ToString();
        }

        /// <summary>
        /// Преобразует параметры в строку POST-запроса.
        /// </summary>
        /// <param name="parameters">Параметры.</param>
        /// <param name="dontEscape">Указывает, нужно ли кодировать значения параметров.</param>
        /// <param name="encoding">Кодировка, применяемая для преобразования параметров запроса. Если значение параметра равно <see langword="null"/>, то будет использовано значение <see cref="System.Text.Encoding.UTF8"/>.</param>
        /// <returns>Строка запроса.</returns>
        /// <exception cref="System.ArgumentNullException">Значение параметра <paramref name="parameters"/> равно <see langword="null"/>.</exception>
        public static string ToPostQueryString(IEnumerable<KeyValuePair<string, string>> parameters, bool dontEscape, Encoding encoding = null)
        {
            #region Проверка параметров

            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            #endregion

            var queryBuilder = new StringBuilder();

            foreach (var param in parameters)
            {
                if (string.IsNullOrEmpty(param.Key))
                {
                    continue;
                }

                queryBuilder.Append(param.Key);
                queryBuilder.Append('=');

                if (dontEscape)
                {
                    queryBuilder.Append(param.Value);
                }
                else
                {
                    queryBuilder.Append(
                        UrlEncode(param.Value ?? string.Empty, encoding));
                }

                queryBuilder.Append('&');
            }

            if (queryBuilder.Length != 0)
            {
                // Удаляем '&' в конце.
                queryBuilder.Remove(queryBuilder.Length - 1, 1);
            }

            return queryBuilder.ToString();
        }

        /// <summary>
        /// Определяет и возвращает MIME-тип на основе расширения файла.
        /// </summary>
        /// <param name="extension">Расширение файла.</param>
        /// <returns>MIME-тип.</returns>
        public static string DetermineMediaType(string extension)
        {
            string mediaType = "application/octet-stream";

            try
            {
                using (var regKey = Registry.ClassesRoot.OpenSubKey(extension))
                {
                    if (regKey != null)
                    {
                        object keyValue = regKey.GetValue("Content Type");

                        if (keyValue != null)
                        {
                            mediaType = keyValue.ToString();
                        }
                    }
                }
            }
            #region Catch's

            catch (IOException) { }
            catch (ObjectDisposedException) { }
            catch (UnauthorizedAccessException) { }
            catch (SecurityException) { }

            #endregion

            return mediaType;
        }

        #region User Agent

        /// <summary>
        /// Генерирует случайный User-Agent от браузера IE.
        /// </summary>
        /// <returns>Случайный User-Agent от браузера IE.</returns>
        public static string IEUserAgent()
        {
            string windowsVersion = RandomWindowsVersion();

            string version = string.Empty;
            string trident = string.Empty;

            #region Генерация случайной версии

            if (windowsVersion.Contains("NT 5.1"))
            {
                version = "9.0";
                trident = "5.0";
            }
            else if (windowsVersion.Contains("NT 6.0"))
            {
                version = "9.0";
                trident = "5.0";
            }
            else
            {
                version = "11.0";
                trident = "7.0";
            }

            #endregion

            return string.Format(
                "Mozilla/5.0 ({0}; WOW64; Trident/{1}; rv:{2}) like Gecko",
                windowsVersion, trident, version);
        }

        /// <summary>
        /// Генерирует случайный User-Agent от браузера Opera.
        /// </summary>
        /// <returns>Случайный User-Agent от браузера Opera.</returns>
        public static string OperaUserAgent()
        {
            string windowsVersion = RandomWindowsVersion();

            string chromeVersion = string.Empty;
            string operaVersion = string.Empty;
            string systemType = string.Empty;

            #region Генерация случайной версии

            if (windowsVersion.Contains("NT 5.1") || windowsVersion.Contains("NT 6.0"))
            {
                chromeVersion = "49.0.2623.112";
                operaVersion = "36.0.2130.80";
                systemType = "WOW64";
            }
            else
            {
                systemType = "Win64; x64";
                switch (Rand.Next(2))
                {
                    case 0:
                        chromeVersion = "76.0.3809.132";
                        operaVersion = "63.0.3368.71";
                        break;

                    case 1:
                        chromeVersion = "76.0.3809.132";
                        operaVersion = "63.0.3368.54789";
                        break;
                }
            }

            #endregion

            return string.Format(
                "Mozilla/5.0 ({0}; {1}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/{2} Safari/537.36 OPR/{3}",
                windowsVersion, systemType, chromeVersion, operaVersion);
        }

        /// <summary>
        /// Генерирует случайный User-Agent от браузера Chrome.
        /// </summary>
        /// <returns>Случайный User-Agent от браузера Chrome.</returns>
        public static string ChromeUserAgent()
        {
            string windowsVersion = RandomWindowsVersion();

            string version = string.Empty;
            string safari = string.Empty;

            #region Генерация случайной версии

            switch (Rand.Next(1))
            {
                case 0:
                    version = "76.0.3809.132";
                    break;
            }

            #endregion

            return string.Format(
                "Mozilla/5.0 ({0}; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/{1} Safari/537.36",
                windowsVersion, version);
        }

        /// <summary>
        /// Генерирует случайный User-Agent от браузера Firefox.
        /// </summary>
        /// <returns>Случайный User-Agent от браузера Firefox.</returns>
        public static string FirefoxUserAgent()
        {
            string windowsVersion = RandomWindowsVersion();

            string gecko = string.Empty;
            string version = string.Empty;

            #region Генерация случайной версии

            switch (Rand.Next(1))
            {
                case 0:
                    version = "69.0";
                    gecko = "20100101";
                    break;
            }

            #endregion

            return string.Format(
                "Mozilla/5.0 ({0}; Win64; x64; rv:{1}) Gecko/{2} Firefox/{1}",
                windowsVersion, version, gecko);
        }

        public static string EdgeUserAgent()
        {
            string windowsVersion = RandomWindowsVersion();

            string edgeVersion = string.Empty;
            string chromeVersion = string.Empty;

            #region Генерация случайной версии

            switch (Rand.Next(1))
            {
                case 0:
                    chromeVersion = "70.0.3538.102";
                    edgeVersion = "18.18362";
                    break;
            }

            #endregion

            return string.Format(
                "Mozilla/5.0 ({0}; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/{1} Safari/537.36 Edge/{2}",
                windowsVersion, chromeVersion, edgeVersion);
        }

        #endregion

        #endregion


        #region Статические методы (закрытые)

        private static bool AcceptAllCertifications(object sender,
            System.Security.Cryptography.X509Certificates.X509Certificate certification,
            System.Security.Cryptography.X509Certificates.X509Chain chain,
            System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private static bool IsUrlSafeChar(char c)
        {
            if ((((c >= 'a') && (c <= 'z')) ||
                ((c >= 'A') && (c <= 'Z'))) ||
                ((c >= '0') && (c <= '9')))
            {
                return true;
            }

            switch (c)
            {
                case '(':
                case ')':
                case '*':
                case '-':
                case '.':
                case '_':
                case '!':
                    return true;
            }

            return false;
        }

        private static char IntToHex(int i)
        {
            if (i <= 9)
            {
                return (char)(i + 48);
            }

            return (char)((i - 10) + 65);
        }

        private static string RandomWindowsVersion()
        {
            string windowsVersion = "Windows NT ";

            switch (Rand.Next(6))
            {
                case 0:
                    windowsVersion += "5.1"; // Windows XP
                    break;

                case 1:
                    windowsVersion += "6.0"; // Windows Vista
                    break;

                case 2:
                    windowsVersion += "6.1"; // Windows 7
                    break;

                case 3:
                    windowsVersion += "6.2"; // Windows 8
                    break;

                case 4:
                    windowsVersion += "6.3"; // Windows 8.1
                    break;

                case 5:
                    windowsVersion += "10.0"; // Windows 10
                    break;
            }

            return windowsVersion += ";";
        }

        #endregion
    }
}
