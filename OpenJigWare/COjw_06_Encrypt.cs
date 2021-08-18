using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;

namespace OpenJigWare
{
    partial class Ojw
    {
        // For Secret
        public class CEncryption
        {
            private static String m_strMasterKey = "OJW5014"; // Default Master Key
            
            // Change master key
            public static bool SetMasterKey(String strOldKey, String strNewKey)
            {
                if (m_strMasterKey == strOldKey)
                {
                    m_strMasterKey = strNewKey;
                    return true;
                }
                return false;
            }
            public static void SetEncrypt(String strSet)
            {
                m_bEncryptionEnable = (strSet == m_strMasterKey) ? true : false;
            }
            public static bool IsEncrypt() { return m_bEncryptionEnable; }

            #region Secret Function
            // you can change if you change secret key which no one knows it
            private static String m_strLetter = "skfktakfTkadlebdrnlrdpekfdkanswkdhkfhtjfmtkakteldkslgkfTo.dlfjswjsckfhdjflsqortjddlslfmrhwjghfgqodltueh,akckasowpEmemftlfjvuelahtgkfshalgkslfk.sodlfmfdnlgkdudjdutqlsjrutofhtmamfduemfqwkfkfaodrkshsl,tkfkaakekgodutnqlslruskffhTnapvusdkszpgkrhwugkfEkfkalxlfk.";

