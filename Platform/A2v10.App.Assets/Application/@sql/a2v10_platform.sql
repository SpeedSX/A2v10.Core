/*
Copyright © 2008-2023 Oleksandr Kukhtin

Last updated : 26 may 2023
module version : 8100
*/
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.SCHEMATA where SCHEMA_NAME=N'a2sys')
	exec sp_executesql N'create schema a2sys';
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.SCHEMATA where SCHEMA_NAME=N'a2security')
	exec sp_executesql N'create schema a2security';
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.SCHEMATA where SCHEMA_NAME=N'a2ui')
	exec sp_executesql N'create schema a2ui';
go
------------------------------------------------
grant execute on schema ::a2sys to public;
grant execute on schema ::a2security to public;
grant execute on schema ::a2ui to public;
go

------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'a2sys' and TABLE_NAME=N'SysParams')
create table a2sys.SysParams
(
	Name sysname not null constraint PK_SysParams primary key,
	StringValue nvarchar(255) null,
	IntValue int null,
	DateValue datetime null,
	GuidValue uniqueidentifier null
);
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.SEQUENCES where SEQUENCE_SCHEMA=N'a2security' and SEQUENCE_NAME=N'SQ_Tenants')
	create sequence a2security.SQ_Tenants as bigint start with 100 increment by 1;
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'a2security' and TABLE_NAME=N'Tenants')
create table a2security.Tenants
(
	Id	int not null constraint PK_Tenants primary key
		constraint DF_Tenants_PK default(next value for a2security.SQ_Tenants),
	[Admin] bigint null, -- admin user ID
	[Source] nvarchar(255) null,
	[TransactionCount] bigint not null constraint DF_Tenants_TransactionCount default(0),
	LastTransactionDate datetime null,
	UtcDateCreated datetime not null constraint DF_Tenants_UtcDateCreated default(getutcdate()),
	TrialPeriodExpired datetime null,
	DataSize float null,
	[State] nvarchar(128) null,
	UserSince datetime null,
	LastPaymentDate datetime null,
	[Locale] nvarchar(32) not null constraint DF_Tenants_Locale default('uk-UA')
);
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.SEQUENCES where SEQUENCE_SCHEMA=N'a2security' and SEQUENCE_NAME=N'SQ_Users')
	create sequence a2security.SQ_Users as bigint start with 100 increment by 1;
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'a2security' and TABLE_NAME=N'Users')
create table a2security.Users
(
	Id	bigint not null
		constraint DF_Users_PK default(next value for a2security.SQ_Users),
	Tenant int not null 
		constraint FK_Users_Tenant_Tenants foreign key references a2security.Tenants(Id),
	UserName nvarchar(255) not null constraint UNQ_Users_UserName unique,
	DomainUser nvarchar(255) null,
	Void bit not null constraint DF_Users_Void default(0),
	SecurityStamp nvarchar(max) not null,
	PasswordHash nvarchar(max) null,
	/*for .net core compatibility*/
	SecurityStamp2 nvarchar(max) null,
	PasswordHash2 nvarchar(max) null,
	TwoFactorEnabled bit not null constraint DF_Users_TwoFactorEnabled default(0),
	Email nvarchar(255) null,
	EmailConfirmed bit not null constraint DF_Users_EmailConfirmed default(0),
	PhoneNumber nvarchar(255) null,
	PhoneNumberConfirmed bit not null constraint DF_Users_PhoneNumberConfirmed default(0),
	LockoutEnabled	bit	not null constraint DF_Users_LockoutEnabled default(1),
	LockoutEndDateUtc datetimeoffset null,
	AccessFailedCount int not null constraint DF_Users_AccessFailedCount default(0),
	[Locale] nvarchar(32) not null constraint DF_Users_Locale2 default('uk-UA'),
	PersonName nvarchar(255) null,
	LastLoginDate datetime null, /*UTC*/
	LastLoginHost nvarchar(255) null,
	Memo nvarchar(255) null,
	ChangePasswordEnabled bit not null constraint DF_Users_ChangePasswordEnabled default(1),
	RegisterHost nvarchar(255) null,
	Segment nvarchar(32) null,
	SetPassword bit,
	UtcDateCreated datetime not null constraint DF_Users_UtcDateCreated default(getutcdate()),
	constraint PK_Users primary key (Tenant, Id)
);
go
------------------------------------------------
create or alter view a2security.ViewUsers
as
	select Id, UserName, DomainUser, PasswordHash, SecurityStamp, Email, PhoneNumber,
		LockoutEnabled, AccessFailedCount, LockoutEndDateUtc, TwoFactorEnabled, [Locale],
		PersonName, Memo, Void, LastLoginDate, LastLoginHost, Tenant, EmailConfirmed,
		PhoneNumberConfirmed, RegisterHost, ChangePasswordEnabled, Segment,
		SecurityStamp2, PasswordHash2, SetPassword
	from a2security.Users u
	where Void = 0 and Id <> 0;
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'a2ui' and TABLE_NAME=N'Modules')
create table a2ui.Modules
(
	Id uniqueidentifier not null,
	Parent uniqueidentifier null,
	[Name] nvarchar(255),
	[Memo] nvarchar(255),
	constraint PK_Modules primary key (Id),
	constraint FK_Modules_Parent_Modules foreign key (Parent) references a2ui.Modules(Id)
);
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'a2ui' and TABLE_NAME=N'TenantModules')
create table a2ui.TenantModules
(
	Tenant int not null,
	Module uniqueidentifier not null,
	UtcDateCreated datetime not null constraint 
		DF_TenantModules_UtcDateCreated default(getutcdate()),
	constraint PK_TenantModules primary key (Tenant, Module),
	constraint FK_TenantModules_Tenant_Tenants foreign key (Tenant) references a2security.Tenants(Id),
	constraint FK_TenantModules_Module_Modules foreign key (Module) references a2ui.Modules(Id)
);
go
-------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'a2ui' and TABLE_NAME=N'ModuleInitProcedures')
create table a2ui.[ModuleInitProcedures]
(
	[Procedure] sysname,
	Module  uniqueidentifier not null,
	Memo nvarchar(255),
	constraint PK_ModuleInitProcedures primary key (Module, [Procedure]),
	constraint FK_ModuleInitProcedures_Module_Modules foreign key (Module) references a2ui.Modules(Id)
);
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'a2ui' and TABLE_NAME=N'Menu')
create table a2ui.Menu
(
	Tenant int not null,
	Id uniqueidentifier not null,
	Module uniqueidentifier not null,
	Parent uniqueidentifier,
	[Name] nvarchar(255),
	[Url] nvarchar(255),
	CreateName nvarchar(255),
	CreateUrl nvarchar(255),
	Icon nvarchar(255),
	[Order] int not null constraint DF_Menu_Order default(0),
	[ClassName] nvarchar(255) null,
	constraint PK_Menu primary key (Tenant, Id),
	constraint FK_Menu_Parent_Menu foreign key (Tenant, Parent) references a2ui.Menu(Tenant, Id),
	constraint FK_Menu_Module_Modules foreign key (Module) references a2ui.Modules(Id),
	constraint FK_Menu_Tenant_Module_TenantModules foreign key (Tenant, Module) references a2ui.TenantModules(Tenant, Module)
);
go

