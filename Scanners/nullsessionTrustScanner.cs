﻿//
// Copyright (c) Ping Castle. All rights reserved.
// https://www.pingcastle.com
//
// Licensed under the Non-Profit OSL. See LICENSE file in the project root for full license information.
//
using PingCastle.ADWS;
using PingCastle.Healthcheck;
using PingCastle.misc;
using PingCastle.RPC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace PingCastle.Scanners
{
	public class nullsessionTrustScanner : IScanner
	{

		public string Name { get { return "nullsession-trust"; } }
		public string Description { get { return "Dump the trusts of a domain via null session if possible"; } }

		public string Server { get; private set; }
		public int Port { get; private set; }
		public NetworkCredential Credential { get; private set; }

		public void Initialize(string server, int port, NetworkCredential credential)
		{
			Server = server;
			Port = port;
			Credential = credential;
		}

		public bool QueryForAdditionalParameterInInteractiveMode()
		{
			return true;
		}

		public void Export(string filename)
        {
			DisplayAdvancement("Starting");
			nrpc session = new nrpc(); ;
			DisplayAdvancement("Trusts obtained via null session");
			List<TrustedDomain> domains;
			int res = session.DsrEnumerateDomainTrusts(Server, 0x3F, out domains);
			if (res != 0)
			{
				DisplayAdvancement("Error " + res + " (" + new Win32Exception(res).Message + ")");
				return;
			}
			DisplayAdvancement("Success - " + domains.Count + " trusts found");
			using (StreamWriter sw = File.CreateText(filename))
			{
				sw.WriteLine("Trust index,DnsDomainName,NetbiosDomainName,TrustAttributes,TrustType,Flags,DomainGuid,DomainSid,ParentIndex");
				int i = 0;
				foreach (var domain in domains)
				{
					sw.WriteLine(i++ + "," + domain.DnsDomainName + "," + domain.NetbiosDomainName + "," +
						TrustAnalyzer.GetTrustAttribute(domain.TrustAttributes) + " (" + domain.TrustAttributes + ")" + "," +
						TrustAnalyzer.GetTrustType(domain.TrustType) + " (" + domain.TrustType + ")" + "," + domain.Flags + "," +
						domain.DomainGuid + "," + domain.DomainSid + "," + domain.ParentIndex);
				}
			}
		}
		
		private static void DisplayAdvancement(string data)
		{
			string value = "[" + DateTime.Now.ToLongTimeString() + "] " + data;
			Console.WriteLine(value);
			Trace.WriteLine(value);
		}
	}
}