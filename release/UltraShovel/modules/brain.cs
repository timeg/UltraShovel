if (Brain){	Brain.Unload();	} 

Brain = {
	AmmoTime = 0,
	ReturnTime = 0,
	ItemsTime = { 
		[1] = 0,
		[2] = 0,
		[3] = 0,
		[4] = 0,
		[5] = 0,
		[6] = 0,
		[7] = 0				
	}
}

Brain.Unload = func (){
	Brain = nil;
}

Brain.Load = func(){	
	if (Shovel.IsDebug) SysMsg("Brain loaded");
}

// Call this method to keep position
Brain.KeepPosition = func(){
	if (Settings.Family.Mode == 0){
		if (!Shovel.IsTime(Brain.ReturnTime, Settings.Family.ReturnTime)) return;
		Brain.ReturnTime = Shovel.Now;
		
		for (i = 1, Characters.Count())
		{
			if(IsNearFromKeepDestPosition(Characters[i].SelfAi, 100) != 'YES')
			{
				if (Shovel.IsDebug) SysMsg('Return ' .. Characters[i].Name);
				KeepDestMoveTo(Characters[i].SelfAi);				
			}
		}
		
		sleep(1000);	
	} else {
		sleep(3000);
		if (Shovel.IsTime(Brain.ReturnTime, Settings.Family.CallTime)){	
			Brain.ReturnTime = Shovel.Now;
			Call();
		}			
	}
}

// Call this method to handle auto-attack
Brain.AutoAttack = func(Actor){
	sleep(100);
	if (!Actor.Settings.AutoAttack) { return; }
	if (IsMoving(Actor.SelfAi) == 'YES') { return; }
	var curTarget = GetNearAtkableEnemy(Actor.SelfAi, Settings.Family.KeepRange);
	if (!curTarget) return;
	SetAiTarget(Actor.SelfAi, curTarget);
	if (IsAbleToAttack(Actor.SelfAi) == 'YES'){
		Attack(Actor.SelfAi, curTarget);
	}
}

// Call this method to handle auto-pick
Brain.AutoPick = func(Actor){
	sleep(100);
	// stop pick up if char is moving
	if (IsMoving(Actor.SelfAi) == 'YES') { return false; }
	if (Actor.Settings.AutoPick)
	{
		// get nearest item in range
		var nearItem = GetNearItem(Actor.SelfAi, Settings.Family.PickRange);
		if (nearItem != 0) 
		{
			// pick it
			PickItem(Actor.SelfAi, nearItem);
			return true;
		}
		else return false;
	}
}

// Call this method to handle auto-skills
Brain.AutoSkills = func(Actor){
	sleep(100);
	for (i = 1, table.getn(Actor.Settings.WeaponSets)){
		var set = Actor.Settings.WeaponSets[i];
		for (j = 1, table.getn(set.Stances)){
			var stance = set.Stances[j];
			for (k = 1, 5){			
				var skill = stance.Skills[k];
				var result = Brain.TryToUseSkill(Actor, stance, skill);
				if (result) sleep(100);
			}
		}
	}
	// use job skill
	var skill = Actor.Settings.JobSkill;
	var result = Brain.TryToUseSkill(Actor, nil, skill);
	if (result) sleep(100);
}

// Character trying to use skill
Brain.TryToUseSkill = func(Actor, stance, skill){
	if (skill.Id == 0) return false;
	if (!skill.InUse) return false;
	if (!Shovel.IsTime(skill.UsageTime, skill.Timing)) return false;

	// if selected stance is incorrect, then choose correct stance
	if (stance != nil){
		if (skill.Target != 'Corpse'){
			if (!Brain.IsMyStanceCorrect(Actor, stance)){
				Characters.ChangeStance(Actor.Index, stance.Index);
			}
		}
	}
	
	if (Shovel.IsDebug) SysMsg('[' .. Actor.Name .. '] Skill: ' .. skill.Id);
	// handle targets
	if (skill.Target == 'Enemy'){
		var curTarget = GetNearAtkableEnemy(Actor.SelfAi, Settings.Family.KeepRange);
		if (curTarget){
			UseSkill(Actor.SelfAi, curTarget, skill.Id);
		}
		
	} else if (skill.Target == 'None'){	
		if (Settings.Family.CallAtPartyBuff){
			if (skill.IsTeamBuff){
				Characters.Call(Actor.Index);
			}
		}
		
		if (UseSkillNone) UseSkillNone(Actor.SelfAi, skill.Id);
		
	} else if (skill.Target == 'Party'){
		if (Settings.Family.CallAtPartyBuff){
			if (skill.IsTeamBuff){
				Characters.Call(Actor.Index);
			}
		}
		
		var curTarget = GetNeedHealFriend(Actor.SelfAi, Settings.Family.KeepRange, 95)
		if (curTarget){
			UseSkill(Actor.SelfAi, curTarget, skill.Id);
		}
		
	} else if (skill.Target == 'Corpse'){
		var curTarget = Brain.GetDeadTeamMember();
		if (curTarget){
			if (!Brain.IsMyStanceCorrect(Actor, stance)){
				Characters.ChangeStance(Actor.Index, stance.Index);
			}
			UseSkill(Actor.SelfAi, curTarget, skill.Id);
		}
		
	} else if (skill.Target == 'Ground'){
		if (UseSkillNone) UseSkillNone(Actor.SelfAi, skill.Id);	
		
	} else if (skill.Target == 'Wave'){
		var curTarget = GetNearAtkableEnemy(Actor.SelfAi, Settings.Family.KeepRange);
		if (curTarget){
			UseSkill(Actor.SelfAi, curTarget, skill.Id);
		}
	} else {
		SysMsg('I dont know how to use skill with [Id] = ' .. skill.Id);
	}
	
	skill.UsageTime = Shovel.Now; 
	while(IsSkillUsing(Actor.SelfAi) == 'YES') { sleep(200); }
	return true;
}

