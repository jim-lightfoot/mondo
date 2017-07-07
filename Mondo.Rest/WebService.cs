/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Rest							                    */
/*             File: WebService.cs								            */
/*        Class(es): WebService								                */
/*          Purpose: Calls a Web service (REST API)                         */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: Aug 3 2016                                             */
/*                                                                          */
/*   Copyright (c) 2016-2017 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

using Mondo.Common;

namespace Mondo.Rest
{
    /****************************************************************************/
    /****************************************************************************/
    public class WebService : IDisposable
    {
        private readonly string           _baseUrl;
        private readonly Lazy<HttpClient> _client;

        /************************************************************************/
        public WebService(string baseUrl)
        {
            _baseUrl = baseUrl;

            _client = new Lazy<HttpClient>( ()=>
            {
                HttpClient client = null;

                if(this.Certificate != null)
                {
                    var handler = new WebRequestHandler();

                    handler.ClientCertificates.Add(this.Certificate);

                    client = new HttpClient(handler);
                }
                else
                    client = new HttpClient();

                return client;
            },
            true);

        }

        /*********************************************************************/
        public string           UserName    { get; set; }
        public string           Password    { get; set; }
        public bool             Cache       { get; set; } = false;
        public X509Certificate  Certificate { get; set; }

        /*********************************************************************/
        public async Task<JToken> Get(string endPoint = "")
        {
            return await CallWebService(endPoint, HttpMethod.Get, null);
        }

        /*********************************************************************/
        public async Task<JToken> Delete(string endPoint = "")
        {
            return await CallWebService(endPoint, HttpMethod.Delete, null, false);
        }

        /*********************************************************************/
        public async Task<JToken> Post(string endPoint, JToken data, bool expectResult = true)
        {
            return await CallWebService(endPoint, HttpMethod.Post, data != null ? new StringContent(data.ToString()) : null, expectResult);
        }

        /*********************************************************************/
        public async Task<JToken> Post(string endPoint, IDictionary<string, string> data, bool expectResult = true)
        {
            HttpContent content = null;

            if (data != null && data.Count  > 0)
            {
                content = new FormUrlEncodedContent(data);
                content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };
            }

            return await CallWebService(endPoint, HttpMethod.Post, content, expectResult);
        }

        /*********************************************************************/
        public async Task<JToken> Put(string endPoint, JToken data = null, bool expectResult = true)
        {
            return await CallWebService(endPoint, HttpMethod.Put, data != null ? new StringContent(data.ToString()) : null, expectResult);
        }

        /*********************************************************************/
        public virtual async Task SetHeaders(HttpRequestHeaders headers)
        {
            if(!this.Cache)
                headers.CacheControl = new CacheControlHeaderValue { NoCache = true };

            if(!string.IsNullOrWhiteSpace(this.UserName))
                headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{this.UserName}:{this.Password}")));
        }

        /*********************************************************************/
        private async Task<JToken> CallWebService(string endPoint, HttpMethod method, HttpContent content, bool expectResult = true)
        {
            string url = Url.Combine(_baseUrl, endPoint);

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method     = method,
            };

            if (content != null)
                request.Content = content;

            await SetHeaders(request.Headers);

            var client = _client.Value;
            var result = "";

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();

                result = await response.Content.ReadAsStringAsync();

                if (!expectResult)
                    return (null);
            }

            return JToken.Parse(result);
        }

        /*********************************************************************/
        public void Dispose()
        {
            if(_client.IsValueCreated)
                _client.Value.Dispose();
        }
    }
}
