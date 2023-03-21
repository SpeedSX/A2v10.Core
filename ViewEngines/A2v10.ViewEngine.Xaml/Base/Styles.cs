﻿// Copyright © 2018 Alex Kukhtin. All rights reserved.

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace A2v10.Xaml;

public class Styles : Dictionary<String, Style>
{
	public Styles()
	{
	}

	protected Styles(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
