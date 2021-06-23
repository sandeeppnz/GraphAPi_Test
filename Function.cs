using Amazon;
using Amazon.Lambda.Core;
using Amazon.RDS;
using Amazon.RDS.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambda1
{
    public class CumuloMessage
    {
        public Function.MessageT MessageType { get; set; }
        public string CustomerCode { get; set; }
        public dynamic Message { get; set; }

    }

    public class Function
    {
        public enum MessageT
        {
            Undefined = -1,
            Custom = 1,
            RDS_Event = 2,
            CW_Alarm = 3,
            Custom_Azure = 4,
        }

        public CumuloMessage FunctionHandler(string input, ILambdaContext context)
        {

            var rds = new AmazonRDSClient("", "",
                new AmazonRDSConfig
                {
                    ServiceURL = "https://rds.ap-southeast-1.amazonaws.com",
                    RegionEndpoint = RegionEndpoint.APSoutheast2
                });


            var request = new DescribeDBInstancesRequest();
            var response = rds.DescribeDBInstancesAsync(request);
            var instances = response.Result.DBInstances;
            if (instances.Count > 0)
            {
                var messageList = instances;

                var messageObj = new CumuloMessage()
                {
                    MessageType = MessageT.Custom,
                    CustomerCode = "",
                    Message = messageList
                };

                return messageObj;
            }
            return null;
        }
    }
}
