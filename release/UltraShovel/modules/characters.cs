if (Characters){	Characters.Unload();	} 

Characters = { Jobs = {}, CurrentIndex = 1, AreInitialized = false }

Characters.Unload = func (){
	Characters = nil;
}


// === PUBLIC METHODS ===


Characters.Handle = func(self){	
	// this check needs when family change location
	// or pick new chars in barracks
	var aiIndex = Characters.GetAiIndex(self);
	if (aiIndex == nil && Characters.AreInitialized){
		if (Shovel.IsDebug) SysMsg('New chars detected');
		Characters.CancelAll();
	}
	if (Characters.AreInitialized) return;
	
	var i = Characters.CurrentIndex;
	// [i] can't be more then 3, 3 chars is maximum :)
	if (i > 3){
		SysMsg('Something went wrong');
		return;
	}
	
	if (Shovel.IsDebug) SysMsg("Loading actor " .. i);
	
	
	var name = Characters.GetName(i);
	var job = Characters.GetJobBySelf(self);	
	
	// init [current] variable
	// and put weapons and stances inside
	var current = {};
	// get id by job
	var id = Characters.FindIdByJobName(job);
	if (id == 0) return;	
	
	current.Job = job;	
	current.Name = name;
	current.Index = i;
	// load self and selfai
	current.Self = self;
	current.SelfAi = GetAiActor(self);
	// load temp params
	current.Level = Characters.GetLevel(i);
	current.HP = Characters.GetHP(i);
	current.SP = Characters.GetSP(i);
	current.MaxHP = Characters.GetMaxHP(i);
	current.MaxSP = Characters.GetMaxSP(i);
	
	var settings = Characters.Load(i, current.Name);
	if (settings == nil){
		// load default user params
		current.Settings = {
			HPlevel = 70,
			SPlevel = 70,
			BulletsID = 21116,
			NumPadButton = 0,
			UseBullets = false,
			AutoAttack = true,
			AutoPick = true,
			AutoHP = true,
			AutoSP = false,
			DeadAlarm = false,
			WeaponIndex = 1,
			WeaponSets = Characters.LoadWeaponSets(id),
			JobSkill = Characters.LoadSkill(GetDataTableNumberValue("Job", id, "JobSkill"))
		}
	} else {
		current.Settings = settings;		
	}
	
	// put our table to global table, then we can easy access it
	Characters[i] = current;
	Characters.RefreshSkillUsing(i);
	
	SysMsg(Text['Slot'] .. i .. " : " .. current.Job .. ' [' .. current.Name .. ']');
	Characters.CurrentIndex = Characters.CurrentIndex + 1;
	
	// finish condition
	if (Characters.CurrentIndex - 1 == Characters.Count()){
		Characters.AreInitialized = true;
		if (Shovel.IsDebug) SysMsg('Characters initialization finished');
	}
}

// Method returns index of char by [self] parameter
Characters.GetAiIndex = func(self){	
	for (i = 1, 3){
		if (Characters[i] != nil && Characters[i].Self == self){
			return i;
		}
	}
	return nil;
}

// Method updates hp/sp/maxhp/maxsp of character
Characters.Update = func(index){
	if (Characters[index] == nil) return;
	Characters[index].HP = Characters.GetHP(index);
	Characters[index].SP = Characters.GetSP(index);
	Characters[index].MaxHP = Characters.GetMaxHP(index);
	Characters[index].MaxSP = Characters.GetMaxSP(index);
}

// Method dumps character's settings to file
Characters.Save = func(index){	
	if (Characters[index] == nil) return;
	var fileName = Shovel.AiFolder .. Shovel.FSP .. Shovel.SettingsFolder .. Shovel.FSP .. 'Char_' .. Shovel.Commander .. '_' .. Characters[index].Name;
	System.SaveTableToFile(Characters[index].Settings, fileName);
	if (Shovel.IsDebug) SysMsg('Settings for ' .. Characters[index].Name .. ' saved');
}

