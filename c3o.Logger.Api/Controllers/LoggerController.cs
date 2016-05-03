﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Elmah.Io.Client;
using c3o.Logger.Data;

namespace c3o.Logger.Web
{
	[RoutePrefix("api.logger")]
	public class LoggerController : ApiController
	{
		[HttpGet]
		[Route("messages/{id}")]
		public LogMessage Detail(long id, HydrationLevel level = HydrationLevel.Detailed)
		{
			using (LoggerContext db = new LoggerContext())
			{
				var message = db.LogMessages.Where(x => x.Id == id)
						.Include(x => x.Log)
						.Include(x => x.User)
						.Include(x => x.Source)
						.Include(x => x.MessageType)
						.Include(x => x.Application)
						.Include(x => x.Detail)
						.FirstOrDefault();

				return new LogMessage(message, level);
			}			
		}

		[HttpGet]
		[Route("messages/search/{log:alpha?}")]
		public LogSearchResponseModel Search(string log=null, string application = null
            , Elmah.Io.Client.Severity? severity = null
            //, string type = null
            //, string source = null
            //,string user = null
            , List<long> logs = null
            , List<long> applications = null
            , List<long> types = null
			,List<long> sources = null
			,List<long> users = null
			,DateTime? start = null
			,DateTime? end = null
            , SearchSpan span = SearchSpan.All
            , int limit = 10
            , HydrationLevel level = HydrationLevel.Basic)
        { 
            using (LoggerContext db = new LoggerContext())
            {
                // convert to utc
                if (start.HasValue) start.Value.ToUniversalTime();
                if (end.HasValue) end.Value.ToUniversalTime();

                // convert to span
                if (!start.HasValue && span > 0)
                {
                    start = DateTime.UtcNow.AddMinutes((int)span * -1);
                }

                IQueryable<c3o.Logger.Data.LogMessage> messages = null;

                if (start.HasValue)
                {
                    if (end.HasValue)
                    {
                        messages = (IQueryable<c3o.Logger.Data.LogMessage>)db.LogMessages.Where(x => x.DateTime >= start && x.DateTime <= end)
                            .Include(x => x.Log)
                            .Include(x => x.User)
                            .Include(x => x.Source)
                            .Include(x => x.MessageType)
                            .Include(x => x.Application)
                            .OrderByDescending(x => x.DateTime);
                    }
                    else
                    {
                        messages = (IQueryable<c3o.Logger.Data.LogMessage>)db.LogMessages.Where(x => x.DateTime >= start)
                            .Include(x => x.Log)
                            .Include(x => x.User)
                            .Include(x => x.Source)
                            .Include(x => x.MessageType)
                            .Include(x => x.Application)
                            .OrderByDescending(x => x.DateTime);
                    }
                }
                else
                {
                    messages = (IQueryable<c3o.Logger.Data.LogMessage>)db.LogMessages
                            .Include(x => x.Log)
                            .Include(x => x.User)
                            .Include(x => x.Source)
                            .Include(x => x.MessageType)
                            .Include(x => x.Application)
                            .OrderByDescending(x => x.DateTime);
                }

                //Log theLog = null;
                if (!string.IsNullOrWhiteSpace(log))
                {
                    //var start = DateTime.Now.Date;
                    var allLogs = (IQueryable<c3o.Logger.Data.Log>)db.Logs.OrderBy(x => x.Name);
                    var theLog = allLogs.FirstOrDefault(x => x.Name == log);
                    if (theLog != null)
                    {
                        messages = messages.Where(x => x.LogId == theLog.Id);
                    }
                }

                if (logs != null && logs.Any())
                {
                    messages = messages.Where(x => logs.Any(y => y == x.LogId));
                }


                // application
                if (!string.IsNullOrWhiteSpace(application))
                {
                    var obj = db.LogApplications.FirstOrDefault(x => x.Name == application);
                    if (obj != null)
                    {
                        messages = messages.Where(x => x.ApplicationId == obj.Id);
                        //logs = logs.Where(x => x.Applications.Any(a => a.Id == obj.Id));						
                    }
                }

                // applications
                if (applications != null && applications.Any())
                {
                    //var list = types.Select(x => x.Id);
                    messages = messages.Where(x => applications.Any(y => y == x.ApplicationId));
                }

                //// types
                //if (!string.IsNullOrWhiteSpace(type))
                //{
                //    var obj = db.MessageTypes.FirstOrDefault(x => x.Name == type);
                //    if (obj != null)
                //    {
                //        messages = messages.Where(x => x.MessageTypeId == obj.Id);
                //    }
                //}

                if (types != null && types.Any())
                {
                    //var list = types.Select(x => x.Id);
                    messages = messages.Where(x => types.Any(y => y == x.MessageTypeId));
                }

                if (sources != null && sources.Any())
                {
                    //var list = sources.Select(x => x.Id);
                    messages = messages.Where(x => sources.Any(y => y == x.SourceId));
                }

                if (users != null && users.Any())
                {
                    //var list = users.Select(x => x.Id);
                    messages = messages.Where(x => users.Any(y => y == x.UserId));
                }

                //// sources
                //if (!string.IsNullOrWhiteSpace(source))
                //{
                //    var obj = db.MessageSources.FirstOrDefault(x => x.Name == source);
                //    if (obj != null)
                //    {
                //        messages = messages.Where(x => x.SourceId == obj.Id);
                //    }
                //}


                //// users
                //if (!string.IsNullOrWhiteSpace(user))
                //{
                //    var obj = db.Users.FirstOrDefault(x => x.Name == user);
                //    if (obj != null)
                //    {
                //        messages = messages.Where(x => x.UserId == obj.Id);
                //    }
                //}

                // severity
                if (severity.HasValue) { messages = messages.Where(x => x.Severity == severity); }

                if (limit > 0)
                {
                    messages = messages.Take(limit);
                }

                // Setup model
                var model = new LogSearchResponseModel(messages.ToList(), level);
    //            if (theLog != null) { model.Log = theLog.Name; }
				//model.Application = application;
				//model.Severity = severity;
				//model.Source = source;
				//model.Span = span;
				//model.Type = type;
				//model.User = user;
				return model;
			}
		}
	}
}