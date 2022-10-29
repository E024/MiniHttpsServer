using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Pkcs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MiniHttpsServer
{
    internal class TlsServer : DefaultTlsServer
    {
        private readonly Certificate certificate;
        private readonly AsymmetricKeyEntry keyEntry;

        protected override ProtocolVersion MaximumVersion
        {
            get
            {
                return ProtocolVersion.TLSv12;
            }
        }

        internal TlsServer(Certificate certificate, AsymmetricKeyEntry keyEntry)
        {
            this.certificate = certificate;
            this.keyEntry = keyEntry;
        }

        //public override ProtocolVersion GetServerVersion()
        //{
        //    ProtocolVersion serverVersion = this.GetServerVersion();
        //    Console.WriteLine(MethodBase.GetCurrentMethod() + "Tls服务端协商提供服务版本为:" + serverVersion);
        //    return serverVersion;
        //}

        public override void NotifyAlertReceived(byte alertLevel, byte alertDescription)
        {
            string strLogMessage = "Tls服务端收到alert信号: " + AlertLevel.GetText(alertLevel) + ", " + AlertDescription.GetText(alertDescription);
            if (alertLevel == 2)
            {
                Console.WriteLine(MethodBase.GetCurrentMethod() + strLogMessage);
            }
            else
            {
                Console.WriteLine(MethodBase.GetCurrentMethod() + strLogMessage);
            }
        }


        protected override TlsEncryptionCredentials GetRsaEncryptionCredentials()
        {
            return TlsUtils.LoadEncryptionCredentials(certificate, keyEntry, ((TlsContext)this));

        }
        protected override TlsSignerCredentials GetRsaSignerCredentials()
        {
            return TlsUtils.LoadSignerCredentials(certificate, keyEntry, mContext, mSupportedSignatureAlgorithms);
        }
    }
}
