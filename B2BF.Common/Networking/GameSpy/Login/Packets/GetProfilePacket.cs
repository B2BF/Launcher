using B2BF.Common.Account;

namespace B2BF.Common.Networking.GameSpy.Login.Packets
{
    public class GetProfilePacket
    {
        public static void Handle(string[] packet, LoginClient loginClient)
        {
            var id = packet[8];
            loginClient.Send((string.Format(
                    "\\pi\\\\profileid\\{0}\\nick\\{1}\\userid\\{2}\\email\\{3}\\sig\\{4}\\uniquenick\\{5}\\pid\\0\\firstname\\\\lastname\\" +
                    "\\countrycode\\{6}\\birthday\\16844722\\lon\\0.000000\\lat\\0.000000\\loc\\\\id\\{7}\\final\\",
                    AccountInfo.UId, AccountInfo.Username, AccountInfo.UId, "private@b2bf.net", GenerateSig(), AccountInfo.Username, "BE", ((id == "2") ? "2" : "5"))));
        }

        private static string GenerateSig()
        {
            string s = "";
            int length = 32;

            var rand = new Random();

            while (length > 0)
            {
                --length;
                s += chars2[rand.Next(14)];
            }
            return s;
        }

        public const string chars2 = "123456789abcdef";
    }
}