            public static char LetterCode2Letter(int nIndex, char cOrgData)
            {
                int nSize = m_strLetter.Length;
                if (nSize <= 0) return (char)0;
                //String strLetter = "skfktakfTkadlebdrnlrdpekfdkanswkdhkfhtjfmtkakteldkslgkfTo.dlfjswjsckfhdjflsqortjddlslfmrhwjghfgqodltueh,akckasowpEmemftlfjvuelahtgkfshalgkslfk.sodlfmfdnlgkdudjdutqlsjrutofhtmamfduemfqwkfkfaodrkshsl,tkfkaakekgodutnqlslruskffhTnapvusdkszpgkrhwugkfEkfkalxlfk.";

                /* [password key]
                 * 나랏말쌈이듕귁에달아문자와로서르사맛디아니할쌔.이런전차로어린백성이니르고저홇배이셔도,마참내제뜨들시러펴디못할노미하니라.내이를위하여어엿비너겨새로스믈여듧자랄맹가노니,사람마다해여수비니겨날로쑤메편안케하고져할따라미니라.
                 * ==================================================================================================================
                 * skfktakfTkadlebdrnlrdpekfdkanswkdhkfhtjfmtkakteldkslgkfTo.   나랏말쌈이듕귁에달아문자와로서르사맛디아니할쌔.
                 * dlfjswjsckfhdjflsqortjddlslfmrhwjghfgqodltueh,               이런전차로어린백성이니르고저홇배이셔도,
                 * akckasowpEmemftlfjvuelahtgkfshalgkslfk.                      마참내제뜨들시러펴디못할노미하니라.
                 * sodlfmfdnlgkdudjdutqlsjrutofhtmamfduemfqwkfkfaodrkshsl,      내이를위하여어엿비너겨새로스믈여듧자랄맹가노니,
                 * tkfkaakekgodutnqlslruskffhTnapvusdkszpgkrhwugkfEkfkalxlfk.   사람마다해여수비니겨날로쑤메편안케하고져할따라미니라.
                 * ==================================================================================================================
                 * skfktakfTk adlebdrnlr dpekfdkans wkdhkfhtjf mtkakteldk -> 50
                 * slgkfTo.dl fjswjsckfh djflsqortj ddlslfmrhw jghfgqodlt -> 100
                 * ueh,akckas owpEmemftl fjvuelahtg kfshalgksl fk.sodlfmf -> 150
                 * dnlgkdudjd utqlsjruto fhtmamfdue mfqwkfkfao drkshsl,tk -> 200
                 * fkaakekgod utnqlslrus kffhTnapvu sdkszpgkrh wugkfEkfka -> 250
                 * lxlfk. -> 256
                 * ==================================================================================================================
                 * skfktakfTkadlebdrnlrdpekfdkanswkdhkfhtjfmtkakteldkslgkfTo.dlfjswjsckfhdjflsqortjddlslfmrhwjghfgqodltueh,akckasowpEmemftlfjvuelahtgkfshalgkslfk.sodlfmfdnlgkdudjdutqlsjrutofhtmamfduemfqwkfkfaodrkshsl,tkfkaakekgodutnqlslruskffhTnapvusdkszpgkrhwugkfEkfkalxlfk.             * 
                 */
                return (char)(m_strLetter[nIndex % nSize] ^ cOrgData);
            }
            public static byte LetterCode2Letter(int nIndex, byte byOrgData)
            {
                int nSize = m_strLetter.Length;
                if (nSize <= 0) return (byte)0;
                //String strLetter = "skfktakfTkadlebdrnlrdpekfdkanswkdhkfhtjfmtkakteldkslgkfTo.dlfjswjsckfhdjflsqortjddlslfmrhwjghfgqodltueh,akckasowpEmemftlfjvuelahtgkfshalgkslfk.sodlfmfdnlgkdudjdutqlsjrutofhtmamfduemfqwkfkfaodrkshsl,tkfkaakekgodutnqlslruskffhTnapvusdkszpgkrhwugkfEkfkalxlfk.";

                /* [password key]
                 * 나랏말쌈이듕귁에달아문자와로서르사맛디아니할쌔.이런전차로어린백성이니르고저홇배이셔도,마참내제뜨들시러펴디못할노미하니라.내이를위하여어엿비너겨새로스믈여듧자랄맹가노니,사람마다해여수비니겨날로쑤메편안케하고져할따라미니라.
                 * ==================================================================================================================
                 * skfktakfTkadlebdrnlrdpekfdkanswkdhkfhtjfmtkakteldkslgkfTo.   나랏말쌈이듕귁에달아문자와로서르사맛디아니할쌔.
                 * dlfjswjsckfhdjflsqortjddlslfmrhwjghfgqodltueh,               이런전차로어린백성이니르고저홇배이셔도,
                 * akckasowpEmemftlfjvuelahtgkfshalgkslfk.                      마참내제뜨들시러펴디못할노미하니라.
                 * sodlfmfdnlgkdudjdutqlsjrutofhtmamfduemfqwkfkfaodrkshsl,      내이를위하여어엿비너겨새로스믈여듧자랄맹가노니,
                 * tkfkaakekgodutnqlslruskffhTnapvusdkszpgkrhwugkfEkfkalxlfk.   사람마다해여수비니겨날로쑤메편안케하고져할따라미니라.
                 * ==================================================================================================================
                 * skfktakfTk adlebdrnlr dpekfdkans wkdhkfhtjf mtkakteldk -> 50
                 * slgkfTo.dl fjswjsckfh djflsqortj ddlslfmrhw jghfgqodlt -> 100
                 * ueh,akckas owpEmemftl fjvuelahtg kfshalgksl fk.sodlfmf -> 150
                 * dnlgkdudjd utqlsjruto fhtmamfdue mfqwkfkfao drkshsl,tk -> 200
                 * fkaakekgod utnqlslrus kffhTnapvu sdkszpgkrh wugkfEkfka -> 250
                 * lxlfk. -> 256
                 * ==================================================================================================================
                 * skfktakfTkadlebdrnlrdpekfdkanswkdhkfhtjfmtkakteldkslgkfTo.dlfjswjsckfhdjflsqortjddlslfmrhwjghfgqodltueh,akckasowpEmemftlfjvuelahtgkfshalgkslfk.sodlfmfdnlgkdudjdutqlsjrutofhtmamfduemfqwkfkfaodrkshsl,tkfkaakekgodutnqlslruskffhTnapvusdkszpgkrhwugkfEkfkalxlfk.             * 
                 */
                return (byte)(m_strLetter[nIndex % nSize] ^ byOrgData);
            }
            private static bool m_bEncryptionEnable = false;
            //public static String Encryption(bool bEncryptionMode, String strData)
            //{
            //    byte[] byteData = Encoding.Default.GetBytes(strData);
            //    byteData = Encryption(bEncryptionMode, byteData);
            //    return Encoding.Default.GetString(byteData);
            //}
            public static byte[] Encryption(bool bEncryptionMode, byte[] byteData)
            {
                int nHeaderSize = 2; // "!" , oxff
                if ((m_bEncryptionEnable == false) && (bEncryptionMode == false)) return byteData; // 암호화 하는 것은 굳이 안해도 되지만 암호화 해제는 보안이 필요.
                // Check it first, header is ("!" , oxff) or not
                if (byteData.Length < nHeaderSize) return byteData;
                if ((byteData[0] == '!') && ((char)(byteData[1]) == 0xff)) // it is made with encryption(Kor: 이미 암호화 되어 있는 경우)
                {
                    if (bEncryptionMode == true) return byteData; // Do not encrypt when it is made with encryption already(Kore: 암호화를 해야하는데 이미 암호화 되어 있다면 안하도록...
                    // It'll change from encryption to normal(Kor: 이경우는 암호화 헤제 명령만 받는다.)
                    else
                    {
                        // Remove Encryption Header(Kor: 암호화 헤더를 없앤다.)
                        byte[] byteData2 = new byte[byteData.Length - nHeaderSize];
                        Array.Copy(byteData, nHeaderSize, byteData2, 0, byteData2.Length);
                        Array.Resize<byte>(ref byteData, byteData2.Length);
                        Array.Copy(byteData2, byteData, byteData2.Length);
                        byteData2 = null;

                        // Remove Encryption(Kor: 암호화 해제)
                        for (int i = 0; i < byteData.Length; i++)
                            byteData[i] = (byte)LetterCode2Letter(i % (m_strLetter.Length), byteData[i]);
                    }
                }
                else // as it made without encryption(Kore: 암호화 되어 있는 파일이 아닌 경우)
                {
                    if (bEncryptionMode == false) return byteData; // just return it becase of no encryption file(Kor: 암호화 되어 있지 않은 파일은 암호화 해제가 필요 없으므로 그냥 리턴)
                    // It'll change from normal to encryption(Kor: 이 경우는 암호화 설정 명령만 받는다.)
                    else
                    {
                        // Add a Encryption Code(Kor: 암호화 코드 삽입)
                        byte[] byteData2 = new byte[byteData.Length + nHeaderSize];
                        Array.Copy(byteData, 0, byteData2, nHeaderSize, byteData.Length);
                        Array.Resize<byte>(ref byteData, byteData2.Length);
                        Array.Copy(byteData2, byteData, byteData2.Length);
                        byteData2 = null;
                        byteData[0] = (byte)'!';
                        byteData[1] = (byte)0xff;

                        // Encryption - No Header(Kor: 암호화 - 헤더 제외)
                        for (int ii = nHeaderSize; ii < byteData.Length; ii++)
                        {
                            int i = ii - nHeaderSize;
                            byteData[ii] = (byte)LetterCode2Letter(i % (m_strLetter.Length), byteData[ii]);
                        }
                    }
                }

                m_bEncryptionEnable = false; // Change it Encryption Enable to false(Kor: 다시 암호화 Enable 을 해제한다.)
                return byteData;
            }
            #endregion Secret Function
        }
    }
}
