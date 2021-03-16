﻿using A2v10.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.Infrastructure
{
	public class RenderInfo : IRenderInfo
	{
		public String RootId { get; init; }
		public String FileName { get; init; }
		public String FileTitle { get; init; }
		public String Path { get; init; }
		public String Text { get; init; }
		public IDataModel DataModel { get; init; }
		//public ITypeChecker TypeChecker
		public String CurrentLocale { get; init; }
		public Boolean IsDebugConfiguration { get; init; }
		public Boolean SecondPhase { get; init; }
	}
}
