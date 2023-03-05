﻿using System.Management;

namespace Kamera_Linearführung
{
    class Identifizierung
    {
        public static string macId()
        {
            return identifier("Win32_NetworkAdapterConfiguration", "MACAddress", "IPEnabled");
        }

        private static string identifier(string wmiClass, string wmiProperty, string wmiMustBeTrue)
        {
            string result = "";
            System.Management.ManagementClass mc = new System.Management.ManagementClass(wmiClass);
            System.Management.ManagementObjectCollection moc = mc.GetInstances();

            foreach (System.Management.ManagementObject mo in moc)
            {
                if (mo[wmiMustBeTrue].ToString() == "True")
                {
                    //Only get the first one
                    if (result == "")
                    {
                        try
                        {
                            result = mo[wmiProperty].ToString();
                            break;
                        }
                        catch
                        {
                        }
                    }
                }
            }
            return result;
        }

        //Return a hardware identifier
        private static string identifier(string wmiClass, string wmiProperty)
        {
            string result = "";
            System.Management.ManagementClass mc = new System.Management.ManagementClass(wmiClass);
            System.Management.ManagementObjectCollection moc = mc.GetInstances();
            foreach (System.Management.ManagementObject mo in moc)
            {
                //Only get the first one
                if (result == "")
                {
                    try
                    {
                        result = mo[wmiProperty].ToString();
                        break;
                    }
                    catch
                    {
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// BIOS IDentifier
        /// </summary>
        /// <returns></returns>
        public static string BIOS_ID()
        {
            return GetFirstIdentifier("Win32_BIOS", "Manufacturer")
                    + GetFirstIdentifier("Win32_BIOS", "SMBIOSBIOSVersion")
                    + GetFirstIdentifier("Win32_BIOS", "IdentificationCode")
                    + GetFirstIdentifier("Win32_BIOS", "SerialNumber")
                    + GetFirstIdentifier("Win32_BIOS", "ReleaseDate")
                    + GetFirstIdentifier("Win32_BIOS", "Version");
        }

        /// <summary>
        /// ManagementClass used to read the first specific properties
        /// </summary>
        /// <param name="wmiClass">Object Class to query</param>
        /// <param name="wmiProperty">Property to get info</param>
        /// <returns></returns>
        private static string GetFirstIdentifier(string wmiClass, string wmiProperty)
        {
            string result = string.Empty;
            ManagementClass mc = new System.Management.ManagementClass(wmiClass);
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                //Only get the first one
                if (string.IsNullOrEmpty(result))
                {
                    try
                    {
                        if (mo[wmiProperty] != null) result = mo[wmiProperty].ToString();
                        break;
                    }
                    catch
                    {
                    }
                }
            }
            return result.Trim();
        }
    }
}
