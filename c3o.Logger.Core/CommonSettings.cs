﻿using System;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;
using System.Web.Security;

namespace c3o.Core
{
    public static class CommonSettings
    {
        public static string Application { get { return ConfigurationManager.AppSettings.Get("Site:Application"); } }

		public static string DatabaseName { get { return ConfigurationManager.AppSettings.Get("Site.DatabaseName"); } }

		public static string DeploymentPath { get { return ConfigurationManager.AppSettings.Get("Site.DeploymentPath"); } }

		// 0 = Clear / 1 = Encrypted / 2 = Hashed 
		public static MembershipPasswordFormat PasswordFormat { get { return ConfigurationManager.AppSettings.Get("Site:PasswordFormat").ParseEnum<MembershipPasswordFormat>(MembershipPasswordFormat.Hashed); } }
	}
}