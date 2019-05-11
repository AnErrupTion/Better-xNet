using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xNet.xNet._Http
{
    class HttpHelper
    {
        public HttpResponse Send(HttpMethod method, string address, HttpContent content = null)
        {
            #region Проверка параметров

            if (address == null)
            {
                throw new ArgumentNullException("address");
            }

            if (address.Length == 0)
            {
                throw ExceptionHelper.EmptyString("address");
            }

            #endregion

            return Raw(method, address, content);
        }

        private HttpResponse Raw(HttpMethod method, string address, HttpContent content = null)
        {
            #region Проверка параметров

            if (address == null)
            {
                throw new ArgumentNullException("address");
            }

            if (address.Length == 0)
            {
                throw ExceptionHelper.EmptyString("address");
            }

            #endregion

            var uri = new Uri(address, UriKind.RelativeOrAbsolute);
            return new HttpRequest().Raw(method, uri, content);
        }
    }
}
