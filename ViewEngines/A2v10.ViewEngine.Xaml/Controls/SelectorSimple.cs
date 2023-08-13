﻿// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Xaml;

public class SelectorSimple : Selector
{

	public String? Url { get; set; }
	public String? Data { get; set; }

	protected override void OnEndInit()
	{
		base.OnEndInit();

		if (AddOns.Count > 0)
		{
			for (int i = 0; i < AddOns.Count; i++)
			{
				var command = AddOns[i].GetBindingCommand(nameof(Command));
				if (command != null)
				{
					if (command.Command == CommandType.Browse && command.Url == $"{Url}/browse")
						return;
				}
            }
        }

		if (String.IsNullOrEmpty(DisplayProperty))
			DisplayProperty = "Name";

		Fetch = $"{Url}/fetch";
		var hlink = new Hyperlink()
		{
			Icon = Icon.Search,
		};
		var cmd = new BindCmd()
		{
			Command = CommandType.Browse,
			Url = $"{Url}/browse",
		};
		var val = this.GetBinding(nameof(Value));
		if (val != null)
			cmd.BindImpl.SetBinding("Argument", new Bind() { Path = val.Path });
		var dat = this.GetBinding("Data");
		if (dat != null)
		{
			cmd.BindImpl.SetBinding("Data", dat);
			this.BindImpl.SetBinding(nameof(FetchData), dat);
		}
		hlink.BindImpl.SetBinding("Command", cmd);
		AddOns.Insert(0, hlink);
	}
}