/*
Copyright © 2008-2023 Oleksandr Kukhtin

Last updated : 24 may 2023
module version : 8100
*/
------------------------------------------------
create or alter procedure a2sys.[AppTitle.Load]
as
begin
	set nocount on;
	select [AppTitle], [AppSubTitle]
	from (select [Name], [Value] = StringValue from a2sys.SysParams) as s
		pivot (min(Value) for [Name] in ([AppTitle], [AppSubTitle])) as p;
end
go

------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.DOMAINS where DOMAIN_SCHEMA=N'a2sys' and DOMAIN_NAME=N'Id.TableType' and DATA_TYPE=N'table type')
create type a2sys.[Id.TableType] as table(
	Id bigint null
);
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.DOMAINS where DOMAIN_SCHEMA=N'a2sys' and DOMAIN_NAME=N'GUID.TableType' and DATA_TYPE=N'table type')
create type a2sys.[GUID.TableType] as table(
	Id uniqueidentifier null
);
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.DOMAINS where DOMAIN_SCHEMA=N'a2sys' and DOMAIN_NAME=N'NameValue.TableType' and DATA_TYPE=N'table type')
create type a2sys.[NameValue.TableType] as table(
	[Name] nvarchar(255),
	[Value] nvarchar(max)
);
go


/*
Copyright © 2008-2023 Oleksandr Kukhtin

Last updated : 24 may 2023
module version : 8100
*/
------------------------------------------------
create or alter procedure a2security.FindUserById
@Id bigint
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;

	select * from a2security.ViewUsers where Id=@Id;
end
go
------------------------------------------------
create or alter procedure a2security.FindUserByEmail
@Email nvarchar(255)
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;

	select * from a2security.ViewUsers where Email=@Email;
