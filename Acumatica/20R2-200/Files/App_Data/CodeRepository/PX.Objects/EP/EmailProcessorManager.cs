using System;
using System.Collections.Generic;
using PX.Common.Service;
using PX.Data;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.EP
{
	public class EmailProcessEventArgs
	{
		private readonly PXGraph _graph;
		private readonly EMailAccount _account;
		private readonly CRSMEmail _message;

		private bool _isSuccessful;

		public EmailProcessEventArgs(PXGraph graph, EMailAccount account, CRSMEmail message)
		{
			if (graph == null) throw new ArgumentNullException("graph");
			if (account == null) throw new ArgumentNullException("account");
			if (message == null) throw new ArgumentNullException("message");

			_graph = graph;
			_account = account;
			_message = message;
		}

		public CRSMEmail Message
		{
			get { return _message; }
		}

		public PXGraph Graph
		{
			get { return _graph; }
		}

		public EMailAccount Account
		{
			get { return _account; }
		}

		public bool IsSuccessful
		{
			get 
			{
				return _isSuccessful;
			}
			set 
			{
				_isSuccessful |= value;
			}
		}
	}

	public interface IEmailProcessor
	{
		void Process(EmailProcessEventArgs e);
	}

	public class EmailProcessorManager
	{
		private static readonly MultiHandler<IEmailProcessor> _handlers;

		static EmailProcessorManager()
		{
			_handlers = new MultiHandler<IEmailProcessor>();
		}

		public static void Clear()
		{
			_handlers.Clear();
		}

		public static void Register(IEmailProcessor handler)
		{
			if (handler == null) throw new ArgumentNullException("handler");

			_handlers.Register(handler);
		}

		public static IEnumerable<IEmailProcessor> Handlers
		{
			get { return _handlers.Handlers; }
		}
	}
}
