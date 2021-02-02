//This class should be used to help write data context classes for unit tests.
//Unfortunately, we have some problem with using Castle nuget package in PX.Objects - it can not be correctly installed on feature building.

//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using Castle.DynamicProxy;
//using PX.Api;
//using PX.Data;
//using PX.DbServices;
//using PX.Objects.Common.Tools;

//namespace Common
//{
//	public class RepositoryDumpingInterceptor : IInterceptor
//	{
//		private const string basePath = @"c:\acumatica\RepositoryDumper\";

//		private static string[] _ignoredFields = new[]
//		{
//			"CreatedByID",
//			"CreatedByScreenID",
//			"CreatedDateTime",
//			"LastModifiedByID",
//			"LastModifiedByScreenID",
//			"LastModifiedDateTime",
//			"tstamp",
//			"DeletedDatabaseRecord"
//		};

//		public void Intercept(IInvocation invocation)
//		{
//			using (var writer = new StreamWriter(String.Concat(basePath, invocation.Method.Name, ".txt"), true))
//			{
//				writer.WriteLine(String.Format("New method call: {0}", DateTime.Now));
//				writer.WriteLine(String.Format("Arguments: {0}",
//					string.Join(", ", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray())));

//				invocation.Proceed();

//				var dump = invocation.ReturnValue.DumpForDataContext();

//				writer.WriteLine(dump);
//			}
//		}
//	}
//}
