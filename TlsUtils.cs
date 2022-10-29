using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;

namespace MiniHttpsServer
{
    internal class TlsUtils
    {
        internal static TlsEncryptionCredentials LoadEncryptionCredentials(Certificate certificate, AsymmetricKeyEntry asymmetricKeyEntry, TlsContext context)
        {
            return (TlsEncryptionCredentials)new DefaultTlsEncryptionCredentials(context, certificate, asymmetricKeyEntry.Key);
        }

        internal static TlsSignerCredentials LoadSignerCredentials(Certificate certificate, AsymmetricKeyEntry asymmetricKeyEntry, TlsContext context, IList supportedSignatureAlgorithms)
        {
            SignatureAndHashAlgorithm val = null;
            if (supportedSignatureAlgorithms != null)
            {
                foreach (SignatureAndHashAlgorithm supportedSignatureAlgorithm in supportedSignatureAlgorithms)
                {
                    SignatureAndHashAlgorithm val2 = supportedSignatureAlgorithm;
                    if (val2.Signature == 1)
                    {
                        val = val2;
                        break;
                    }
                }
                if (val == null)
                {
                    return null;
                }
            }
            return new DefaultTlsSignerCredentials(context, certificate, asymmetricKeyEntry.Key, val);
        }

        internal static Certificate GetCertificateFromP12(string p12Path, string p12Passwd, out AsymmetricKeyEntry asymmetricKeyEntry)
        {
            Pkcs12Store store = new Pkcs12StoreBuilder().Build();

            if (string.IsNullOrEmpty(p12Passwd))
            {
                using (FileStream fileStream = File.OpenRead(p12Path))
                {
                    store.Load(fileStream, new char[0]);
                }
            }
            else
            {
                char[] array = p12Passwd.ToCharArray();
                using (FileStream fileStream2 = File.OpenRead(p12Path))
                {
                    store.Load(fileStream2, array);
                }
            }
            string _alias = null;
            foreach (string alias in store.Aliases)
            {
                if (store.IsKeyEntry(alias))
                {
                    _alias = alias;
                    break;
                }
            }
            asymmetricKeyEntry = store.GetKey(_alias);
            X509CertificateEntry[] certificateChain = store.GetCertificateChain(_alias);
            X509CertificateStructure[] array2 = (X509CertificateStructure[])(object)new X509CertificateStructure[certificateChain.Length];
            for (int i = 0; i < certificateChain.Length; i++)
            {
                array2[i] = certificateChain[i].Certificate.CertificateStructure;
            }

            return new Certificate(array2);
        }

        internal static string Fingerprint(X509CertificateStructure x509CertificateStructure)
        {
            byte[] encoded = x509CertificateStructure.GetEncoded();
            byte[] array = Sha256DigestOf(encoded);
            byte[] bytes = Hex.Encode(array);
            string text = Encoding.ASCII.GetString(bytes).ToUpper(CultureInfo.InvariantCulture);
            StringBuilder stringBuilder = new StringBuilder();
            int num = 0;
            stringBuilder.Append(text.Substring(0, 2));
            while ((num += 2) < text.Length)
            {
                stringBuilder.Append(':');
                stringBuilder.Append(text.Substring(num, 2));
            }
            return stringBuilder.ToString();
        }

        internal static byte[] Sha256DigestOf(byte[] input)
        {
            return DigestUtilities.CalculateDigest("SHA256", input);
        }
    }
}