// Method loads character's settings from file
// returns nil if settings file does not exists
Characters.Load = func(index, name){
	var fileName = Shovel.AiFolder .. Shovel.FSP .. Shovel.SettingsFolder .. Shovel.FSP .. 'Char_' .. Shovel.Commander .. '_' .. name;
	var settings = System.LoadTableFromFile(fileName);
	if (settings != nil){
		if (Shovel.IsDebug) SysMsg('Settings for ' .. name .. ' loaded');
		return settings;
	} else {		
		if (Shovel.IsDebug) SysMsg('Settings for ' .. name .. ' not found');
		if (Shovel.IsDebug) SysMsg(fileName);
	}
}

// Returns index of selected stance for character with index = [index], starts with 1
Characters.GetSelectedStanceIndex = func(index){
	var uiFrame = GetFrameByName('charbaseinfo', index - 1);
	if (uiFrame){
		var uiControl = GetControl(uiFrame, 'STANCETAB');
		var selectedStanceIndex = GetNumber(uiControl);
		
		return selectedStanceIndex + 1;	
	} else {
		SysMsg('Error during detecting selected stance');
		return 0;
	}
}

// Change stance carefully
Characters.ChangeStance = func(index, stanceIndex){
	var currentStanceIndex = Characters.GetSelectedStanceIndex(index);
	if (Shovel.IsDebug) SysMsg('[' .. Characters[index].Name .. '] changing stance from ' .. currentStanceIndex .. ' to ' .. stanceIndex);
	var leaderIndex = GetLeaderIndex() + 1;
	if (index != leaderIndex) SelectMyPc(index - 1);
	sleep(100);
	ChangeStance(stanceIndex - 1);
	if (index != leaderIndex) SelectMyPc(leaderIndex - 1);
	sleep(2000);
}

// Call all team to character with index = [index]
Characters.Call = func(index){
	var leaderIndex = GetLeaderIndex() + 1;
	if (index != leaderIndex) SelectMyPc(index - 1);
	sleep(100);
	var callCount = 0;
	while (callCount < 5){
		Call();
		callCount = callCount + 1;
		sleep(400);
	}
	if (index != leaderIndex) SelectMyPc(leaderIndex - 1);
	sleep(200);
}

// === PRIVATE METHODS ===

Characters.RefreshSkillUsing = func(index){
	for (i = 1, table.getn(Characters[index].Settings.WeaponSets)){
		var set = Characters[index].Settings.WeaponSets[i];
		for (j = 1, table.getn(set.Stances)){
			var stance = set.Stances[j];
			for (k = 1, 5){
				var skill = stance.Skills[k];
				skill.UsageTime = -1000;
			}
		}
	}
	
	Characters[index].Settings.JobSkill.UsageTime = -1000;
}

// Methods will delete loaded chars and returns default values for module
Characters.CancelAll = func(){
	for(i = 1, 3){
		Characters[i] = nil;
	}
	Characters.AreInitialized = false;
	Characters.CurrentIndex = 1;
	
	SelectAll();
	SetAllSelectMode();
	
	ChangeTactics("TS_NONE");
}

// Method returns the name of character with index = [index]
Characters.GetName = func(index){
	var frame = GetFrameByName('status', index - 1);
	if (frame){				
		return GetTextByKey(frame, 'charactername');
	}
}

// Method returns level of character with index = [index]
Characters.GetLevel = func(index){
	var frame = GetFrameByName('status', index - 1);
	if (frame){				
		return GetTextByKey(frame, 'characterlevel');
	}
}

// Method returns job of character with index = [index]
Characters.GetJob = func(index){
	var frame = GetFrameByName('status', index - 1);
	if (frame){				
		return GetTextByKey(frame, 'characterjob');
	}
}

// Method returns job name of character using self parameter
Characters.GetJobBySelf = func(self){
	var selfAi = GetAiActor(self);
	return GetJobName(selfAi);
}

// Method returns HP of character with index = [index]
Characters.GetHP = func(index){
	var frame = GetFrameByName('charbaseinfo', index - 1);
	if (frame){				
		return GetTextByKey(frame, 'hp');
	}
}