end
go
------------------------------------------------
create or alter procedure a2security.FindUserByName
@UserName nvarchar(255)
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;

	select * from a2security.ViewUsers where UserName=@UserName;
end
go
------------------------------------------------
create or alter procedure a2security.FindUserByPhoneNumber
@PhoneNumber nvarchar(255)
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;

	select * from a2security.ViewUsers where PhoneNumber=@PhoneNumber;
end
go
------------------------------------------------
create or alter procedure a2security.UpdateUserLogin
@Id bigint,
@LastLoginDate datetime,
@LastLoginHost nvarchar(255)
as
begin
	set nocount on;
	set transaction isolation level read committed;
	update a2security.ViewUsers set LastLoginDate = @LastLoginDate, LastLoginHost = @LastLoginHost 
	where Id=@Id;
end
go
------------------------------------------------
create or alter procedure a2security.UpdateUserLockout
@Id bigint,
@AccessFailedCount int,
@LockoutEndDateUtc datetimeoffset
as
begin
	set nocount on;
	set transaction isolation level read committed;

	update a2security.ViewUsers set 
		AccessFailedCount = @AccessFailedCount, LockoutEndDateUtc = @LockoutEndDateUtc
	where Id=@Id;
end
go

------------------------------------------------
create or alter procedure a2security.[User.SetPasswordHash]
@Id bigint,
@PasswordHash nvarchar(max)
as
begin
	set nocount on;
	set transaction isolation level read committed;

	update a2security.ViewUsers set PasswordHash2 = @PasswordHash where Id = @Id;
end
go
------------------------------------------------
create or alter procedure a2security.[User.SetSecurityStamp]
@Id bigint,
@SecurityStamp nvarchar(max)
as
begin
	set nocount on;
	set transaction isolation level read committed;

	update a2security.ViewUsers set SecurityStamp2 = @SecurityStamp where Id = @Id;
end
go
------------------------------------------------
create or alter procedure a2security.[User.SetPhoneNumberConfirmed]
@Id bigint,
@PhoneNumber nvarchar(255),
@Confirmed bit
as
begin
	set nocount on;
	set transaction isolation level read committed;
	update a2security.ViewUsers set PhoneNumber = @PhoneNumber, PhoneNumberConfirmed = @Confirmed where Id = @Id;
end
go


/*
Copyright © 2008-2023 Oleksandr Kukhtin

Last updated : 26 may 2023
module version : 8100
*/
------------------------------------------------
create or alter procedure a2ui.[Menu.User.Load]
@TenantId int = 1,
@UserId bigint
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;

	select * from a2security.ViewUsers where Id = @UserId;
end
go
------------------------------------------------
drop procedure if exists a2ui.[Menu.Merge];
drop type if exists a2ui.[Menu.TableType]
go
------------------------------------------------
create type a2ui.[Menu.TableType] as table
(
	Id uniqueidentifier,
	Parent uniqueidentifier,
	[Name] nvarchar(255),
	[Url] nvarchar(255),
	Icon nvarchar(255),
	[Order] int,
	ClassName nvarchar(255),
	CreateName nvarchar(255),
	CreateUrl nvarchar(255)
);
go

------------------------------------------------
create or alter procedure a2ui.[Menu.Merge]
@TenantId int,
@Menu a2ui.[Menu.TableType] readonly,
@ModuleId uniqueidentifier
as
begin
	set nocount on;
	set transaction isolation level read committed;

	with T as (
		select * from a2ui.Menu where Tenant = @TenantId
	)
	merge T as t
	using @Menu as s
	on t.Id = s.Id and t.Tenant = @TenantId
	when matched then update set
		t.Id = s.Id,
		t.Parent = s.Parent,
		t.[Name] = s.[Name],
		t.[Url] = s.[Url],
		t.[Icon] = s.Icon,
		t.[Order] = s.[Order],
		t.ClassName = s.ClassName,
		t.CreateUrl= s.CreateUrl,
		t.CreateName = s.CreateName
	when not matched by target then insert(Module, Tenant, Id, Parent, [Name], [Url], Icon, [Order], ClassName, CreateUrl, CreateName) values 
		(@ModuleId, @TenantId, Id, Parent, [Name], [Url], Icon, [Order], ClassName, CreateUrl, CreateName)
	when not matched by source and t.Tenant = @TenantId and t.Module = @ModuleId then
		delete;
end
go

