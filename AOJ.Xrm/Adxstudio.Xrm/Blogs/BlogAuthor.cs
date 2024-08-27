/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using Microsoft.Xrm.Portal.Web;

namespace Adxstudio.Xrm.Blogs
{
	public class BlogAuthor : IBlogAuthor
	{
		public BlogAuthor(Guid id, string name, string emailAddress, ApplicationPath applicationPath)
		{
			Id = id;
			Name = name;
			EmailAddress = emailAddress;
			ApplicationPath = applicationPath;
		}

		public ApplicationPath ApplicationPath { get; }

		public string EmailAddress { get; }

		public Guid Id { get; }

		public string Name { get; }
	}
}
