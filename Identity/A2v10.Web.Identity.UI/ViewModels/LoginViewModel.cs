﻿// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

using System;

namespace A2v10.Web.Identity.UI;

public class LoginViewModel : IIdentityViewModel
{
	public String? Login { get; set; }
	public String? Password { get; set; }
	public Boolean RememberMe { get; set; }
	public String? RequestToken { get; set; }

	public Boolean IsPersistent => RememberMe;

	public AppTitleModel? Title { get; init; }
	public String Theme { get; init; } = String.Empty;
	public String? ReturnUrl { get; init; }
}
