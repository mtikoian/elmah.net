﻿namespace Elmah.Net.Logger.Data
{
	public interface ISiteInstance
	{
		string Name { get; }
		ISiteRecord Site { get; }
		string CreateSite(ISiteRecord site);
	}
}