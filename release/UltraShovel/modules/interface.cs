if (Interface){	Interface.Unload();	} 

Interface = {
		StancesIndexes = { [1] = 1, [2] = 1, [3] = 1 },
		SkillsWindow = 'skills' .. Settings.Family.Language,
		PotionsWindow = 'potions' .. Settings.Family.Language,
		SettingsWindow = 'settings' .. Settings.Family.Language,
		AlarmsWindow = 'alarms' .. Settings.Family.Language,
		MenuWindow = 'menu' .. Settings.Family.Language,
		ItemsWindow = 'items' .. Settings.Family.Language,
		AddonsWindow = 'addons' .. Settings.Family.Language,
		AboutWindow = 'about' .. Settings.Family.Language
	}	

Interface.Load = func(){
	Close('accountinfo');
	Interface.LoadWindow('menu');
	var text = Shovel.Commander .. ', ' .. Text['WelcomeMessage'] .. '\n';
	text = text .. Text['Ver'] .. ': ' .. Shovel.AIVersion .. '\n';
	text = text .. Text['Region'] .. ': ' .. Shovel.Region .. '\n\n\n';
	text = text .. Text['AutoKeep'] .. '\n';
	text = text .. 'Чтобы сменить язык, перейдите в Настройки.\n';
	text = text .. 'If you want to change language, then go to Settings.\n';
	var uiFrame = GetFrameByName(Interface.MenuWindow);
	if (uiFrame){	
		SetTextByKey(uiFrame, 'l', text);
	}
	if (Shovel.IsDebug) SysMsg("Interface loaded");
}

Interface.Unload = func(){
	Interface = nil;
}

// Close all UltraShovel windows	
Interface.CloseAll = func(){
	Close(Interface.SkillsWindow);
	Close(Interface.PotionsWindow);
	Close(Interface.SettingsWindow);
	Close(Interface.AlarmsWindow);
	Close(Interface.MenuWindow);
	Close(Interface.ItemsWindow);
	Close(Interface.AddonsWindow);
	Close(Interface.AboutWindow);
}

Interface.ShowMini = func(message){
	Open('accountinfo');
	var frame = GetFrameByName('accountinfo');
	if (!frame) return;
	SetTextByKey(frame, 'msg1', message);
}

// Load and open one of UltraShovel window
Interface.LoadWindow = func(window){
	Interface.CloseAll();	
	if (window == 'menu') Interface.LoadMenuWindow();
	else if (window == 'skills') Interface.LoadSkillsWindow();
	else if (window == 'potions') Interface.LoadPotionsWindow();
	else if (window == 'alarms') Interface.LoadAlarmsWindow();
	else if (window == 'settings') Interface.LoadSettingsWindow();
	else if (window == 'items') Interface.LoadItemsWindow();
	else if (window == 'addons') Interface.LoadAddonsWindow();
	else if (window == 'about') Interface.LoadAboutWindow();
	else SysMsg("Something went wrong with frames");
}

// Controls

Interface.SetCheckBox = func(frame, checkBoxName, value){
	var uiCheckBox = GetCheckBox(frame, checkBoxName);
	var newValue = 0;
	if (value) newValue = 1;
	SetCheck(uiCheckBox, newValue);	
}

// Loads menu window
Interface.LoadMenuWindow = func(){
	Open(Interface.MenuWindow);
	var uiFrame = GetFrameByName(Interface.MenuWindow);
	if (!uiFrame) return;

	// set text for labels
	SetTextByKey(uiFrame, 'as' , Text['AutoSkills']);
	SetTextByKey(uiFrame, 'ai' , Text['Potions']);
	SetTextByKey(uiFrame, 'al' , Text['Alarms']);
	SetTextByKey(uiFrame, 'se' , Text['Settings']);	
	SetTextByKey(uiFrame, 'it' , Text['Items']);
	SetTextByKey(uiFrame, 'ad' , Text['Addons']);
	SetTextByKey(uiFrame, 'ab' , Text['About']);
	SetTextByKey(uiFrame, 'lo' , Text['LogWin']);
}