// Method returns HP of character with index = [index]
Characters.GetSP = func(index){
	var frame = GetFrameByName('charbaseinfo', index - 1);
	if (frame){				
		return GetTextByKey(frame, 'sp');
	}
}

// Method returns Max HP of character with index = [index]
Characters.GetMaxHP = func(index){
	var frame = GetFrameByName('charbaseinfo', index - 1);
	if (frame){				
		return GetTextByKey(frame, 'max_hp');
	}
}

// Method returns Max SP of character with index = [index]
Characters.GetMaxSP = func(index){
	var frame = GetFrameByName('charbaseinfo', index - 1);
	if (frame){				
		return GetTextByKey(frame, 'max_sp');
	}
}

// Method returns count of characters in current family set
Characters.Count = func(){
	var count = 0;
	for (i = 0, 2){
		if (IsExistMyPc(i) == 'YES'){
			count = count + 1;
		}
	}	
	return count;
}

// This method will split [inputStr] with [sep] symbol.
Characters.SplitString = func(inputstr, sep){
	if (sep == nil)
		sep = "%s";
    
	var t={};
	var i=1;
	for (str in string.gfind(inputstr, "([^"..sep.."]+)"))
	{
		t[i] = str;
		i = i + 1;
	}
	return t;
}

// Method loads all possible chars, which can be read from GE db
Characters.LoadAllPossibleChars = func(){
	var currentJobIndex = 1;
	for (i = 1, 199){
		var currentJob = GetDataTableStringValue("Job", i, "ClassName");
		if (currentJob != nil){
			Characters.Jobs[currentJobIndex] = {};
			Characters.Jobs[currentJobIndex].Id = i;
			Characters.Jobs[currentJobIndex].Name = currentJob;
			currentJobIndex = currentJobIndex + 1;
		}
	}
	
	SysMsg("Loaded " .. table.getn(Characters.Jobs) .. " characters!");
}

// Returns id of character with job name [jobName]
// Returns 0 if job name is incorrect
Characters.FindIdByJobName = func(jobName){
	for (key, value in pairs (Characters.Jobs)){
		if (value.Name == jobName){
			return value.Id;
		}
	}
	SysMsg("Incorrect job name");
	return 0;
}

// Method returns weapon sets for character with id = [id]
Characters.LoadWeaponSets = func(id){
	// each char has weapon sets
	// get weapon sets for char with id
	var weaponSetsString = GetDataTableStringValue("Job", id, "EqpWeaponSet");
	// split weapon sets string, it looks like "xxx,xxx,xxx,xxx"
	var weaponSets = Characters.SplitString(weaponSetsString, ',');
	// get num of weaponSets
	var numOfWeaponSets = table.getn(weaponSets);
	
	var result = {}
	// for each weapon set do:
	for (i = 1, numOfWeaponSets){
		result[i] = {};
		// get id
		result[i].Id = weaponSets[i];
		// get weapons
		result[i].LeftHand = GetDataTableStringValue("StanceCondition", weaponSets[i], "LHand");
		result[i].RightHand = GetDataTableStringValue("StanceCondition", weaponSets[i], "RHand");
		// load stances
		result[i].Stances = Characters.LoadStancesInWeaponSet(weaponSets[i]);
	}
	return result;
}

// Method returns stances inside weapon set with id = [weaponSetId]
Characters.LoadStancesInWeaponSet = func(weaponSetId){
	var stanceIndex = 1;
	var stances = {};
	// check all possible positions	
	while (stanceIndex <= 6)
	{
		// get stance id
		stanceId = GetDataTableNumberValue("StanceCondition", weaponSetId, "Stance" .. stanceIndex);
		// if id like 0, it means no more stances to read
		if (stanceId == 0) break;
		// fill info about stance
		stance = Characters.LoadStance(stanceId);
		stance.Index = stanceIndex;
		// fill table with stances
		stances[stanceIndex] = stance;
		// increment stance index
		stanceIndex = stanceIndex + 1;
	}		
	return stances;
}

