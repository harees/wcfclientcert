using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Web;

namespace WCFService
{
    public static class X509CertificateExtensions
    {
        /// <summary>
        /// Extracts Username from X509Certificate2 certificate.
        /// </summary>
        /// <param name="certificate">X509Certificate2</param>
        /// <returns></returns>
        public static string ExtractCommonName(this X509Certificate2 certificate)
        {
            if (certificate == null) return string.Empty;

            foreach (string str in certificate.Subject.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                if (!str.Trim().StartsWith("CN", StringComparison.InvariantCultureIgnoreCase)) continue;

                int i = str.IndexOf("=", StringComparison.InvariantCultureIgnoreCase) + 1;
                if (i > 0 && str.Length >= i)
                {
                    return str.Substring(i).Trim();
                }
            }

            return string.Empty;
        }
    }

    public class MyValidator: X509CertificateValidator
    {
        private const string ArrCertificateHeaderKey = "X-ARR-ClientCert";
        private const string ClientCertificateHeaderKey = "X-Client-Certificate";
        private const string Sha1HeaderKey = "X-SSL-Client-SHA1";
        private const string SslClientVerifyHeader = "X-SSL-Client-Verify";
        private const string SslClientCnHeaderKey = "X-SSL-Client-CN";
        public override void Validate(X509Certificate2 certificate)
        {
            var subjectCert = certificate;
            if (subjectCert == null)
            {
                    string certHeader = HttpContext.Current.Request.Headers[ArrCertificateHeaderKey];
                    if (string.IsNullOrWhiteSpace(certHeader))
                        certHeader = HttpContext.Current.Request.Headers[ClientCertificateHeaderKey];
                    var mesageHeaders = OperationContext.Current.IncomingMessageHeaders;
                    if (!string.IsNullOrWhiteSpace(certHeader))
                    {
                        var clientCertBytes = Convert.FromBase64String(certHeader);
                        subjectCert = new X509Certificate2(clientCertBytes);
                    }
            }
            var username = HttpContext.Current.Request.Headers["X-USER"];
            if (subjectCert != null)
            {
                X509CertificateValidator.ChainTrust.Validate(certificate);
                
                var subjectUserName = subjectCert.ExtractCommonName();
                if (!username?.Equals(subjectUserName) ?? false)
                {
                    throw new SecurityTokenValidationException("User not valid");
                }
                return;
            }

            if (HttpContext.Current.Request.Headers[SslClientVerifyHeader]?.Equals("0") ?? false)
            {
                throw new SecurityTokenValidationException("Certificate not found");
            }

            if (HttpContext.Current.Request[Sha1HeaderKey]?.Equals(username) ?? false)
            {
                throw new SecurityTokenValidationException("User not valid");
            }

        }
    }


}