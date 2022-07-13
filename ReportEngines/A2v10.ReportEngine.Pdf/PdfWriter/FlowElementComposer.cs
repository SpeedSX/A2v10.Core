﻿// Copyright © 2022 Oleksandr Kukhtin. All rights reserved.

using QuestPDF.Infrastructure;

namespace A2v10.ReportEngine.Pdf;

internal abstract class FlowElementComposer
{
	internal abstract void Compose(IContainer container);
}
