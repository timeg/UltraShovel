if (System){	System.Unload();	} 

System = {}

System.Unload = func (){
	System = nil;
}

// Enumerate all loading methods here
System.Load = func(){	
	System.LoadModules();
	System.LoadData();
	if (Shovel.IsDebug) SysMsg("System loaded");
}

// Little snippet to load file with lua code safety
// with error handling
System.LoadFile = func(module){
	var status, error = pcall (dofile, module);
	if (!status) SysMsg(tostring(error));
}

// Load additional data
System.LoadData = func(){
	System.LoadFile( Shovel.AiFolder .. Shovel.FSP .. Shovel.DataFolder .. Shovel.FSP .. 'Ammo' .. Shovel.Ext );
}

// List of all available modules
System.Modules ={
	[1] = 'settings',
	[2] = 'localization',
	[3] = 'interface',
	[4] = 'addons',
	[5] = 'brain',
	[6] = 'alarms',
	[7] = 'sgewiththeshovel',
	[8] = 'hotkeys',
	[9] = 'characters'
}

// Loads all modules listed before
System.LoadModules = func(){
	for (k, v in pairs(System.Modules)){
		System.LoadFile( Shovel.AiFolder .. Shovel.FSP .. Shovel.ModulesFolder .. Shovel.FSP .. v .. Shovel.Ext );
	}
}

// Method unloads all modules
System.UnloadModules = func(){
	Interface = nil;
	Brain = nil;
	Alarms = nil;
	Text = nil;
	Addons = nil;
	Settings = nil;
	sGeWithTheShovel = nil;
	Hotkeys = nil;
	Characters = nil;
}

// Method prints [table]. Debug purposes.
System.PrintTable = func(tablename){	
	for (key, value in pairs(tablename)){
		SysMsg(tostring(key) .. ' = ' .. tostring(value));
	}
}

// Method returns dumped into string table
System.SerializeTable = func(val, name, skipnewlines, depth){	
	if (skipnewlines == nil){
		skipnewlines = false;
	}
	if (depth == nil){
		depth = 0;
	}
	
    tmp = string.rep("  ", depth)

    if (name != nil){
		if (type(name)=="number"){
			tmp = tmp .. '[' .. name .. ']' .. " = ";
		} else {
			tmp = tmp .. name .. " = ";
		}
	}

    if (type(val) == "table"){
        tmp = tmp .. "{" .. (!skipnewlines && string.char(10) || "")

        for (k, v in pairs(val)){
            tmp =  tmp .. System.SerializeTable(v, k, skipnewlines, depth + 1) .. "," .. (!skipnewlines && string.char(10) || "")
        }

        tmp = tmp .. string.rep("  ", depth) .. "}"
	}
    else if (type(val) == "number"){
        tmp = tmp .. tostring(val)
	}
    else if (type(val) == "string"){
        tmp = tmp .. string.format("%q", val)
    }
	else if (type(val) == "boolean"){
        tmp = tmp .. (val && "true" || "false")
	}
    else{
        tmp = tmp .. "\"[inserializeable datatype:" .. type(val) .. "]\""
    }

    return tmp
}

// Method dumps table with name [value] to file with name [filename]
System.SaveTableToFile = func(value, filename){
	tmp = System.SerializeTable(value, 'TempStorage');
	var file = io.open(filename, 'w');
	if (file) 
	{
		file:write(tmp);
		file:flush();
		file:close();
	}
}

// Method loads table with name [value] from file with name [filename]
System.LoadTableFromFile = func(filename){
	var file = io.open(filename, 'r');
	if (!file) { return nil; }
	file:close();
	dofile(filename);
	return TempStorage;
}

System.Log = func(frameName, text){
	var uiFrame = GetFrameByName(frameName);
	if (!uiFrame) return;
	SetTextByKey(uiFrame, 'l', text);
}

System.Load();