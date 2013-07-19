if (Hotkeys){	Hotkeys.Unload();	} 

Hotkeys = {
	
}

Hotkeys.Load = func(){	
	if (Shovel.IsDebug) SysMsg("Hotkeys loaded");
}

// Change auto keep mode [on/off].
// Shift + 4
Hotkeys.ChangeKeepMode = func()
{	
	Shovel.Farm = !Shovel.Farm;
	if (Shovel.Farm){
		SysMsg(Text['KeepModeOn']);
		//ChangeTactics("TS_KEEP");
	} else {
		SysMsg(Text['KeepModeOff']);
		ChangeTactics("TS_NONE");
	}
}

// Change auto pick for character with index = [index]
// Shift + 1/2/3
Hotkeys.ChangeAutoPick = func(index){
	var i = tonumber(index);
	if (!Characters.AreInitialized) return;
	if (Characters[index] == nil) return;
	Characters[index].Settings.AutoPick = !Characters[index].Settings.AutoPick;
	
	if (Characters[index].Settings.AutoPick){
		SysMsg('[' .. index .. ' | ' .. Characters[index].Name .. '] ' .. Text['AutoPickOn']);
	} else {
		SysMsg('[' .. index .. ' | ' .. Characters[index].Name .. '] ' .. Text['AutoPickOff']);
	}
	
	Interface.UpdatePotionsWindow(i);
}

Hotkeys.SilentMode = func(){
	DisableBGM(1);
	DisableVoiceEffect(1);
	DisableSoundEffect(1);
	SetSEVolume(0);
	SetBGMVolume(0);
	SetVCVolume(0);
	SysMsg(Text['SilentMode']);
}

Hotkeys.Load();