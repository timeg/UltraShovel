if (Settings){	Settings.Unload();	} 

Settings = {
	// private settings for developers
	AmmoUsageDelay = 10,
	RepeatSound = 2,
	UpdateInventoryTime = 10,
	// public settings for user
	
	Family = {
		AutoItems = {
			[1] = { Num = 0, InUse = false, Time = 30 },
			[2] = { Num = 1, InUse = false, Time = 30 },
			[3] = { Num = 2, InUse = false, Time = 30 },
			[4] = { Num = 3, InUse = false, Time = 30 },
			[5] = { Num = 4, InUse = false, Time = 30 },
			[6] = { Num = 5, InUse = false, Time = 30 },
			[7] = { Num = 6, InUse = false, Time = 30 },
		},
		CallAtPartyBuff = true,
		UseSimpleAlarm = true,
		KeepRange = 1000,
		PickRange = 1000,
		ReportAlarm = false,
		InventoryAlarm = false,
		ReportAlarmSound = "Animal_Chicken",
		InventoryAlarmSound = "Club_Animals",
		DeadAlarmSound = "Club_Animals",
		ReturnTime = 10,
		CallTime = 5,
		Mode = 0,
		Language = "rus"
	}
}

Settings.Unload = func (){
	Settings = nil;
}

// Method writes current settings to file
Settings.Save = func(){
	var fileName = Shovel.AiFolder .. Shovel.FSP .. Shovel.SettingsFolder .. Shovel.FSP .. 'Family_' .. Shovel.Commander;
	System.SaveTableToFile(Settings.Family, fileName);
	if (Shovel.IsDebug) SysMsg("Settings saved");
}

// Method reads settings for current family if exists
Settings.Load = func(){
	var fileName = Shovel.AiFolder .. Shovel.FSP .. Shovel.SettingsFolder .. Shovel.FSP .. 'Family_' .. Shovel.Commander;
	var settings = System.LoadTableFromFile(fileName);
	if (settings != nil){
		Settings.Family = settings;
		if (Shovel.IsDebug) SysMsg('Settings loaded for family ' .. Shovel.Commander);
	}
}

Settings.ChangeLanguage = func(){
	Interface.CloseAll();
	Interface = nil;
	Localization = nil;
	System.LoadFile( Shovel.AiFolder .. Shovel.FSP .. Shovel.ModulesFolder .. Shovel.FSP .. 'localization' .. Shovel.Ext );
	System.LoadFile( Shovel.AiFolder .. Shovel.FSP .. Shovel.ModulesFolder .. Shovel.FSP .. 'interface' .. Shovel.Ext );
	if (Shovel.IsDebug) SysMsg('Language changed');
}

Settings.Load();