// === SETTINGS WINDOW ===

// Loads settings window
Interface.LoadSettingsWindow = func(){
	// stop display settings if characters not initialized
	if (!Characters.AreInitialized) return;
	Open(Interface.SettingsWindow);
	
	var uiFrame = GetFrameByName(Interface.SettingsWindow);
	if (!uiFrame) return;
	for (i = 1, Characters.Count()){	
		SetTextByKey(uiFrame, 'n' .. i , Characters[i].Name);	
		Interface.SetCheckBox(uiFrame, 'AutoPickChar' .. i, Characters[i].Settings.AutoPick);			
		Interface.SetCheckBox(uiFrame, 'AutoAttack' .. i, Characters[i].Settings.AutoAttack);
	}	
	// set text
	SetTextByKey(uiFrame, 'kv' , Settings.Family.KeepRange);
	SetTextByKey(uiFrame, 'pv' , Settings.Family.PickRange);
	SetTextByKey(uiFrame, 'k' , Text['KeepRange']);
	SetTextByKey(uiFrame, 'p' , Text['PickRange']);	
	SetTextByKey(uiFrame, 'a', Text['AutoAttack']);
	SetTextByKey(uiFrame, 'm', Text['Mode']);
	SetTextByKey(uiFrame, 'capb', Text['CallAtBuff']);
	SetTextByKey(uiFrame, 'lang', Text['Language']);
	SetTextByKey(uiFrame, 'ru', Text['Rus']);
	SetTextByKey(uiFrame, 'en', Text['Eng']);
	
	if (Settings.Family.Mode == 0){
		SetTextByKey(uiFrame, 're', Text['ReturnEach']);
		SetText(uiFrame, "eac", Settings.Family.ReturnTime);
	} else {
		SetTextByKey(uiFrame, 're', Text['CallEach']);
		SetText(uiFrame, "eac", Settings.Family.CallTime);
	}
	
	// handle language	
	var rusLang = false;
	if (Settings.Family.Language == "rus"){
		rusLang = true;
	}
				
	Interface.SetCheckBox(uiFrame, 'rus', rusLang);		
	Interface.SetCheckBox(uiFrame, 'eng', !rusLang);		
	Interface.SetCheckBox(uiFrame, 'CallAtPartyBuff', Settings.Family.CallAtPartyBuff);
}

// Change mode (stay and farm / follow leader)
// only from interface
Interface.SetMode = func(){	
	var uiFrame = GetFrameByName(Interface.SettingsWindow);
	if (!uiFrame) return;	
	var uiControl = GetControl(uiFrame, 'select_mode');
	var ctlLevel = GetNumber(uiControl);	
	Settings.Family.Mode = ctlLevel;
	if (Settings.Family.Mode == 0){
		SetTextByKey(uiFrame, 're', Text['ReturnEach']);
		SetText(uiFrame, "eac", Settings.Family.ReturnTime);
		if (Shovel.IsDebug) SysMsg('Mode has been changed to [Stay and farm]');
	} else {
		SetTextByKey(uiFrame, 're', Text['CallEach']);
		SetText(uiFrame, "eac", Settings.Family.CallTime);
		if (Shovel.IsDebug) SysMsg('Mode has been changed to [Follow leader]');
	}
}

Interface.UpdateSettingsWindow = func(){
	var uiFrame = GetFrameByName(Interface.SettingsWindow);
	if (!uiFrame) return;

	SetTextByKey(uiFrame, 'kv' , Settings.Family.KeepRange);
	SetTextByKey(uiFrame, 'pv' , Settings.Family.PickRange);	
}

// === ALARMS WINDOW ===

