﻿# About

A2v10.AzureBlobEngine is a part of A2v10.Platform.


# How to use

```csharp
services.AddBlobStorages(factory =>
{
	factory.RegisterStorage<AzureBlobStorage>("AzureStorage");
});
```

# Related Packages

* [A2v10.Platform](https://www.nuget.org/packages/A2v10.Platform)

# Feedback

A2v10.AzureBlobEngine is released as open source under the MIT license.
Bug reports and contributions are welcome at the GitHub repository.