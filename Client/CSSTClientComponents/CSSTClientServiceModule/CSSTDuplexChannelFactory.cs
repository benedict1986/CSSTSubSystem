using System;
using System.CodeDom;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;

namespace CSSTClientServiceModule
{
    public class TRFDuplexChannelFactory<T> : DuplexChannelFactory<T>
    {
        public static string configutatyionPath { get; set; }

        public TRFDuplexChannelFactory() : base(typeof(T))
        {
            this.InitializeEndpoint((string)null, null);
        }

        protected override ServiceEndpoint CreateDescription()
        {
            ServiceEndpoint serviceEndpoint = base.CreateDescription();
            ExeConfigurationFileMap map = new ExeConfigurationFileMap
            {
                ExeConfigFilename = configutatyionPath
            };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            ServiceModelSectionGroup group = ServiceModelSectionGroup.GetSectionGroup(config);
            ChannelEndpointElement selectedEndpoint = @group?.Client.Endpoints.Cast<ChannelEndpointElement>().FirstOrDefault(endpoint => endpoint.Contract == serviceEndpoint.Contract.ConfigurationName);

            if (selectedEndpoint == null) return serviceEndpoint;
            if (serviceEndpoint.Binding == null)
            {
                serviceEndpoint.Binding = this.CreateBinding(selectedEndpoint.Binding, selectedEndpoint.BindingConfiguration, @group);
            }

            if (serviceEndpoint.Address == null)
            {
                serviceEndpoint.Address = new EndpointAddress(selectedEndpoint.Address, this.GetIdentity(selectedEndpoint.Identity), selectedEndpoint.Headers.Headers);
            }

            if (serviceEndpoint.Behaviors.Count == 0 && !string.IsNullOrEmpty(selectedEndpoint.BehaviorConfiguration))
            {
                this.AddBehaviors(selectedEndpoint.BehaviorConfiguration, serviceEndpoint, @group);
            }
            serviceEndpoint.Name = selectedEndpoint.Contract;
            return serviceEndpoint;
        }

        private Binding CreateBinding(string bindingName, string bindingConfiguration, ServiceModelSectionGroup group)
        {
            BindingCollectionElement bindingElementCollection = group.Bindings[bindingName];
            if (bindingElementCollection.ConfiguredBindings.Count <= 0) return null;
            IBindingConfigurationElement be = bindingElementCollection.ConfiguredBindings.FirstOrDefault(bindingElem => string.CompareOrdinal(bindingElem.Name, bindingConfiguration) == 0);
            if (be == null) return null;
            Binding binding = this.GetBinding(be);
            be.ApplyConfiguration(binding);
            return binding;
        }

        private void AddBehaviors(string behaviorConfiguration, ServiceEndpoint serviceEndpoint, ServiceModelSectionGroup group)
        {
            EndpointBehaviorElement behaviorElement = group.Behaviors.EndpointBehaviors[behaviorConfiguration];
            foreach (BehaviorExtensionElement behaviorExtension in behaviorElement)
            {
                object extension = behaviorExtension.GetType().InvokeMember("CreateBehavior",
                    BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
                    null, behaviorExtension, null);
                if (extension != null)
                {
                    serviceEndpoint.Behaviors.Add((IEndpointBehavior)extension);
                }
            }
        }

        private EndpointIdentity GetIdentity(IdentityElement element)
        {
            //Original : EndpointIdentity identity = null;
            PropertyInformationCollection properties = element.ElementInformation.Properties;
            PropertyInformation propertyInformation = properties["userPrincipalName"];
            if (propertyInformation != null && propertyInformation.ValueOrigin != PropertyValueOrigin.Default)
            {
                return EndpointIdentity.CreateUpnIdentity(element.UserPrincipalName.Value);
            }
            propertyInformation = properties["servicePrincipalName"];
            if (propertyInformation != null && propertyInformation.ValueOrigin != PropertyValueOrigin.Default)
            {
                return EndpointIdentity.CreateSpnIdentity(element.ServicePrincipalName.Value);
            }
            propertyInformation = properties["dns"];
            if (propertyInformation != null && propertyInformation.ValueOrigin != PropertyValueOrigin.Default)
            {
                return EndpointIdentity.CreateDnsIdentity(element.Dns.Value);
            }
            propertyInformation = properties["rsa"];
            if (propertyInformation != null && propertyInformation.ValueOrigin != PropertyValueOrigin.Default)
            {
                return EndpointIdentity.CreateRsaIdentity(element.Rsa.Value);
            }
            propertyInformation = properties["certificate"];
            if (propertyInformation == null || propertyInformation.ValueOrigin == PropertyValueOrigin.Default)
                return null;
                //Original : return identity;
            X509Certificate2Collection supportingCertificates = new X509Certificate2Collection();
            supportingCertificates.Import(Convert.FromBase64String(element.Certificate.EncodedValue));
            if (supportingCertificates.Count == 0)
            {
                throw new InvalidOperationException("UnableToLoadCertificateIdentity");
            }
            X509Certificate2 primaryCertificate = supportingCertificates[0];
            supportingCertificates.RemoveAt(0);
            return EndpointIdentity.CreateX509CertificateIdentity(primaryCertificate, supportingCertificates);
        }

        private Binding GetBinding(IBindingConfigurationElement configurationElement)
        {
            if (configurationElement is CustomBindingElement)
                return new CustomBinding();
            else if (configurationElement is BasicHttpBindingElement)
                return new BasicHttpBinding();
            else if (configurationElement is NetMsmqBindingElement)
                return new NetMsmqBinding();
            else if (configurationElement is NetNamedPipeBindingElement)
                return new NetNamedPipeBinding();
            else if (configurationElement is NetTcpBindingElement)
                return new NetTcpBinding();
            else if (configurationElement is WSDualHttpBindingElement)
                return new WSDualHttpBinding();
            else if (configurationElement is WSHttpBindingElement)
                return new WSHttpBinding();
            else if (configurationElement is WSFederationHttpBindingElement)
                return new WSFederationHttpBinding();

            return null;
        }
    }
}