// Loads alarms window
Interface.LoadAlarmsWindow = func(){
	Open(Interface.AlarmsWindow);
	var uiFrame = GetFrameByName(Interface.AlarmsWindow);
	
	if (!uiFrame) return;
	for (i = 1, Characters.Count()) 
	{
		SetTextByKey(uiFrame, 'n' .. i , Characters[i].Name);
		Interface.SetCheckBox(uiFrame, 'DeadAlarmChar' .. i, Characters[i].Settings.DeadAlarm);
	}
	
	SetTextByKey(uiFrame, 'ra' , Text['ReportAlarm']);
	SetTextByKey(uiFrame, 'ia' , Text['InventoryAlarm']);
	SetTextByKey(uiFrame, 'da' , Text['DeadAlarm']);
	SetTextByKey(uiFrame, 'sa' , Text['UseSimpleAlarm']);
	SetTextByKey(uiFrame, 'sa2' , Text['UseSimpleAlarm2']);
	
	SetText(uiFrame, "ReportAlarmSound", Settings.Family.ReportAlarmSound);
	SetText(uiFrame, "InventoryAlarmSound", Settings.Family.InventoryAlarmSound);
	SetText(uiFrame, "DeadAlarmSound", Settings.Family.DeadAlarmSound);
	
	Interface.SetCheckBox(uiFrame, 'usa', Settings.Family.UseSimpleAlarm);
	Interface.SetCheckBox(uiFrame, 'RA', Settings.Family.ReportAlarm);
	Interface.SetCheckBox(uiFrame, 'IA', Settings.Family.InventoryAlarm);
}

// === AUTO-POTIONS WINDOW ===

// Loads potions window
Interface.LoadPotionsWindow = func(){
	Open(Interface.PotionsWindow);
	var uiFrame = GetFrameByName(Interface.PotionsWindow);
	if (!uiFrame) return;
	for (i = 1, Characters.Count()) 
	{
		SetTextByKey(uiFrame, 'n' .. i , Characters[i].Name);
		SetTextByKey(uiFrame, 'h' .. i , Text['UseHP']);
		SetTextByKey(uiFrame, 'm' .. i , Text['UseMP']);
		SetTextByKey(uiFrame, 'sh' .. i , Text['HPlvl']);
		SetTextByKey(uiFrame, 'sm' .. i , Text['MPlvl']);				
		Interface.UpdatePotionsWindow(i);
	}	
}

// Updates potions window during moving slider
Interface.UpdatePotionsWindow = func(index){
	var uiFrame = GetFrameByName(Interface.PotionsWindow);
	if (!uiFrame) return;
	// calculate sliders values
	var percentHP = Characters[index].MaxHP / 100;				
	var currentHP = math.floor(Characters[index].Settings.HPlevel * percentHP) .. ' HP';		
	var percentSP = Characters[index].MaxSP / 100;		
	var currentSP = math.floor(Characters[index].Settings.SPlevel * percentSP) .. ' MP';
	
	SetTextByKey(uiFrame, 'hl' .. index , currentHP);
	SetTextByKey(uiFrame, 'ml' .. index , currentSP);	
		
	Interface.SetCheckBox(uiFrame, 'useHP' .. index, Characters[index].Settings.AutoHP);
	Interface.SetCheckBox(uiFrame, 'useMP' .. index, Characters[index].Settings.AutoSP);
}

// Call this method to switch all potions using to some value
// [type] can be 'on', 'off', 'levels'
Interface.SwitchAll = func(type , window){
	if(tostring(type) == 'on'){
		for (i = 1, Characters.Count()){				
			Characters[i].Settings.AutoHP = true;
			Characters[i].Settings.AutoSP = true;
			Interface.UpdatePotionsWindow(i);
		}
		
	}
	if(tostring(type) == 'off'){
		for (i = 1, Characters.Count()){				
			Characters[i].Settings.AutoHP = false;
			Characters[i].Settings.AutoSP = false;
			Interface.UpdatePotionsWindow(i);
		}
	}
	if(tostring(type) == 'levels'){
		for (i = 1, Characters.Count()){
			Characters[i].Settings.HPlevel = 99;
			Characters[i].Settings.SPlevel = 99;
			Interface.UpdatePotionsWindow(i);
		}
	}	
}

