using com.alibaba.openapi.client.entity;
using com.alibaba.openapi.client.policy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace com.alibaba.openapi.client
{
    [DataContract(Namespace = "api.php")]
    public class Request
    {
        [DataMember(Order = 0)]
        private APIId oceanApiId = new APIId();

        public APIId OceanApiId
        {
            get { return oceanApiId; }
            set { oceanApiId = value; }
        }

        private RequestPolicy requestPolicyInstance = new RequestPolicy();
        public RequestPolicy RequestPolicyInstance
        {
            get { return requestPolicyInstance; }
            set { requestPolicyInstance = value; }
        }
        
    }
}
