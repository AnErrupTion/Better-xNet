using System;
using System.Collections.Generic;
using System.Text;

namespace Better_xNet
{
    /// <summary>
    /// Представляет коллекцию HTTP-куки.
    /// </summary>
    public class CookieStorage : Dictionary<string, string>
    {
        /// <summary>
        /// Возвращает или задает значение, указывающие, закрыты ли куки для редактирования
        /// </summary>
        /// <value>Значение по умолчанию — <see langword="false"/>.</value>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CookieDictionary"/>.
        /// </summary>
        /// <param name="isLocked">Указывает, закрыты ли куки для редактирования.</param>
        public CookieStorage(bool isLocked = false) : base(StringComparer.OrdinalIgnoreCase) { IsLocked = isLocked; }

        /// <summary>
        /// Возвращает строку, состоящую из имён и значений куки.
        /// </summary>
        /// <returns>Строка, состоящая из имён и значений куки.</returns>
        override public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> cookie in this) sb.AppendFormat("{0}={1}; ", cookie.Key, cookie.Value);
            if (sb.Length > 0) sb.Remove(sb.Length - 2, 2);
            return sb.ToString();
        }
    }
}