﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BPUtil;
using Newtonsoft.Json;

namespace AcuRiteSniffer
{
	public class Settings : SerializableObjectBase
	{
		public string smartHubIp = "192.168.0.135";
		public string accessIpsSemicolonSeparated = "192.168.0.171";
		public ushort myWebPort = 45411;
		/// <summary>
		/// If -1, we won't listen with https.
		/// </summary>
		public int myHttpsPort = 443;
		public int myNetworkInterfaceIndex = 0;
		public bool easyParseMethod = true;
		public string serviceName = "AcuRiteSniffer";
		public string sensorDataFiles = "[24C86E000000_12345678]Temperature: ##tempf##°\\nHumidity: ##humidity##\\nAt ##date## ##time##"
			+ Environment.NewLine + "[24C86E000000_00001234=myWindSpeed.txt]##windspeedmph## MPH"
			+ Environment.NewLine + "[24C86E000000_00001234=myWindDirection.txt]##winddir##";

		private static Regex rxReadSensorDataFileLine = new Regex("\\[(.*?)\\](.*)", RegexOptions.Compiled);
		private List<DataFileTemplate> templates;

		public List<DataFileTemplate> GetSensorDataTemplates()
		{
			if (templates == null)
			{
				templates = new List<DataFileTemplate>();
				if (sensorDataFiles != null)
					foreach (string line in sensorDataFiles.Split(new char[] { '\r', '\n' }))
					{
						Match m = rxReadSensorDataFileLine.Match(line);
						if (m.Success)
							templates.Add(new DataFileTemplate(m.Groups[1].Value, m.Groups[2].Value));
					}
			}
			return templates;
		}

		private HashSet<string> _accessIps;

		public HashSet<string> GetAcuriteAccessIPs()
		{
			if (_accessIps == null)
			{
				_accessIps = new HashSet<string>();
				if (accessIpsSemicolonSeparated != null)
					foreach (string addressStr in accessIpsSemicolonSeparated.Split(new char[] { ';' }))
					{
						if (IPAddress.TryParse(addressStr, out IPAddress address))
							_accessIps.Add(address.ToString());
					}
			}
			return _accessIps;
		}
	}
	public class DataFileTemplate
	{
		public string UniqueID;
		public string FileName;
		public string TemplateStr;

		public DataFileTemplate(string identifier, string templateStr)
		{
			int idxEqualSign = identifier.IndexOf('=');
			if (idxEqualSign == -1)
			{
				UniqueID = identifier;
				FileName = UniqueID + ".txt";
			}
			else
			{
				UniqueID = identifier.Remove(idxEqualSign);
				FileName = identifier.Substring(idxEqualSign + 1);
			}
			TemplateStr = templateStr;
		}
		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