// Call this method to handle auto-items
Brain.AutoItems = func(Actor){
	for (i = 1, 7){
		if (Settings.Family.AutoItems[i].InUse)	{
			if (Shovel.IsTime(Brain.ItemsTime[i], Settings.Family.AutoItems[i].Time)){
				Brain.ItemsTime[i] = Shovel.Now;
				UseCommonItem(Settings.Family.AutoItems[i].Num);
				if (Shovel.IsDebug) SysMsg('Pressing num pad button ' .. Settings.Family.AutoItems[i].Num);
			}
		}		
	}	
}

Brain.AutoBullets = func(Actor){
	if (!Actor.Settings.UseBullets) return;
	if (!Shovel.IsTime(Brain.AmmoTime, Settings.AmmoUsageDelay)) return;
	Brain.AmmoTime = Shovel.Now;
	
	var numberOfBullets = GetItemCountByType(Actor.Settings.BulletsID);
	if (numberOfBullets < 5000){
		UseCommonItem(Actor.Settings.NumPadButton);
	}
}

// Call this method when auto-keep farming mode is enabled
Brain.ReturnToFarm = func(Actor){	
	if (Settings.Family.Mode == 0)
	{			
		if (IsNearFromKeepDestPosition(Actor.SelfAi, 100) != 'YES'){			
			KeepDestMoveTo(Actor.SelfAi);
			sleep(1000);
		}
		if (Shovel.IsDebug) SysMsg('Returned to farm');
		ChangeTacticsAi(Actor.SelfAi, 'TS_KEEP');
	} else {
		Keep();
	}
}

// Call this method to use hp and sp potions
Brain.AutoPotions = func(){
	if (!Characters.AreInitialized) return;
	for(i = 1, Characters.Count())
	{
		Characters.Update(i);
		if(Characters[i].Settings.AutoHP)
		{
			var onePercent = Characters[i].MaxHP / 100;
			var currentPercent = Characters[i].HP / onePercent;
			if (currentPercent < Characters[i].Settings.HPlevel)
			{
				UseItem(Characters[i].Index - 1, 0);
			}
		}
		if(Characters[i].Settings.AutoSP)
		{
			var onePercent = Characters[i].MaxSP / 100;
			var currentPercent = Characters[i].SP / onePercent;
			if (currentPercent < Characters[i].Settings.SPlevel)
			{
				UseItem(Characters[i].Index - 1, 1);
			}
		}
	}
}

// Call this method to enable user target
Brain.UserTarget = func(Actor){
	var selfAi = Actor.SelfAi;
	var userTarget = GetUserTarget(selfAi);
	if(userTarget != nil){
		SetAiTarget(selfAi, userTarget);
		ClearUserTarget(selfAi);
		if (IsAbleToAttack(selfAi) == 'YES')
		{
			Attack(selfAi, userTarget);
		}
	}
}

// Is character with index [i] (0,1,2) dead?
Brain.GetResAbleTarget = func (i) {
	if (IsExistMyPc(i) == 'YES') {
		var actor = Characters[i+1];			
		var success, friend = pcall(GetSelfActor, actor.SelfAi);
		if (success) {
			success, result = pcall(IsDead, friend);
			if (success) {
				if (result == 'YES')
					return friend;
			}
		}			
	}
}

// Returns dead team member as target if exists.
Brain.GetDeadTeamMember = func () {
	return Brain.GetResAbleTarget(0) || Brain.GetResAbleTarget(1) || Brain.GetResAbleTarget(2);
}

// Returns true or false. Is selected stance correct?
Brain.IsMyStanceCorrect = func(Actor, stance){
	// Check for selected weapon
	var selectedWeaponIsCorrect = false;
	var currentWeapon = Actor.Settings.WeaponSets[Actor.Settings.WeaponIndex];
    for (key, value in currentWeapon.Stances){
		if (value.Id == stance.Id){
			// if current weapon set contains stance, it mean selected weapon is correct
			selectedWeaponIsCorrect = true;
		}
	}
	if (!selectedWeaponIsCorrect){
		SysMsg(Text['IncorrectWeapon']);
	}
	if (stance.Index == Characters.GetSelectedStanceIndex(Actor.Index)){
		return true;
	} else {
		// index of stance and index of selected stance are different
		return false;
	}	
}

Brain.Load();