using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#pragma warning disable CA1305 // Specify IFormatProvider

namespace Trezor.Net
{
    public static class EthHelpers
    {
        private const string Format = "X1";
        private static readonly Encoding Encoding = new UTF8Encoding();

        public static string ToHexString(this IEnumerable<byte> bytes) => bytes.Aggregate(string.Empty, (current, theByte) => current + theByte.ToString("X2"));

        public static byte[] ToHexBytes(this string ethString)
        {
            if (ethString == null) throw new ArgumentNullException(nameof(ethString));

            var numberOfCharacters = ethString.Length / 2;
            var returnValue = new byte[numberOfCharacters];

            for (var i = 0; i < numberOfCharacters; i++)
            {
                var x = i * 2;
                var firstHexCharacter = ethString[x];
                var secondHexCharacter = ethString[x + 1];

                var hexStringBuilder = new StringBuilder();
                _ = hexStringBuilder.Append(firstHexCharacter);
                _ = hexStringBuilder.Append(secondHexCharacter);

                var hexString = hexStringBuilder.ToString();

                returnValue[i] = byte.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
            }

            return returnValue;
        }

        public static string ToHex(this long number) => number.ToString(Format);

        public static string ToHex(this int number) => number.ToString(Format);

        public static byte[] ToHexBytes(this int number) => Encoding.GetBytes(number.ToHex());

        public static byte[] ToHexBytes(this long number) => Encoding.GetBytes(number.ToHex());

        public static byte[] ToEthBytes(this long number) => Encoding.GetBytes($"0x{ToHexBytes(number)}");

        public static byte[] ToEthBytes(this int number) => Encoding.GetBytes($"0x{ToHexBytes(number)}");
    }
}
