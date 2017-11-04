using com.alibaba.openapi.client.entity;
using com.alibaba.openapi.client.http;
using com.alibaba.openapi.client.policy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace com.alibaba.openapi.client
{
    public class SyncAPIClient
    {
        private ClientPolicy clientPolicy;

        public SyncAPIClient(ClientPolicy clientPolicy)
        {
            this.clientPolicy = clientPolicy;
        }

        public string send<T>(Request request)
        {
            HttpClient httpClient = new HttpClient(clientPolicy);
            string result = httpClient.request<T>(request, request.RequestPolicyInstance, null);
            return result;
        }

        public string send<T>(Request request, String accessToken)
        {
            HttpClient httpClient = new HttpClient(clientPolicy);
            string result = httpClient.request<T>(request, request.RequestPolicyInstance, accessToken);
            return result;
        }
    }
}