// Call this method during sliding
// [type] should be removed
Interface.Slide = func(type, charIndex){
	var index = tonumber(charIndex);
	var uiFrame = GetFrameByName(Interface.PotionsWindow);
	if (!uiFrame) return;
	
	var uiControl = GetControl(uiFrame, 'HPlevel' .. index);
	var ctlLevel = GetNumber(uiControl);
	Characters[index].Settings.HPlevel = ctlLevel;
	
	uiControl = GetControl(uiFrame, 'MPlevel' .. index);
	ctlLevel = GetNumber(uiControl);
	Characters[index].Settings.SPlevel = ctlLevel;
	
	Interface.UpdatePotionsWindow(index);
}

// Call this method to switch AutoHP and AutoSP 
// settings for character with index [charIndex]
Interface.ChangePotionsUse = func(type, charIndex){
	var index = tonumber(charIndex);
	if (tostring(type) == 'hp'){
		Characters[index].Settings.AutoHP = !Characters[index].Settings.AutoHP;
	}
	if (tostring(type) == 'mp'){
		Characters[index].Settings.AutoSP = !Characters[index].Settings.AutoSP;
	}
}

// === AUTO-SKILLS WINDOW ===

// Loads skills window
Interface.LoadSkillsWindow = func(){
	Open(Interface.SkillsWindow);
	var uiFrame = GetFrameByName(Interface.SkillsWindow);
	if (!uiFrame) return;

	for (i = 1, Characters.Count()){
		SetTextByKey(uiFrame, 'n' .. i , Characters[i].Name);
		SetTextByKey(uiFrame, 'j' .. i , Characters[i].Job);
		Interface.UpdateSkillsWindow(i);	
	}	
}

// Call this method to update skills pictures/timings/checkboxes
// for character with index [charIndex]
Interface.UpdateSkillsWindow = func(charIndex){
	var uiFrame = GetFrameByName(Interface.SkillsWindow);
	if (!uiFrame) return;
	var i = tonumber(charIndex);	
	// get current weapon
	var currentWeapon = Characters[i].Settings.WeaponSets[Characters[i].Settings.WeaponIndex];
	// get current stance
	var currentStance = currentWeapon.Stances[Interface.StancesIndexes[i]];	
	SetTextByKey(uiFrame, 'w' .. i , currentWeapon.LeftHand .. ' + ' .. currentWeapon.RightHand);
	SetTextByKey(uiFrame, 's' .. i , currentStance.Name);
	
	// display stance skills
	for (j = 1, 5) 
	{	
		SetTextByKey(uiFrame, 'pic' .. i .. j, 'shovel-box.net/UltraShovel/' .. tostring(currentStance.Skills[j].Picture) .. '.bmp');
		SetTextByKey(uiFrame, 't' .. i .. j, ' ' .. currentStance.Skills[j].Timing);
		Interface.SetCheckBox(uiFrame, 'check' .. i .. j, currentStance.Skills[j].InUse);
	}	
	
	// display job skill
	SetTextByKey(uiFrame, 'pic' .. i .. '6', 'shovel-box.net/UltraShovel/' .. tostring(Characters[i].Settings.JobSkill.Picture) .. '.bmp');
	SetTextByKey(uiFrame, 't' .. i .. '6', ' ' .. Characters[i].Settings.JobSkill.Timing);
	Interface.SetCheckBox(uiFrame, 'check' .. i .. '6', Characters[i].Settings.JobSkill.InUse);
	
	Open(Interface.SkillsWindow);
}

// Call this method to switch weapon set for 
// character with index [charIndex]. [type] can be 'next' or 'prev'
// only from interface
Interface.ChangeWeapon = func(type, charIndex){
	if (!Characters.AreInitialized) return;
	var index = tonumber(charIndex);
	var max = table.getn(Characters[index].Settings.WeaponSets);
	Interface.StancesIndexes[index] = 1;
	if (tostring(type) == 'next'){
		if (Characters[index].Settings.WeaponIndex < max){
			Characters[index].Settings.WeaponIndex = Characters[index].Settings.WeaponIndex + 1;
		}
	}
	if (tostring(type) == 'prev'){	
		if (Characters[index].Settings.WeaponIndex > 1){
			Characters[index].Settings.WeaponIndex = Characters[index].Settings.WeaponIndex - 1;
		}
	}
	
	Interface.UpdateSkillsWindow(index);
}

