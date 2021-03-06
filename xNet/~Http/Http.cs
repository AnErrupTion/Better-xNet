﻿using System;
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

        private static string[] UserAgents = { "Mozilla/5.0 ({0}; WOW64; Trident/{1}; rv:{2}) like Gecko", "Mozilla/5.0 ({0}; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/{1} Safari/537.36 OPR/{2}", "Mozilla/5.0 ({0}; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.87 Safari/537.36", "Mozilla/5.0 ({0}; Win64; x64; rv:70.0) Gecko/20100101 Firefox/70.0", "Mozilla/5.0 ({0}; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36 Edge/18.18362", "Mozilla/5.0 ({0}; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Brave Chrome/76.0.3809.132 Safari/537.36", "Mozilla/5.0 ({0}; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.87 Safari/537.36 Edg/78.0.276.24", "Mozilla/5.0 (Linux; U; {0}; SM-J710F Build/M1AJQ; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/77.0.3865.73 Mobile Safari/537.36 OPR/44.1.2254.143214", "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)", "Mozilla/5.0 (compatible; Bingbot/2.0; +http://www.bing.com/bingbot.htm)", "Mozilla/5.0 (compatible; Yahoo! Slurp; http://help.yahoo.com/help/us/ysearch/slurp)", "Mozilla/5.0 (compatible; Baiduspider/2.0; +http://www.baidu.com/search/spider.html)", "Mozilla/5.0 (compatible; YandexBot/3.0; +http://yandex.com/bots)", "Sogou-Test-Spider/4.0 (compatible; MSIE 5.5; Windows 98)", "Mozilla/5.0 (compatible; Konqueror/3.5; Linux) KHTML/3.5.5 (like Gecko) (Exabot-Thumbnails)", "Mozilla/5.0 (compatible; Exabot/3.0; +http://www.exabot.com/go/robot)", "facebookexternalhit/{0} (+http://www.facebook.com/externalhit_uatext.php)", "ia_archiver (+http://www.alexa.com/site/help/webmasters; crawler@alexa.com)", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:60.9) Gecko/20100101 Goanna/4.4 Firefox/60.9 PaleMoon/28.7.2", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:56.0) Gecko/20100101 Firefox/56.0 Waterfox/56.3" };

        /// <summary>
        /// Генерирует случайный User-Agent от браузера IE.
        /// </summary>
        /// <returns>Случайный User-Agent от браузера IE.</returns>

        [Obsolete("This User-Agent is obsolete since the creation of newer browsers, please use the other User-Agents! (Chrome, Firefox, etc...)", true)]
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

            return string.Format(UserAgents[0], windowsVersion, trident, version);
        }

        /// <summary>
        /// Генерирует случайный User-Agent от браузера Opera/Opera GX.
        /// </summary>
        /// <returns>Случайный User-Agent от браузера Opera/Opera GX.</returns>
        public static string OperaUserAgent()
        {
            string windowsVersion = RandomWindowsVersion();

            string chromeVersion = string.Empty;
            string operaVersion = string.Empty;

            #region Генерация случайной версии

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

            #endregion

            return string.Format(UserAgents[1], windowsVersion, chromeVersion, operaVersion);
        }

        /// <summary>
        /// Генерирует случайный User-Agent от браузера Chrome.
        /// </summary>
        /// <returns>Случайный User-Agent от браузера Chrome.</returns>
        public static string ChromeUserAgent()
        {
            return string.Format(UserAgents[2], RandomWindowsVersion());
        }

        /// <summary>
        /// Генерирует случайный User-Agent от браузера Firefox.
        /// </summary>
        /// <returns>Случайный User-Agent от браузера Firefox.</returns>
        public static string FirefoxUserAgent()
        {
            return string.Format(UserAgents[3], RandomWindowsVersion());
        }

        public static string EdgeUserAgent()
        {
            return string.Format(UserAgents[4], RandomWindowsVersion());
        }

        public static string BraveUserAgent()
        {
            return string.Format(UserAgents[5], RandomWindowsVersion());
        }

        public static string ChromiumEdgeBetaUserAgent()
        {
            return string.Format(UserAgents[6], RandomWindowsVersion());
        }

        public static string OperaMiniUserAgent()
        {
            return string.Format(UserAgents[7], RandomAndroidVersion());
        }

        public static string GooglebotUserAgent()
        {
            return UserAgents[8];
        }

        public static string BingbotUserAgent()
        {
            return UserAgents[9];
        }

        public static string YahoobotUserAgent()
        {
            return UserAgents[10];
        }

        public static string BaiduspiderUserAgent()
        {
            return UserAgents[11];
        }

        public static string YandexbotUserAgent()
        {
            return UserAgents[12];
        }

        public static string SogouTestspiderUserAgent()
        {
            return UserAgents[13];
        }

        public static string KonquerorUserAgent()
        {
            return UserAgents[14];
        }

        public static string ExabotUserAgent()
        {
            return UserAgents[15];
        }

        public static string FacebookExtHitUserAgent()
        {
            string version = string.Empty;
            switch (Rand.Next(2))
            {
                case 0:
                    version = "1.0";
                    break;

                case 1:
                    version = "1.1";
                    break;
            }

            return string.Format(UserAgents[16], version);
        }

        public static string AlexabotUserAgent()
        {
            return UserAgents[17];
        }

        public static string PaleMoonUserAgent()
        {
            return UserAgents[18];
        }

        public static string WaterfoxClassicUserAgent()
        {
            return UserAgents[19];
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

            switch (Rand.Next(4))
            {
                case 0:
                    windowsVersion += "6.1"; // Windows 7
                    break;

                case 1:
                    windowsVersion += "6.2"; // Windows 8
                    break;

                case 2:
                    windowsVersion += "6.3"; // Windows 8.1
                    break;

                case 3:
                    windowsVersion += "10.0"; // Windows 10
                    break;
            }

            return windowsVersion;
        }

        private static string RandomAndroidVersion()
        {
            string androidVersion = "Android ";

            switch (Rand.Next(6))
            {
                case 0:
                    androidVersion += "7.0"; // Nougat
                    break;

                case 1:
                    androidVersion += "7.1.2"; // Nougat
                    break;

                case 2:
                    androidVersion += "8.0"; // Oreo
                    break;

                case 3:
                    androidVersion += "8.1"; // Oreo
                    break;

                case 4:
                    androidVersion += "9.0"; // Pie
                    break;

                case 5:
                    androidVersion += "10.0"; // Android 10
                    break;
            }

            return androidVersion;
        }

        #endregion
    }
}