using com.alibaba.openapi.client;
using com.alibaba.openapi.client.entity;
using com.alibaba.openapi.client.policy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSdkClient.sdk
{
    public class APIFacade
    {
        private ClientPolicy clientPolicy;

        public APIFacade(ClientPolicy clientPolicy)
        {
            this.clientPolicy = clientPolicy;
        }


        private SyncAPIClient getAPIClient()
        {
            return new SyncAPIClient(clientPolicy);
        }
    }
}