// Call this method to switch stance for 
// character with index [charIndex]. [type] can be 'next' or 'prev'
// only from interface
Interface.ChangeStance = func(type , charIndex){
	if (!Characters.AreInitialized) return;
	var index = tonumber(charIndex);
	var currentWeapon = Characters[index].Settings.WeaponSets[Characters[index].Settings.WeaponIndex];
	var max = table.getn(currentWeapon.Stances);

	if (tostring(type) == 'next'){
		if (Interface.StancesIndexes[index] < max){
			Interface.StancesIndexes[index] = Interface.StancesIndexes[index] + 1;
		}
	}
	if (tostring(type) == 'prev'){	
		if (Interface.StancesIndexes[index] > 1){
			Interface.StancesIndexes[index] = Interface.StancesIndexes[index] - 1;
		}
	}
	
	Interface.UpdateSkillsWindow(index);
}

// Call this method to switch skill InUse parameter 
// only from interface
Interface.ChangeSkillUse = func(charIndex, skillIndex){
	if (!Characters.AreInitialized) return;
	var i = tonumber(charIndex);	
	var sindex = tonumber(skillIndex);
	
	if (sindex == 6){
	// job skill
		Characters[i].Settings.JobSkill.InUse = !Characters[i].Settings.JobSkill.InUse;
		return;
	}
	
	// get current weapon
	var currentWeapon = Characters[i].Settings.WeaponSets[Characters[i].Settings.WeaponIndex];
	// get current stance
	var currentStance = currentWeapon.Stances[Interface.StancesIndexes[i]];
	
	// stop if skill is empty
	if (currentStance.Skills[sindex].Id == 0){
		if (Shovel.IsDebug) SysMsg('Skill is empty!');
		return;
	}
	
	var skill = currentStance.Skills[sindex];
	skill.InUse = !skill.InUse;
}

// Call this method to switch skill Timing parameter 
// only from interface
Interface.ChangeTime = func(type, charIndex, skillIndex){
	if (!Characters.AreInitialized) return;
	var i = tonumber(charIndex);	
	var sindex = tonumber(skillIndex);
	
	// get current weapon
	var currentWeapon = Characters[i].Settings.WeaponSets[Characters[i].Settings.WeaponIndex];
	// get current stance
	var currentStance = currentWeapon.Stances[Interface.StancesIndexes[i]];
	// get current skill
	var skill;
	if (sindex == 6){
		skill = Characters[i].Settings.JobSkill;
	} else {
		skill = currentStance.Skills[sindex];
	}
	
	if (tostring(type) == 'up'){	
		skill.Timing = skill.Timing + skill.Step;		
	}
	if (tostring(type) == 'down'){
		if (skill.Timing < skill.Step) return;		
		skill.Timing = skill.Timing - skill.Step;
	}
	
	Interface.UpdateSkillsWindow(i);
}

// === AUTO-ITEMS WINDOW ===

