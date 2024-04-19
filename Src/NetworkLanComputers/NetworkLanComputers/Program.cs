using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Data;
using System.Collections;
using System.Runtime.InteropServices;

namespace NetworkLanComputers
{
    internal class Program
	{
		public class IPEnumeration : IEnumerable
		{
			private string startAddress;
			private string endAddress;

			internal static Int64 AddressToInt(IPAddress addr)
			{
				byte[] addressBits = addr.GetAddressBytes();

				Int64 retval = 0;
				for (int i = 0; i < addressBits.Length; i++)
				{
					retval = (retval << 8) + (int)addressBits[i];
				}

				return retval;
			}

			internal static Int64 AddressToInt(string addr)
			{
				return AddressToInt(IPAddress.Parse(addr));
			}

			internal static IPAddress IntToAddress(Int64 addr)
			{
				return IPAddress.Parse(addr.ToString());
			}


			public IPEnumeration(string startAddress, string endAddress)
			{
				this.startAddress = startAddress;
				this.endAddress = endAddress;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return (IEnumerator)GetEnumerator();
			}

			public IPEnumerator GetEnumerator()
			{
				return new IPEnumerator(startAddress, endAddress);
			}

		}

		public class IPEnumerator : IEnumerator
		{
			private string startAddress;
			private string endAddress;
			private Int64 currentIP;
			private Int64 endIP;

			public IPEnumerator(string startAddress, string endAddress)
			{
				this.startAddress = startAddress;
				this.endAddress = endAddress;

				currentIP = IPEnumeration.AddressToInt(startAddress);
				endIP = IPEnumeration.AddressToInt(endAddress);
			}

			public bool MoveNext()
			{
				currentIP++;
				return (currentIP <= endIP);
			}

			public void Reset()
			{
				currentIP = IPEnumeration.AddressToInt(startAddress);
			}

			object IEnumerator.Current
			{
				get
				{
					return Current;
				}
			}

			public IPAddress Current
			{
				get
				{
					try
					{
						return IPEnumeration.IntToAddress(currentIP);
					}
					catch (IndexOutOfRangeException)
					{
						throw new InvalidOperationException();
					}
				}
			}
		}

		public static class IPHelper
		{
			[DllImport("iphlpapi.dll", ExactSpelling = true)]
			public static extern int SendARP(int DestIP, int SrcIP, byte[] pMacAddr, ref uint PhyAddrLen);

			public static string getMAC(IPAddress address)
			{
				int intAddress = BitConverter.ToInt32(address.GetAddressBytes(), 0);

				byte[] macAddr = new byte[6];
				uint macAddrLen = (uint)macAddr.Length;
				if (SendARP(intAddress, 0, macAddr, ref macAddrLen) != 0)
					return "(NO ARP result)";

				string[] str = new string[(int)macAddrLen];
				for (int i = 0; i < macAddrLen; i++)
					str[i] = macAddr[i].ToString("x2");

				return string.Join(":", str);
			}
		}
        static void Main(string[] args)
        {
			foreach (IPAddress addr in new IPEnumeration("192.168.1.1", "192.168.1.128"))
			{
				if (IPHelper.getMAC(addr).Contains(":"))
				{
					Console.WriteLine(addr.ToString() + ", " + IPHelper.getMAC(addr) + ", " + GetLocalName(addr));
				}
			}
			Console.ReadKey();
        }
        public static string GetLocalName(IPAddress adress)
        {
            var hostname = Dns.GetHostEntry(adress.ToString()).HostName;
            return hostname;
        }
    }
}