﻿using goudiw.sdk.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace com.goudiw
{
    [DataContract(Namespace = "com.alibaba.openapi.client")]
    public class AbstractRequest<ResponseType>
    {
        [DataMember(Order = 0)]
        private APIId apiId;

        public APIId ApiId
        {
            get { return apiId; }
            set { apiId = value; }
        }
        private Dictionary<String, Object> addtionalParams = new Dictionary<String, Object>();

        public Dictionary<String, Object> AddtionalParams
        {
            get { return addtionalParams; }
            set { addtionalParams = value; }
        }
        private Object requestEntity;

        public Object RequestEntity
        {
            get { return requestEntity; }
            set { requestEntity = value; }
        }
        private Dictionary<String, String> attachments;

        public Dictionary<String, String> Attachments
        {
            get { return attachments; }
            set { attachments = value; }
        }
       
        [DataMember(Order = 1)]
        private String accessToken;

        public String AccessToken
        {
            get { return accessToken; }
            set { accessToken = value; }
        }

    }
}