// Loads items window
Interface.LoadItemsWindow = func(){
	Open(Interface.ItemsWindow);
	var uiFrame = GetFrameByName(Interface.ItemsWindow);
	if (!uiFrame) return;
	for (i = 1, Characters.Count()) 
	{
		SetTextByKey(uiFrame, 'n' .. i , Characters[i].Name);
		Interface.SetCheckBox(uiFrame, 'autoBullets' .. i, Characters[i].Settings.UseBullets);
		
		var bulletsIndex = 0;
		for (ind, ammo in Shovel.Ammo){
			for (key, value in ammo){
				if (tostring(key) == 'ID' &&  Characters[i].Settings.BulletsID == value){
					bulletsIndex = ind;
				}
			}
		}
		
		SetNumber(uiFrame, "bullets" .. i, bulletsIndex);
		SetNumber(uiFrame, "numpad" .. i, Characters[i].Settings.NumPadButton);
	}
	
	SetTextByKey(uiFrame, 'ai' , Text['AutoBullets']);
	SetTextByKey(uiFrame, 'ait' , Text['AutoItems']);
	SetTextByKey(uiFrame, 'c1' , Text['Characters']);
	SetTextByKey(uiFrame, 'c2' , Text['BulletType']);
	SetTextByKey(uiFrame, 'c3' , Text['NumButton']);
	SetTextByKey(uiFrame, 'sec' , Text['Sec']);
	SetTextByKey(uiFrame, 're' , Text['UseEach']);
	
	for (i = 1, 7) 
	{
		SetNumber(uiFrame, 'Item' .. i, Settings.Family.AutoItems[i].Num);
		SetText(uiFrame, 'autoitem' .. i .. '_edit', Settings.Family.AutoItems[i].Time);
		Interface.SetCheckBox(uiFrame, 'autoitem' .. i .. '_check', Settings.Family.AutoItems[i].InUse);
	}	
}

// Save edited timings for auto-items
// only from interface
Interface.SetItems = func(){
	for (i = 1, 7) 
	{	
		var uiFrame = GetFrameByName(Interface.ItemsWindow);
		if (!uiFrame) return;	
		var text = GetText(uiFrame, 'autoitem' .. i .. '_edit');
		Settings.Family.AutoItems[i].Time = tonumber(text); 
	}
}

// Call this method to enable auto item at index = [index]
Interface.EnableAutoItem = func(index){
	var i = tonumber(index);
	Settings.Family.AutoItems[i].InUse = !Settings.Family.AutoItems[i].InUse;
	if (Shovel.IsDebug) SysMsg('Item ' .. i .. '-> in use: ' .. tostring(Settings.Family.AutoItems[i].InUse));
}

// Assign button from dropbox element with name = [nameOfElement] to item with index = [index]
Interface.SetAutoItemButton = func(index, nameOfElement){
	var i = tonumber(index);
	if (!Characters.AreInitialized) return;
	if (Characters[i] == nil) return;
	
	var uiFrame = GetFrameByName(Interface.ItemsWindow);
	if (!uiFrame) return;
	var uiControl = GetControl(uiFrame, nameOfElement);
	var ctlLevel = GetNumber(uiControl);
	Settings.Family.AutoItems[i].Num = ctlLevel;
	if (Shovel.IsDebug) SysMsg('Item ' .. i .. ' -> num pad button: ' .. Settings.Family.AutoItems[i].Num .. ' selected');
}

// Enable auto bullets for character with index = [index]
Interface.EnableAutoBullets = func(index){
	var i = tonumber(index);
	if (!Characters.AreInitialized) return;
	if (Characters[i] == nil) return;
	Characters[i].Settings.UseBullets = true;
	if (Shovel.IsDebug) SysMsg('[' .. i .. '|' .. Characters[i].Name .. '] using bullets ' .. tostring(Characters[i].Settings.UseBullets));
}

// Assign bullets type choosen in element with name = [nameOfElement] to character with index = [index]
Interface.SetAutoBulletsType = func(nameOfElement, index){
	var i = tonumber(index);
	if (!Characters.AreInitialized) return;
	if (Characters[i] == nil) return;
	var uiFrame = GetFrameByName(Interface.ItemsWindow);
	if (!uiFrame) return;
	var uiControl = GetControl(uiFrame, nameOfElement);
	var ctlLevel = GetNumber(uiControl);
	Characters[i].Settings.BulletsID = Shovel.Ammo[ctlLevel].ID;
	if (Shovel.IsDebug) SysMsg('[' .. i .. '|' .. Characters[i].Name .. '] bullets with id ' .. Characters[i].Settings.BulletsID .. ' selected');
}

