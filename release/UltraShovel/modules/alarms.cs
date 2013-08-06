if (Alarms){	Alarms.Unload();	} 

Alarms = {
	SoundAlarmTime = -60,
	UpdateInventoryTime = -10,
	Timer = 0,
}

Alarms.Unload = func (){
	Alarms = nil;
}

Alarms.Load = func(){	
	if (Shovel.IsDebug) SysMsg("Alarms loaded");
}

Alarms.Update = func(){
	if (!Shovel.IsTime(Alarms.Timer, 1)) return;
	Alarms.Timer = Shovel.Now;
	Alarms.CheckDead();
	if(Settings.Family.InventoryAlarm) { Alarms.CheckInventory(); }
	if(Settings.Family.ReportAlarm) { Alarms.CheckReport(); }	
}

Alarms.CheckReport = func(){
	if (IsVisible('autoinspect') == 'YES'){
		Alarms.MakeSound(Settings.Family.ReportAlarmSound);
		return;
	}
}

Alarms.CheckInventory = func(){
	if (!Shovel.IsTime(Alarms.UpdateInventoryTime , 10)) return;

	Alarms.UpdateInventory();
	Alarms.UpdateInventoryTime = Shovel.Now;
	var uiFrame = GetFrameByName('inventory');
	if (!uiFrame) return;
	var count = tonumber(GetTextByKey(uiFrame, 'invitem_count'));
	if (count < 245){return;}
	Alarms.MakeSound(Settings.Family.InventoryAlarmSound);
}

Alarms.CheckDead = func(){
	for(i = 1, Characters.Count()){
		if(Characters[i].Settings.DeadAlarm){
			if (Characters[i].HP == 0){
				Alarms.MakeSound(Settings.Family.DeadAlarmSound);
				return;
			}
		}
	}
}

Alarms.MakeSound = func(param){
	if (!Shovel.IsTime(Alarms.SoundAlarmTime, 60)) return;
	Alarms.SoundAlarmTime = Shovel.Now;
	SysMsg("Alarm!");
	if (Settings.Family.UseSimpleAlarm)
	{
		DisableBGM(0);
		DisableVoiceEffect(0);
		DisableSoundEffect(0);
		SetSEVolume(250);
		SetBGMVolume(250);
		SetVCVolume(250);
		pcall(func() { PlayMusic(GetText(GetFrameByName("jukebox"), 'select song'), 1, 0); });
	} else {
		var path = Shovel.AiFolder .. Shovel.FSP .. Shovel.SoundsFolder .. Shovel.FSP .. param .. '.mp3';
		var link = io.open(path, 'r');
		if (!link){
			SysMsg('Sound file does not exist');
		} else {
			os.execute(path);
		}
	}
}

Alarms.UpdateInventory = func(){
	if (IsVisible('inventory') == 'YES'){
		Close('inventory');
		Open('inventory');
	} else {
		Open('inventory');
		Close('inventory');
	}
}

Alarms.Load();