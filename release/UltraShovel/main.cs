if (Shovel){
	var status, rez = pcall(Shovel.Unload);
		if (!status) SysMsg(rez);
} else {
	Shovel = {
		AIVersion = 		"Release 2.11", 
		Name = 				"UltraShovel",
		Commander = 		GetMyCommanderName(), 
		Region = 			GetNation(), 
		GameStartTime = 	os.time(), 
		Ext = 				".cs",
		LanguagesFolder = 	"languages",
		ModulesFolder =		"modules",
		SoundsFolder = 		"sounds",
		AiFolder = 			"UltraShovel",
		SettingsFolder =	"settings",
		DataFolder =        "database",
		AddonsFolder = 		"addons",
		EOL = 				string.char(13), 
		FSP = 				string.char(92), 
		TAB = 				string.char(9), 
		WINEOL = 			string.char(10),
		Passed = 			0,
		Now =				0,
		_G_ = { SysMsg = SysMsg , GetNation = GetNation, sleep = sleep},
		IsDebug = false,
		Farm = false
	}	
	
	// Replaced SysMsg function
	SysMsg = func (text) { Shovel._G_.SysMsg(os.date('[%X] ')..tostring(text)); }
	// Replaced GetNation function
	GetNation = func () { var x = Shovel._G_.GetNation(); if (x == '') return 'SEA'; else return x; }	
	// Get current region
	Shovel.Region = GetNation();
	
	Shovel.Unload = func (){
		ChangeTactics("TS_NONE");
		var k, v;
		if (Shovel._G_) 
		{
			for (k, v in pairs(Shovel._G_)) 
			{
				if (type(v) == 'function') _G[k] = v;
				else _G[k] = nil;
			}
			Shovel._G_ = nil;
		}		
		Close('tutomessage');
		Interface.ShowMini(Text['GoodByeMessage']);		
		Interface.CloseAll();
		System.UnloadModules();
		
		Shovel = nil;
	}

	// Main initialization method
	Shovel.Initialize = func(){
		pcall(dofile, Shovel.AiFolder .. Shovel.FSP .. Shovel.ModulesFolder .. Shovel.FSP .. 'system' .. Shovel.Ext );
		Shovel.LoadAI();
		Close('tutomessage');
		
		ShowAllPcLevel();	
		SelectAll();
		SetAllSelectMode();
		ChangeTactics("TS_NONE");
	}
		
	Shovel.LoadAI = func(){		
		var k, v;
		for (k, v in pairs(Shovel)) {
			if (type(v) == 'function') 
			{
				if (_G[k] && type(_G[k]) == 'function') 
				{
					if (!Shovel._G_[k]) Shovel._G_[k] = _G[k];	
					
					_G[k] = v;
				} else if (string.sub(k, 1, 4) == 'SCR_') {
					Shovel._G_[k] = true;					
					_G[k] = v;
				}
			}
		}
	}
	
	// Method returns true if passed more then [seconds] since [time]
	Shovel.IsTime = func (time, seconds, now) {
		if (time){
			if (now) return (tonumber(now) - tonumber(time)) >= tonumber(seconds);
			return (tonumber(Shovel.Now) - tonumber(time)) >= tonumber(seconds);
		}
		return false;
	}
	
	// Replaced sleep method
	sleep = func (ms) {
		// call updates for modules if needed
		// each sleep [very often]
		Interface.Update();
		Alarms.Update();
		Brain.AutoPotions();
		Shovel.Passed = Shovel.Passed + tonumber(ms);
		// call updates each 1 sec
		if (Shovel.Passed >= 1000) {
			Shovel.Now = os.difftime(os.time(), Shovel.GameStartTime);
			Shovel.Passed = 0;
			if (sGeWithTheShovel) sGeWithTheShovel.Monitoring();
			Addons.Update();
			Brain.AutoItems();
		}
		// call base method
	    Shovel._G_.sleep(tonumber(ms));
	}

	// Main SCR_TS_MOVE function replaced by UltraShovel
	Shovel.SCR_TS_NONE = func (self) {	
		Shovel.Prepare(self, "None");
		
		var aiIndex = Characters.GetAiIndex(self);	
		if (aiIndex == nil){
			SysMsg('Something wrong with character');
		}
		
		if (Shovel.Farm){
			Brain.ReturnToFarm(Characters[aiIndex]);
		}
		
		while(true){
			sleep(200);
			Brain.UserTarget(Characters[aiIndex]);
		}
	}
		
	// Main SCR_TS_MOVE function replaced by UltraShovel	
	Shovel.SCR_TS_MOVE = func (self) { 		
		Shovel.Prepare(self, "Move");

		var aiIndex = Characters.GetAiIndex(self);	
		if (aiIndex == nil){
			SysMsg('Something wrong with character');
		}
		
		if (Shovel.Farm){
			Brain.ReturnToFarm(Characters[aiIndex]);
		}
				
		while(true){	
			sleep(200);
			Brain.UserTarget(Characters[aiIndex]);
		}		
	}

	// Main SCR_TS_KEEP function replaced by UltraShovel
	Shovel.SCR_TS_KEEP = func (self){ 
		Shovel.Prepare(self, "Keep");
										
		var aiIndex = Characters.GetAiIndex(self);	
		if (aiIndex == nil){
			SysMsg('Something wrong with character');
		}
		
		// save current char settings each time when user press space
		Characters.Save(aiIndex);
		// call IsLeader makes us possible to be sure
		// {if} block will be called once
		if (IsLeader(GetAiActor(self)) == 'YES' ){
			Settings.Save();
		}
		
		while(true){	
			sleep(100);
			
			Brain.AutoAttack(Characters[aiIndex]);
			Brain.AutoPick(Characters[aiIndex]);	
			Brain.AutoBullets(Characters[aiIndex]);
			Brain.UserTarget(Characters[aiIndex]);
			Brain.AutoSkills(Characters[aiIndex]);
			Brain.KeepPosition();
		}		
	}
				
	// Call this method at start of each SCR_TS
	// it helps to handle existing/new chars
	Shovel.Prepare = func(self, stance) {
		sleep(100);
		
		var aiIndex = Characters.GetAiIndex(self);
		if (aiIndex != nil){
			if (Shovel.IsDebug) SysMsg(stance .. " + " .. aiIndex);
		}
		
		Characters.Handle(self);	
		
		// cant start farm with non-initialized chars
		while (!Characters.AreInitialized) sleep(200);
	}
			
	Shovel.SCR_ATTACKER_TS_NONE = Shovel.SCR_TS_NONE;
	Shovel.SCR_ATTACKER_TS_MOVE = Shovel.SCR_TS_MOVE;
	Shovel.SCR_ATTACKER_TS_KEEP = Shovel.SCR_TS_KEEP;
	Shovel.SCR_HEALER_TS_NONE = Shovel.SCR_TS_NONE;
	Shovel.SCR_HEALER_TS_MOVE = Shovel.SCR_TS_MOVE;
	Shovel.SCR_HEALER_TS_KEEP = Shovel.SCR_TS_KEEP;
	Shovel.SCR_PUPPET_TS_NONE = Shovel.SCR_TS_NONE;
	Shovel.SCR_PUPPET_TS_MOVE = Shovel.SCR_TS_MOVE;
	Shovel.SCR_PUPPET_TS_KEEP = Shovel.SCR_TS_KEEP;
	 
	Shovel.Initialize();
	
}