// Assign button type choosen in element with name = [nameOfElement] to character with index = [index] (for autobullets)
Interface.SetAutoBulletsButton = func(nameOfElement, index){
	var i = tonumber(index);
	if (!Characters.AreInitialized) return;
	if (Characters[i] == nil) return;
	
	var uiFrame = GetFrameByName(Interface.ItemsWindow);
	if (!uiFrame) return;
	var uiControl = GetControl(uiFrame, nameOfElement);
	var ctlLevel = GetNumber(uiControl);
	Characters[i].Settings.NumPadButton = ctlLevel;
	if (Shovel.IsDebug) SysMsg('[' .. i .. '|' .. Characters[i].Name .. '] num pad button ' .. Characters[i].Settings.NumPadButton .. ' selected');
}

// === ADDONS WINDOW ===

// Loads addons window
Interface.LoadAddonsWindow = func(){
	Open(Interface.AddonsWindow);
	var uiFrame = GetFrameByName(Interface.AddonsWindow);
	if (!uiFrame) return;
	
	SetTextByKey(uiFrame, 't' , Text['AddonList']);
}

// Loads description for selected addon
Interface.SetAddon = func(){
	var uiFrame = GetFrameByName(Interface.AddonsWindow);
	if (!uiFrame) return;
	var uiControl = GetControl(uiFrame, 'select_addon');
	var ctlLevel = GetNumber(uiControl);
	for (key, value in Addons.Current)
	{
		if (ctlLevel == value.ListID)
		{
			SetTextByKey(uiFrame, 'author' , value.Author);
			SetTextByKey(uiFrame, 'desc' , value.Description);
		}		
	}
}

// === ABOUT WINDOW ===

// Loads about window
Interface.LoadAboutWindow = func(){
	Open(Interface.AboutWindow);
}

// === COMMON ===

// Common method for handling many changes from interface
Interface.SetSettings = func (type, param, charIndex){
	if (tostring(type) == 'slider'){
		var uiFrame = GetFrameByName(Interface.SettingsWindow);
		if (!uiFrame) return;
		var uiControl = GetControl(uiFrame, param);
		var ctlLevel = GetNumber(uiControl);
		Settings.Family[param] = ctlLevel;	
		Interface.UpdateSettingsWindow();
	}
	
	if (tostring(type) == 'check'){
		Settings.Family[param] = !Settings.Family[param];
	}
	
	if (tostring(type) == 'checkactor'){
		var index = tonumber(charIndex);
		if (tostring(param) == 'autoattack' .. tostring(charIndex)){
			Characters[index].Settings.AutoAttack = !Characters[index].Settings.AutoAttack;
		}		
	}
	
	if (tostring(type) == 'mode')
	{
		var uiFrame = GetFrameByName(Interface.SettingsWindow);
		if (!uiFrame) return;
		var text = GetText(uiFrame, 'eac');
		if (Settings.Family.Mode == 0){
			Settings.Family.ReturnTime = tonumber(text);
		} else {
			Settings.Family.CallTime = tonumber(text);			
		}
	}
	
	if (tostring(type) == 'savesounds')
	{
		var uiFrame = GetFrameByName(Interface.AlarmsWindow);
		if (!uiFrame) return;
		var text = GetText(uiFrame, 'ReportAlarmSound');
		Settings.Family.ReportAlarmSound = text;
		text = GetText(uiFrame, 'InventoryAlarmSound');
		Settings.Family.InventoryAlarmSound = text;
		text = GetText(uiFrame, 'DeadAlarmSound');
		Settings.Family.DeadAlarmSound = text;
	}
	
	if (tostring(type) == 'lang')
	{
		var uiFrame = GetFrameByName(Interface.SettingsWindow);
		if (!uiFrame) return;
		Settings.Family.Language = tostring(param);
		Settings.ChangeLanguage();		
	}
}

// This method needs to draw checkboxes on the images correctly
Interface.Update = func(){
	if(IsVisible(Interface.SkillsWindow)){
		var uiFrame = GetFrameByName(Interface.SkillsWindow);
		if (!uiFrame) return;
		UpdateUi(uiFrame);
	}
}


Interface.Load();