------------------------------------------------
create or alter procedure a2ui.[Menu.User.Load]
@TenantId int = 1,
@UserId bigint = null
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;

	declare @RootId uniqueidentifier = N'00000000-0000-0000-0000-000000000000';
	with RT as (
		select Id=m0.Id, ParentId = m0.Parent, [Level] = 0
			from a2ui.Menu m0
			where m0.Tenant = @TenantId and m0.Id = @RootId
		union all
		select m1.Id, m1.Parent, RT.[Level]+1
			from RT inner join a2ui.Menu m1 on m1.Parent = RT.Id and m1.Tenant = @TenantId
	)
	select [Menu!TMenu!Tree] = null, [Id!!Id]=RT.Id, [!TMenu.Menu!ParentId]=RT.ParentId,
		[Menu!TMenu!Array] = null,
		m.[Name], m.Url, m.Icon, m.ClassName, m.CreateUrl, m.CreateName
	from RT 
		inner join a2ui.Menu m on m.Tenant = @TenantId and RT.Id=m.Id
	order by RT.[Level], m.[Order], RT.[Id];

	-- system parameters
	select [SysParams!TParam!Object]= null, [AppTitle], [AppSubTitle]
	from (select [Name], [Value]=StringValue from a2sys.SysParams) as s
		pivot (min([Value]) for [Name] in ([AppTitle], [AppSubTitle])) as p;
end
go
------------------------------------------------
create or alter procedure a2ui.[RegisterModule]
@ModuleId uniqueidentifier,
@Name nvarchar(255)
as
begin
	if not exists(select * from a2ui.Modules where [Id] = @moduleId)
		insert into a2ui.Modules ([Id], [Name]) values (@moduleId, @Name);
	else
		update a2ui.Modules set [Name] = @Name where [Id] = @moduleId;
end
go
------------------------------------------------
create or alter procedure a2ui.[Tenant.ConnectModule]
@ModuleId uniqueidentifier,
@TenantId int
as
begin
	if not exists(select * from a2ui.TenantModules where Tenant = @TenantId and Module = @ModuleId)
		insert into a2ui.TenantModules(Tenant, Module) values (@TenantId, @ModuleId);
end
go
------------------------------------------------
create or alter procedure a2ui.[RegisterInitProcedure]
@Module uniqueidentifier,
@Procedure sysname
as
begin 
	set nocount on;
	set transaction isolation level read committed;

	if not exists(select * from a2ui.ModuleInitProcedures where [Procedure] = @Procedure and Module = @Module)
		insert into a2ui.ModuleInitProcedures(Module, [Procedure]) values (@Module, @Procedure);
end
go

------------------------------------------------
create or alter procedure a2ui.[InvokeInitProcedures]
@TenantId int
as
begin 
	set nocount on;
	set transaction isolation level read committed;

	declare @procName sysname;
	declare @moduleId uniqueidentifier;
	declare @prms nvarchar(255);
	declare @sql nvarchar(255);
	set @prms = N'@TenantId int, @ModuleId uniqueidentifier';
	declare #crs cursor local fast_forward read_only for
		select [Procedure], tm.Module from a2ui.TenantModules tm
		inner join a2ui.ModuleInitProcedures mip on tm.Module = mip.Module
		where tm.Tenant = @TenantId
		group by [Procedure], tm.[Module];
	open #crs;
	fetch next from #crs into @procName, @moduleId;
	while @@fetch_status = 0
	begin
		set @sql = N'exec ' + @procName + N' @TenantId = @TenantId, @ModuleId = @ModuleId';
		exec sp_executesql @sql, @prms, @TenantId, @moduleId;
		fetch next from #crs into @procName,@moduleId;
	end
	close #crs;
	deallocate #crs;
end
go



/*
Copyright © 2008-2023 Oleksandr Kukhtin

Last updated : 24 may 2023
module version : 8100
*/


------------------------------------------------
if not exists(select * from a2security.Tenants where Id <> 0)
	insert into a2security.Tenants(Id) values (1);
go
------------------------------------------------
if not exists(select * from a2security.Tenants where Id = 0)
	insert into a2security.Tenants(Id) values (0);
go
------------------------------------------------
if not exists(select * from a2security.Users)
begin
	set nocount on;
	set transaction isolation level read committed;

	insert into a2security.Users(Id, Tenant, UserName, SecurityStamp, PasswordHash, PersonName, EmailConfirmed)
	values (99, 1, N'admin@admin.com', N'c9bb451a-9d2b-4b26-9499-2d7d408ce54e', N'AJcfzvC7DCiRrfPmbVoigR7J8fHoK/xdtcWwahHDYJfKSKSWwX5pu9ChtxmE7Rs4Vg==',
		N'System administrator', 1);
end
go
