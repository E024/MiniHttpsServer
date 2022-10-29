using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MiniHttpsServer
{
    internal class TlsAuthentication : Org.BouncyCastle.Crypto.Tls.TlsAuthentication
    {

        private readonly TlsContext mContext;

        private readonly Certificate certificate;

        private readonly AsymmetricKeyEntry keyEntry;

        internal TlsAuthentication(Certificate certificate, AsymmetricKeyEntry keyEntry, TlsContext context)
        {
            this.certificate = certificate;
            this.keyEntry = keyEntry;
            mContext = context;
        }

        public virtual void NotifyServerCertificate(Certificate serverCertificate)
        {
            X509CertificateStructure[] certificateList = serverCertificate.GetCertificateList();
            Console.WriteLine(MethodBase.GetCurrentMethod() + "TLS获取到服务端证书链长度:" + certificateList.Length);
            for (int i = 0; i != certificateList.Length; i++)
            {
                X509CertificateStructure val = certificateList[i];
                Console.WriteLine(MethodBase.GetCurrentMethod() + string.Concat("    fingerprint:SHA-256 ", TlsUtils.Fingerprint(val), " (", val.Subject, ")"));
            }
        }

        public virtual TlsCredentials GetClientCredentials(CertificateRequest certificateRequest)
        {
            byte[] certificateTypes = certificateRequest.CertificateTypes;
            if (certificateTypes != null && Arrays.Contains(certificateTypes, 1))
            {
                return (TlsCredentials)(object)TlsUtils.LoadSignerCredentials(certificate, keyEntry, mContext, certificateRequest.SupportedSignatureAlgorithms);
            }
            return null;
        }
    }
}
