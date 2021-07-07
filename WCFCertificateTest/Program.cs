using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using WCFCertificateTest.ServiceReference1;

namespace WCFCertificateTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ChannelFactory<IService1Channel> factory
                = new ChannelFactory<IService1Channel>("BasicHttpsBinding_IService1");
            factory.Credentials.ClientCertificate.SetCertificate(StoreLocation.CurrentUser, StoreName.My, X509FindType.FindBySubjectName, "hsyed");
            IService1Channel wcfClientChannel = factory.CreateChannel(); 
            
           var data = wcfClientChannel.GetData(1);

        }

    }
    public class MessageEndpointBehavior : IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            // No implementation necessary  
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(new MessageInspector());
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            // No implementation necessary  
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            // No implementation necessary  
        }
    }

    public class MessageInspector : IClientMessageInspector
    {
        public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            
        }

        public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel)
        {
            var userName = Environment.UserName;
            HttpRequestMessageProperty httpRequestMessage;

            if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out var httpRequestMessageObject))
            {
                httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;
                if (httpRequestMessage != null && string.IsNullOrEmpty(httpRequestMessage.Headers["X-User"]))
                {
                    httpRequestMessage.Headers["X-User"] = userName;
                }
            }
            else
            {
                httpRequestMessage = new HttpRequestMessageProperty();
                httpRequestMessage.Headers.Add("X-User", userName);
                request.Properties.Add(HttpRequestMessageProperty.Name, httpRequestMessage);
            }
            return null;
        }
    }
    public class MessageBehaviorExtensionElement : BehaviorExtensionElement
    {
        public override Type BehaviorType => typeof(MessageEndpointBehavior);

        protected override object CreateBehavior()
        {
            // Create the  endpoint behavior that will insert the message  
            // inspector into the client runtime  
            return new MessageEndpointBehavior();
        }
    }


}
