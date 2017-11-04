﻿using com.alibaba.openapi.client.policy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.alibaba.openapi.client.entity
{
    public class GetTokenRequest : Request
    {
        public GetTokenRequest()
        {
            OceanApiId = new APIId();
            OceanApiId.Name = "getToken";
            OceanApiId.NamespaceValue = "system.oauth2";
            OceanApiId.Version = 1;

            RequestPolicyInstance = new RequestPolicy();
            RequestPolicyInstance.UseHttps = true;
        }

        private String code;

        public void setCode(String code)
        {
            this.code = code;
        }
        public String getCode()
        {
            return this.code;
        }

        private String grant_type;

        public void setGrant_type(String grant_type)
        {
            this.grant_type = grant_type;
        }
        public String getGrant_type()
        {
            return this.grant_type;
        }

        private bool need_refresh_token;

        public void setNeed_refresh_token(bool need_refresh_token)
        {
            this.need_refresh_token = need_refresh_token;
        }
        public bool Need_refresh_token()
        {
            return this.need_refresh_token;
        }

        private String client_id;

        public void setClient_id(String client_id)
        {
            this.client_id = client_id;
        }
        public String getClient_id()
        {
            return this.client_id;
        }
        private String client_secret;

        public void setClient_secret(String client_secret)
        {
            this.client_secret = client_secret;
        }
        public String getClient_secret()
        {
            return this.client_secret;
        }


    }
}