// Method returns stance with id = [stanceId]
// Stance structure:
// 		Id = Int
// 		Name = String
// 		[1] = Table
// 		[2] = Table
// 		[3] = Table
// 		[4] = Table
// 		[5] = Table
Characters.LoadStance = func(stanceId){
	var stance = {};
	// get id
	stance.Id = stanceId;
	// get name
	stance.Name = GetDataTableStringValue("Stance", stanceId, "EngName");
	stance.Skills = {};
	// load each skill
	for (i = 1, 5){
		skillId = GetDataTableNumberValue("Stance", stanceId, "SkillID" .. i);
		stance.Skills[i] = Characters.LoadSkill(skillId);
	}
	return stance;
}

// Method returns skill with id = [skillId]
// Skill structure:
// 		Id = Int
// 		Picture = String
//		Timing = Int
//		UsageTime = Int
//		Target = String (Party / Corpse / Enemy / Ground / Wave / None / UI)
//		InUse = Bool
Characters.LoadSkill = func(skillId){
	var skill = {};
	// get id
	skill.Id = skillId;
	skill.Target = GetDataTableStringValue("Skill", skillId, "Target");	
	skill.InUse = false;
	skill.UsageTime = -1000;
	skill.IsTeamBuff = false;
	
	// get picture
	var picName = GetDataTableStringValue("Skill", skillId, "FileName");
	if (picName == nil){
		skill.Picture = 'none';
	} else {
		skill.Picture = picName;
	}
		
	// getting params for calculating timing
	var castTime = GetDataTableNumberValue("Skill", skillId, "CastTime");
	var coolDown = GetDataTableNumberValue("Skill", skillId, "CoolDown");	
	var holdTime = GetDataTableNumberValue("Skill", skillId, "HoldTime");
	var duration = 0;
		
	if (skill.Target == 'None' || skill.Target == 'Party'){
		var buffId = GetDataTableNumberValue("Skill", skillId, "BuffID");
		
		if (buffId != 0){
			var descriptionCode = GetDataTableStringValue("Skill", skillId, "SpecDesc10");
			duration = Characters.GetBuffDuration(descriptionCode, skillId);
			// is a team buff?
			var useType = GetDataTableStringValue("Skill", skillId, "UseType");
			var onTeam = GetDataTableStringValue("Skill", skillId, "OnTeam");
			if (useType == 'AREA' && onTeam == 'YES'){
				skill.IsTeamBuff = true;
			}
		}		
	}
	
	// formula: calculating default timing for skill
	var timing = castTime + coolDown + holdTime + duration + 100;
	// we using seconds, then div 1000
	skill.Timing = math.ceil(timing / 1000);	
	// calculating step for change timing
	skill.Step = 10;		
	if (skill.Timing < 40) skill.Step = 8;
	if (skill.Timing < 20) skill.Step = 4;
	if (skill.Timing < 10) skill.Step = 3;
	if (skill.Timing < 5) skill.Step = 2;
	
	return skill;
}

// Very dirty way to get buff duration, dunno another way.
Characters.GetBuffDuration = func(descCode, skillId){
	Open('Empty');
	var uiFrame = GetFrameByName('empty');
	if (!uiFrame){
		SysMsg('Error while creating >empty< frame');
		return 15000;
	}
	
	SetTextByKey(uiFrame, 'emptyLabel' , descCode);
	var text = GetTextByKey(uiFrame, 'emptyLabel');	
	var result = string.gfind(text, 'Duration%s:%s([%d]+)%s([%w]+)');
	var duration, metric = result();
	if (duration == nil) {
		// try another way
		result = string.gfind(text, 'Duration%s([%d]+)%s([%w]+)');
		duration, metric = result();
		if (duration == nil){
			if (Shovel.IsDebug) SysMsg('Error while calculating buff duration for skill with id ' .. skillId);
			if (Shovel.IsDebug) SysMsg('Using default value [15]');
			duration = 15;
		}		
	}
	Close('Empty');
	return duration * 1000;
}

// Entry point of module here
Characters.LoadAllPossibleChars();