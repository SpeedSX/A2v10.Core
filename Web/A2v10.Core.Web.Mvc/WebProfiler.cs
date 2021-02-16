﻿
using A2v10.Data.Interfaces;
using A2v10.Infrastructure;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace A2v10.Core.Web.Mvc
{

	public class ProfileTimer
	{
		private Stopwatch _timer;

		[JsonProperty("elapsed")]
		public Int64 Elapsed { get; set; }

		protected ProfileTimer()
		{
			_timer = new Stopwatch();
			_timer.Start();
		}
		public void Stop()
		{
			if (_timer.IsRunning)
				_timer.Stop();
			Elapsed = _timer.ElapsedMilliseconds;
		}
	}

	public sealed class ProfileItem : ProfileTimer, IDisposable
	{
		[JsonProperty("text")]
		public String Text { get; set; }

		public ProfileItem(String msg)
			: base()
		{
			Text = msg;
		}

		public void Dispose()
		{
			Stop();
		}
	}

	internal class ProfileItems : List<ProfileItem>
	{
	}

	internal class ProfileRequest : ProfileTimer, IProfileRequest, IDisposable
	{
		public ProfileRequest(String address)
			: base()
		{
			Url = address;
			Items = new Dictionary<ProfileAction, ProfileItems>();
		}

		public void Dispose()
		{
			Stop();
		}

		[JsonProperty("url")]
		public String Url { get; set; }

		[JsonProperty("items")]
		public IDictionary<ProfileAction, ProfileItems> Items { get; set; }

		public IDisposable Start(ProfileAction kind, String description)
		{
			var itm = new ProfileItem(description);
			if (!Items.TryGetValue(kind, out ProfileItems elems))
			{
				elems = new ProfileItems();
				Items.Add(kind, elems);
			}
			elems.Add(itm);
			return itm;
		}
	}

	internal class DummyRequest : IProfileRequest
	{
		public IDisposable Start(ProfileAction kind, String description)
		{
			return null;
		}

		public void Stop()
		{
		}
	}

	public sealed class WebProfiler : IProfiler, IDataProfiler, IDisposable
	{
		const String SESSION_KEY = "_Profile_";
		const Int32 REQUEST_COUNT = 50;

		private LinkedList<ProfileRequest> _requestList;
		private ProfileRequest _request;

		public Boolean Enabled { get; set; }


		private readonly IHttpContextAccessor _httpContext;

		public WebProfiler(IHttpContextAccessor httpContext)
		{
			_httpContext = httpContext;
		}

		public void Dispose()
		{
			if (_request != null)
				_request.Dispose();
		}

		public IProfileRequest CurrentRequest => _request ?? new DummyRequest() as IProfileRequest;

		public IProfileRequest BeginRequest(String address, String session)
		{
			if (!Enabled)
				return null;
			if (address.ToLowerInvariant().EndsWith("/_shell/trace"))
				return null;
			LoadSession();
			_request = new ProfileRequest(address);
			_requestList.AddFirst(_request);
			while (_requestList.Count > REQUEST_COUNT)
				_requestList.RemoveLast();
			return _request;
		}

		public void EndRequest(IProfileRequest request)
		{
			if (request != _request)
				return;
			SaveSession();
		}

		void LoadSession()
		{
			var session = _httpContext.HttpContext.Session;
			var json = session.GetString(SESSION_KEY);
			if (String.IsNullOrEmpty(json))
				_requestList = new LinkedList<ProfileRequest>();
			else
				_requestList = JsonConvert.DeserializeObject<LinkedList<ProfileRequest>>(json);
		}

		void SaveSession()
		{
			String json = JsonConvert.SerializeObject(_requestList);
			var session = _httpContext.HttpContext.Session;
			session.SetString(SESSION_KEY, json);
		}

		public String GetJson()
		{
			var currentContext = _httpContext.HttpContext;
			if (currentContext == null)
				return null;
			var session = _httpContext.HttpContext.Session;
			var json = session.GetString(SESSION_KEY);
			if (String.IsNullOrEmpty(json))
				return null;
			return json;
		}

		#region IDataProfiler
		IDisposable IDataProfiler.Start(String command)
		{
			return CurrentRequest.Start(ProfileAction.Sql, command);
		}
		#endregion
	}
}
