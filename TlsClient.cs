using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MiniHttpsServer
{
    internal class TlsClient : DefaultTlsClient
    {

        internal TlsSession mSession;

        private readonly Certificate certificate;

        private readonly AsymmetricKeyEntry keyEntry;

        public override ProtocolVersion ClientVersion
        {
            get
            {
                return ProtocolVersion.TLSv12;
            }
        }

        internal TlsClient(TlsSession session, Certificate certificate, AsymmetricKeyEntry keyEntry)
        {
            mSession = session;
            this.certificate = certificate;
            this.keyEntry = keyEntry;
        }

        public override TlsSession GetSessionToResume()
        {
            return mSession;
        }

        public override void NotifyAlertRaised(byte alertLevel, byte alertDescription, string message, Exception cause)
        {
            string strLogMessage = "TLS客户端发起alert信号: " + AlertLevel.GetText(alertLevel) + ", " + AlertDescription.GetText(alertDescription);
            Console.WriteLine(MethodBase.GetCurrentMethod() + strLogMessage);

        }

        public override void NotifyAlertReceived(byte alertLevel, byte alertDescription)
        {
            string strLogMessage = "Tls客户端收到alert信号: " + AlertLevel.GetText(alertLevel) + ", " + AlertDescription.GetText(alertDescription);

            Console.WriteLine(MethodBase.GetCurrentMethod() + strLogMessage);
        }

        public override IDictionary GetClientExtensions()
        {
            IDictionary dictionary = TlsExtensionsUtilities.EnsureExtensionsInitialised(this.GetClientExtensions());
            TlsExtensionsUtilities.AddEncryptThenMacExtension(dictionary);
            return dictionary;
        }


        public override void NotifyServerVersion(ProtocolVersion serverVersion)
        {
            ((AbstractTlsClient)this).NotifyServerVersion(serverVersion);
            Console.WriteLine(MethodBase.GetCurrentMethod() + "TLS客户端协商版本:" + serverVersion);
        }

        public override Org.BouncyCastle.Crypto.Tls.TlsAuthentication GetAuthentication()
        {
            return (Org.BouncyCastle.Crypto.Tls.TlsAuthentication)(object)new TlsAuthentication(certificate, keyEntry, mContext);
        }

        public override void NotifyHandshakeComplete()
        {
            this.NotifyHandshakeComplete();
            TlsSession resumableSession = mContext.ResumableSession;
            if (resumableSession != null)
            {
                byte[] sessionID = resumableSession.SessionID;
                string text = Hex.ToHexString(sessionID);
                if (mSession != null && Arrays.AreEqual(mSession.SessionID, sessionID))
                {
                    Console.WriteLine(MethodBase.GetCurrentMethod() + "Resumed session: " + text);
                }
                else
                {
                    Console.WriteLine(MethodBase.GetCurrentMethod() + "Established session: " + text);
                }
                mSession = resumableSession;
            }
        }
    }
}
