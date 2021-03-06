﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class CustomCommands
    {
        public static bool IsEnabled = false, IsRunning = false;     
        public static Dictionary<string, string[]> Dict = new Dictionary<string, string[]>();
        public static Dictionary<string, int[]> Dict1 = new Dictionary<string, int[]>();
        public static List<int> TeleportCheckProtection = new List<int>();
        private const string file = "CustomChatCommands.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);

        public static void Load()
        {
            if (IsEnabled && !IsRunning)
            {
                LoadXml();
                InitFileWatcher();
            }
        }

        public static void Unload()
        {
            if (IsRunning && !IsEnabled)
            {
                fileWatcher.Dispose();
                IsRunning = false;
            }
        }

        private static void LoadXml()
        {
            if (!Utils.FileExists(filePath))
            {
                UpdateXml();
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(filePath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", file, e.Message));
                return;
            }
            XmlNode _XmlNode = xmlDoc.DocumentElement;
            foreach (XmlNode childNode in _XmlNode.ChildNodes)
            {
                if (childNode.Name == "Commands")
                {
                    Dict.Clear();
                    Dict1.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Commands' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("Trigger"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Commands entry because of missing a Trigger attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!_line.HasAttribute("Response"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Commands entry because of missing a Response attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        int _delay = 0;
                        if (_line.HasAttribute("DelayBetweenUses"))
                        {
                            if (!int.TryParse(_line.GetAttribute("DelayBetweenUses"), out _delay))
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Using default value of 0 for DelayBetweenUses for command entry {1} because of invalid (non-numeric) value: {0}", subChild.OuterXml, _line.GetAttribute("Trigger")));
                            }
                        }
                        int _number = 0;
                        if (_line.HasAttribute("Number"))
                        {
                            if (!int.TryParse(_line.GetAttribute("Number"), out _number))
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Using default value of 0 for Number for command entry {1} because of invalid (non-numeric) value: {0}", subChild.OuterXml, _line.GetAttribute("Trigger")));
                            }
                        }
                        string _trigger = _line.GetAttribute("Trigger");
                        if (!Dict.ContainsKey(_trigger))
                        {
                            string _response1 = _line.GetAttribute("Response");
                            string _response2 = _line.GetAttribute("Response2");
                            string[] _response = { _response1, _response2 };
                            Dict.Add(_trigger, _response);
                        }
                        if (!Dict1.ContainsKey(_trigger))
                        {
                            int[] _c = { _number, _delay };
                            Dict1.Add(_trigger, _c);
                        }
                    }
                }
            }
        }

        private static void UpdateXml()
        {
            fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<CustomCommands>");
                sw.WriteLine("    <Commands>");
                sw.WriteLine("        <!-- possible variables {EntityId} {SteamId} {PlayerName}-->");
                if (Dict.Count > 0)
                {
                    foreach (KeyValuePair<string, string[]> kvp in Dict)
                    {
                        int[] _value;
                        if (Dict1.TryGetValue(kvp.Key, out _value))
                        {
                            sw.WriteLine(string.Format("        <Command Number=\"{0}\" Trigger=\"{1}\" Response=\"{2}\" Response2=\"{3}\" DelayBetweenUses=\"{4}\" />", _value[0], kvp.Key, kvp.Value[0], kvp.Value[1], _value[1]));
                        }
                    }
                }
                else
                {
                    sw.WriteLine("        <Command Number=\"1\" Trigger=\"help\" Response=\"say &quot;Type /commands for a list of chat commands.&quot;\" Response2=\"\" DelayBetweenUses=\"0\" />");
                    sw.WriteLine("        <Command Number=\"2\" Trigger=\"info\" Response=\"say &quot;Type /commands for a list of chat commands.&quot;\" Response2=\"\" DelayBetweenUses=\"0\" />");
                    sw.WriteLine("        <Command Number=\"3\" Trigger=\"rules\" Response=\"say &quot;Visit YourSiteHere to see the rules.&quot;\" Response2=\"\" DelayBetweenUses=\"0\" />");
                    sw.WriteLine("        <Command Number=\"4\" Trigger=\"website\" Response =\"say &quot;Visit YourSiteHere.&quot;\" Response2=\"\" DelayBetweenUses=\"0\" />");
                    sw.WriteLine("        <Command Number=\"5\" Trigger=\"teamspeak\" Response=\"say &quot;The Teamspeak3 info is YourInfoHere.&quot;\" Response2=\"\" DelayBetweenUses=\"0\" />");
                    sw.WriteLine("        <Command Number=\"6\" Trigger=\"market\" Response=\"tele {EntityId} 0 -1 0\" Response2=\"pm {EntityId} &quot;{PlayerName} you have been sent to the market.&quot;\" DelayBetweenUses=\"60\" />");
                    sw.WriteLine("        <Command Number=\"7\" Trigger=\"spawnZ\" Response=\"ser {EntityId} 20 @ 4 9 11\" Response2=\"pm {EntityId} &quot;Spawned zombies on you.&quot;\" DelayBetweenUses=\"60\" />");
                    sw.WriteLine("        <Command Number=\"8\" Trigger=\"test3\" Response=\"Your command here\" Response2=\"\" DelayBetweenUses=\"20\" />");
                    sw.WriteLine("        <Command Number=\"9\" Trigger=\"test4\" Response=\"Your command here\" Response2=\"\" DelayBetweenUses=\"30\" />");
                    sw.WriteLine("        <Command Number=\"10\" Trigger=\"test5\" Response=\"Your command here\" Response2=\"\" DelayBetweenUses=\"40\" />");
                }
                sw.WriteLine("    </Commands>");
                sw.WriteLine("</CustomCommands>");
                sw.Flush();
                sw.Close();
            }
            fileWatcher.EnableRaisingEvents = true;
        }

        private static void InitFileWatcher()
        {
            fileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.EnableRaisingEvents = true;
            IsRunning = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!Utils.FileExists(filePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static string GetChatCommands1(ClientInfo _cInfo)
        {
            string _commands_1 = string.Format("{0}Commands are:", Config.Chat_Response_Color);
            if (FriendTeleport.IsEnabled)
            {
                _commands_1 = string.Format("{0} /friend /friend # /accept", _commands_1);
            }
            if (Shop.IsEnabled)
            {
                _commands_1 = string.Format("{0} /wallet /shop /buy", _commands_1);
            }
            if (Gimme.IsEnabled)
            {
                _commands_1 = string.Format("{0} /gimme", _commands_1);
            }
            if (TeleportHome.IsEnabled)
            {
                _commands_1 = string.Format("{0} /sethome /home /fhome /delhome", _commands_1);
            }
            if (Day7.IsEnabled)
            {
                _commands_1 = string.Format("{0} /day7", _commands_1);
            }
            if (Bloodmoon.IsEnabled)
            {
                _commands_1 = string.Format("{0} /bloodmoon", _commands_1);
            }
            if (IsEnabled)
            {
                _commands_1 = string.Format("{0} /pm /re", _commands_1);
            }
            if (ClanManager.IsEnabled)
            {
                _commands_1 = string.Format("{0} /clancommands", _commands_1);
            }
            return _commands_1;
        }

        public static string GetChatCommands2(ClientInfo _cInfo)
        {
            string _commands_2 = string.Format("{0}More Commands:", Config.Chat_Response_Color);
            if (FirstClaimBlock.IsEnabled)
            {
                _commands_2 = string.Format("{0} /claim", _commands_2);
            }
            if (RestartVote.IsEnabled)
            {
                _commands_2 = string.Format("{0} /restart", _commands_2);
                if (RestartVote.VoteOpen)
                {
                    _commands_2 = string.Format("{0} /yes /no", _commands_2);
                }
            }
            if (Animals.IsEnabled)
            {
                _commands_2 = string.Format("{0} /trackanimal /track", _commands_2);
            }
            if (VoteReward.IsEnabled)
            {
                _commands_2 = string.Format("{0} /reward", _commands_2);
            }
            if (ChatHook.Donator_Name_Coloring)
            {
                _commands_2 = string.Format("{0} /doncolor", _commands_2);
            }
            if (ChatHook.Reserved_Check)
            {
                _commands_2 = string.Format("{0} /reserved", _commands_2);
            }
            if (AutoShutdown.IsEnabled)
            {
                _commands_2 = string.Format("{0} /shutdown", _commands_2);
            }
            if (AdminList.IsEnabled)
            {
                _commands_2 = string.Format("{0} /admin", _commands_2);
            }
            if (Travel.IsEnabled)
            {
                _commands_2 = string.Format("{0} /travel", _commands_2);
            }
            if (ChatHook.Special_Player_Name_Coloring && ChatHook.SpecialPlayers.Contains(_cInfo.playerId))
            {
                _commands_2 = string.Format("{0} /spcolor", _commands_2);
            }
            if (TeleportHome.IsEnabled & TeleportHome.Set_Home2_Enabled)
            {
                _commands_2 = string.Format("{0} /sethome2 /home2 /fhome2 /delhome2", _commands_2);
            }
            return _commands_2;
        }

        public static string GetChatCommands3(ClientInfo _cInfo)
        {
            string _commands_3 = string.Format("{0}More Commands:", Config.Chat_Response_Color);
            if (WeatherVote.IsEnabled)
            {
                _commands_3 = string.Format("{0} /weather", _commands_3);
                if (WeatherVote.VoteOpen)
                {
                    _commands_3 = string.Format("{0} /sun /rain /snow /fog /wind", _commands_3);
                }
            }
            if (AuctionBox.IsEnabled)
            {
                _commands_3 = string.Format("{0} /auction /auction sell # /auction buy # /auction cancel", _commands_3);
            }
            if (DeathSpot.IsEnabled)
            {
                _commands_3 = string.Format("{0} /died", _commands_3);
            }
            if (Fps.IsEnabled)
            {
                _commands_3 = string.Format("{0} /fps", _commands_3);
            }
            if (Loc.IsEnabled)
            {
                _commands_3 = string.Format("{0} /loc", _commands_3);
            }
            if (MuteVote.IsEnabled)
            {
                _commands_3 = string.Format("{0} /mute", _commands_3);
            }
            if (KickVote.IsEnabled)
            {
                _commands_3 = string.Format("{0} /kick", _commands_3);
            }
            if (Suicide.IsEnabled)
            {
                _commands_3 = string.Format("{0} /killme /suicide", _commands_3);
            }
            return _commands_3;
        }

        public static string GetChatCommands4(ClientInfo _cInfo)
        {
            string _commands_4 = string.Format("{0}More Commands:", Config.Chat_Response_Color);
            if (LobbyChat.IsEnabled)
            {
                _commands_4 = string.Format("{0} /lobby", _commands_4);
                if (LobbyChat.Return)
                {
                    _commands_4 = string.Format("{0} /return", _commands_4);
                }
            }
            if (Bounties.IsEnabled)
            {
                _commands_4 = string.Format("{0} /bountylist /bounty #", _commands_4);
            }
            if (Lottery.IsEnabled)
            {
                _commands_4 = string.Format("{0} /lottery /lottery # /lottery enter", _commands_4);
            }
            if (Report.IsEnabled)
            {
                _commands_4 = string.Format("{0} /report", _commands_4);
            }
            if (BikeReturn.IsEnabled)
            {
                _commands_4 = string.Format("{0} /bike", _commands_4);
            }
            if (Stuck.IsEnabled)
            {
                _commands_4 = string.Format("{0} /stuck", _commands_4);
            }
            if (RestartVote.IsEnabled)
            {
                _commands_4 = string.Format("{0} /restart", _commands_4);
            }
            if (Bank.IsEnabled)
            {
                _commands_4 = string.Format("{0} /bank /deposit /withdraw /wallet deposit /wallet withdraw", _commands_4);
            }
            return _commands_4;
        }

        public static string GetChatCommandsCustom(ClientInfo _cInfo)
        {
            string _commandsCustom = string.Format("{0}Custom commands are:", Config.Chat_Response_Color);
            if (Dict.Count > 0)
            {
                foreach (KeyValuePair<string, string[]> kvp in Dict)
                {
                    string _c = kvp.Key;
                    _commandsCustom = string.Format("{0} /{1}", _commandsCustom, _c);
                }
            }
            if (_commandsCustom.EndsWith("Custom commands are:"))
            {
                _commandsCustom = string.Format("{0}Sorry, there are no custom chat commands.", Config.Chat_Response_Color);
            }
            _commandsCustom = string.Format("{0}[-]", _commandsCustom);
            return _commandsCustom;
        }

        public static string GetChatCommandsAdmin(ClientInfo _cInfo)
        {
            string _commandsAdmin = string.Format("{0}Admin commands are:", Config.Chat_Response_Color);
            if (AdminChat.IsEnabled && GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId))
            {
                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                if (Admin.PermissionLevel <= ChatHook.Admin_Level)
                {
                    _commandsAdmin = string.Format("{0} @admins", _commandsAdmin);
                    string[] _command = { "say" };
                    if (GameManager.Instance.adminTools.CommandAllowedFor(_command, _cInfo.playerId))
                    {
                        _commandsAdmin = string.Format("{0} @all", _commandsAdmin);
                    }
                    string[] _command1 = { "jail" };
                    if (GameManager.Instance.adminTools.CommandAllowedFor(_command1, _cInfo.playerId))
                    {
                        if (Jail.IsEnabled)
                        {
                            _commandsAdmin = string.Format("{0} /jail", _commandsAdmin);
                        }
                    }
                    string[] _command2 = { "mute" };
                    if (GameManager.Instance.adminTools.CommandAllowedFor(_command2, _cInfo.playerId))
                    {
                        _commandsAdmin = string.Format("{0} /mute", _commandsAdmin);
                    }
                    if (_commandsAdmin.EndsWith("Admin commands are:"))
                    {
                        _commandsAdmin = string.Format("{0}Sorry, there are no admin chat commands.", Config.Chat_Response_Color);
                    }
                }
            }
            return _commandsAdmin;
        }

        public static void CheckCustomDelay(ClientInfo _cInfo, string _message, string _playerName, bool _announce)
        {
            int _timepassed = 0;
            int[] _c;
            if (Dict1.TryGetValue(_message, out _c))
            {
                if (_c[1] == 0)
                {
                    CommandResponse(_cInfo, _message, _playerName, _announce, _c);
                }
                else
                {
                    bool _donator = false;
                    Player p = PersistentContainer.Instance.Players[_cInfo.playerId, false];
                    if (_c[0] == 1)
                    {
                        if (p.CustomCommand1 != null)
                        {
                            TimeSpan varTime = DateTime.Now - p.CustomCommand1;
                            double fractionalMinutes = varTime.TotalMinutes;
                            _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _donator = true;
                                        int _newDelay = _c[1] / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            CommandResponse(_cInfo, _message, _playerName, _announce, _c);
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            DelayResponse(_cInfo, _message, _playerName, _announce, _timeleft, _newDelay);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _timepassed = -1;
                        }
                    }
                    if (_c[0] == 2)
                    {
                        if (p.CustomCommand2 != null)
                        {
                            TimeSpan varTime = DateTime.Now - p.CustomCommand2;
                            double fractionalMinutes = varTime.TotalMinutes;
                            _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _donator = true;
                                        int _newDelay = _c[1] / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            CommandResponse(_cInfo, _message, _playerName, _announce, _c);
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            DelayResponse(_cInfo, _message, _playerName, _announce, _timeleft, _newDelay);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _timepassed = -1;
                        }
                    }
                    if (_c[0] == 3)
                    {
                        if (p.CustomCommand3 != null)
                        {
                            TimeSpan varTime = DateTime.Now - p.CustomCommand3;
                            double fractionalMinutes = varTime.TotalMinutes;
                            _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _donator = true;
                                        int _newDelay = _c[1] / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            CommandResponse(_cInfo, _message, _playerName, _announce, _c);
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            DelayResponse(_cInfo, _message, _playerName, _announce, _timeleft, _newDelay);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _timepassed = -1;
                        }
                    }
                    if (_c[0] == 4)
                    {
                        if (p.CustomCommand4 != null)
                        {
                            TimeSpan varTime = DateTime.Now - p.CustomCommand4;
                            double fractionalMinutes = varTime.TotalMinutes;
                            _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _donator = true;
                                        int _newDelay = _c[1] / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            CommandResponse(_cInfo, _message, _playerName, _announce, _c);
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            DelayResponse(_cInfo, _message, _playerName, _announce, _timeleft, _newDelay);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _timepassed = -1;
                        }
                    }
                    if (_c[0] == 5)
                    {
                        if (p.CustomCommand5 != null)
                        {
                            TimeSpan varTime = DateTime.Now - p.CustomCommand5;
                            double fractionalMinutes = varTime.TotalMinutes;
                            _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _donator = true;
                                        int _newDelay = _c[1] / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            CommandResponse(_cInfo, _message, _playerName, _announce, _c);
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            DelayResponse(_cInfo, _message, _playerName, _announce, _timeleft, _newDelay);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _timepassed = -1;
                        }
                    }
                    if (_c[0] == 6)
                    {
                        if (p.CustomCommand6 != null)
                        {
                            TimeSpan varTime = DateTime.Now - p.CustomCommand6;
                            double fractionalMinutes = varTime.TotalMinutes;
                            _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _donator = true;
                                        int _newDelay = _c[1] / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            CommandResponse(_cInfo, _message, _playerName, _announce, _c);
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            DelayResponse(_cInfo, _message, _playerName, _announce, _timeleft, _newDelay);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _timepassed = -1;
                        }
                    }
                    if (_c[0] == 7)
                    {
                        if (p.CustomCommand7 != null)
                        {
                            TimeSpan varTime = DateTime.Now - p.CustomCommand7;
                            double fractionalMinutes = varTime.TotalMinutes;
                            _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _donator = true;
                                        int _newDelay = _c[1] / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            CommandResponse(_cInfo, _message, _playerName, _announce, _c);
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            DelayResponse(_cInfo, _message, _playerName, _announce, _timeleft, _newDelay);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _timepassed = -1;
                        }
                    }
                    if (_c[0] == 8)
                    {
                        if (p.CustomCommand8 != null)
                        {
                            TimeSpan varTime = DateTime.Now - p.CustomCommand8;
                            double fractionalMinutes = varTime.TotalMinutes;
                            _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _donator = true;
                                        int _newDelay = _c[1] / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            CommandResponse(_cInfo, _message, _playerName, _announce, _c);
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            DelayResponse(_cInfo, _message, _playerName, _announce, _timeleft, _newDelay);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _timepassed = -1;
                        }
                    }
                    if (_c[0] == 9)
                    {
                        if (p.CustomCommand9 != null)
                        {
                            TimeSpan varTime = DateTime.Now - p.CustomCommand9;
                            double fractionalMinutes = varTime.TotalMinutes;
                            _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _donator = true;
                                        int _newDelay = _c[1] / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            CommandResponse(_cInfo, _message, _playerName, _announce, _c);
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            DelayResponse(_cInfo, _message, _playerName, _announce, _timeleft, _newDelay);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _timepassed = -1;
                        }
                    }
                    if (_c[0] == 10)
                    {
                        if (p.CustomCommand10 != null)
                        {
                            TimeSpan varTime = DateTime.Now - p.CustomCommand10;
                            double fractionalMinutes = varTime.TotalMinutes;
                            _timepassed = (int)fractionalMinutes;
                            if (ReservedSlots.IsEnabled && ReservedSlots.Reduced_Delay)
                            {
                                if (ReservedSlots.Dict.ContainsKey(_cInfo.playerId))
                                {
                                    DateTime _dt;
                                    ReservedSlots.Dict.TryGetValue(_cInfo.playerId, out _dt);
                                    if (DateTime.Now < _dt)
                                    {
                                        _donator = true;
                                        int _newDelay = _c[1] / 2;
                                        if (_timepassed >= _newDelay)
                                        {
                                            CommandResponse(_cInfo, _message, _playerName, _announce, _c);
                                        }
                                        else
                                        {
                                            int _timeleft = _newDelay - _timepassed;
                                            DelayResponse(_cInfo, _message, _playerName, _announce, _timeleft, _newDelay);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _timepassed = -1;
                        }
                    }
                    if (!_donator)
                    {
                        if (_timepassed >= _c[1] || _timepassed == -1)
                        {
                            CommandResponse(_cInfo, _message, _playerName, _announce, _c);
                        }
                        else
                        {
                            int _timeleft = _c[1] - _timepassed;
                            string _phrase616;
                            if (!Phrases.Dict.TryGetValue(616, out _phrase616))
                            {
                                _phrase616 = "{PlayerName} you can only use {Command} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
                            }
                            _phrase616 = _phrase616.Replace("{Command}", _message);
                            _phrase616 = _phrase616.Replace("{PlayerName}", _playerName);
                            _phrase616 = _phrase616.Replace("{DelayBetweenUses}", _c[1].ToString());
                            _phrase616 = _phrase616.Replace("{TimeRemaining}", _timeleft.ToString());
                            if (_announce)
                            {
                                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase616), Config.Server_Response_Name, false, "ServerTools", false);
                            }
                            else
                            {
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase616), Config.Server_Response_Name, false, "ServerTools", false));
                            }
                        }
                    }
                }
            }
        }

        public static void DelayResponse(ClientInfo _cInfo, string _message, string _playerName, bool _announce, int _timeleft, int _newDelay)
        {
            string _phrase616;
            if (!Phrases.Dict.TryGetValue(616, out _phrase616))
            {
                _phrase616 = "{PlayerName} you can only use {Command} once every {DelayBetweenUses} minutes. Time remaining: {TimeRemaining} minutes.";
            }
            _phrase616 = _phrase616.Replace("{Command}", _message);
            _phrase616 = _phrase616.Replace("{PlayerName}", _playerName);
            _phrase616 = _phrase616.Replace("{DelayBetweenUses}", _newDelay.ToString());
            _phrase616 = _phrase616.Replace("{TimeRemaining}", _timeleft.ToString());
            if (_announce)
            {
                GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase616), Config.Server_Response_Name, false, "ServerTools", false);
            }
            else
            {
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase616), Config.Server_Response_Name, false, "ServerTools", false));
            }
        }

        public static string CommandResponse(ClientInfo _cInfo, string _message, string _playerName, bool _announce, int[] _c)
        {
            string[] _r;
            if (Dict.TryGetValue(_message, out _r))
            {
                string _response = _r[0];
                _response = _response.Replace("{EntityId}", _cInfo.entityId.ToString());
                _response = _response.Replace("{SteamId}", _cInfo.playerId);
                _response = _response.Replace("{PlayerName}", _playerName);
                if (_response.StartsWith("say "))
                {
                    _response = _response.Replace("say ", "");
                    if (_announce)
                    {
                        GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _response), Config.Server_Response_Name, false, "ServerTools", false);
                    }
                    else
                    {
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _response), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                }
                if (_response.StartsWith("tele ") || _response.StartsWith("tp ") || _response.StartsWith("teleportplayer "))
                {
                    Players.NoFlight.Add(_cInfo.entityId);
                    if (Players.ZoneExit.ContainsKey(_cInfo.entityId))
                    {
                        Players.ZoneExit.Remove(_cInfo.entityId);
                    }
                    SdtdConsole.Instance.ExecuteSync(_response, _cInfo);
                }
                else
                {
                    SdtdConsole.Instance.ExecuteSync(_response, _cInfo);
                }
                string _response2 = _r[1];
                if (_response2 != null)
                {
                    _response2 = _response2.Replace("{EntityId}", _cInfo.entityId.ToString());
                    _response2 = _response2.Replace("{SteamId}", _cInfo.playerId);
                    _response2 = _response2.Replace("{PlayerName}", _playerName);
                    if (_response2.StartsWith("say "))
                    {
                        _response2 = _response2.Replace("say ", "");
                        if (_announce)
                        {
                            GameManager.Instance.GameMessageServer((ClientInfo)null, EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _response2), Config.Server_Response_Name, false, "ServerTools", false);
                        }
                        else
                        {
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _response2), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                    if (_response2.StartsWith("tele ") || _response2.StartsWith("tp ") || _response2.StartsWith("teleportplayer "))
                    {
                        Players.NoFlight.Add(_cInfo.entityId);
                        SdtdConsole.Instance.ExecuteSync(_response2, _cInfo);
                    }
                    else
                    {
                        SdtdConsole.Instance.ExecuteSync(_response2, _cInfo);
                    }
                    if (_c[0] == 1)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand1 = DateTime.Now;
                        PersistentContainer.Instance.Save();
                    }
                    if (_c[0] == 2)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand2 = DateTime.Now;
                        PersistentContainer.Instance.Save();
                    }
                    if (_c[0] == 3)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand3 = DateTime.Now;
                        PersistentContainer.Instance.Save();
                    }
                    if (_c[0] == 4)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand4 = DateTime.Now;
                        PersistentContainer.Instance.Save();
                    }
                    if (_c[0] == 5)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand5 = DateTime.Now;
                        PersistentContainer.Instance.Save();
                    }
                    if (_c[0] == 6)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand6 = DateTime.Now;
                        PersistentContainer.Instance.Save();
                    }
                    if (_c[0] == 7)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand7 = DateTime.Now;
                        PersistentContainer.Instance.Save();
                    }
                    if (_c[0] == 8)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand8 = DateTime.Now;
                        PersistentContainer.Instance.Save();
                    }
                    if (_c[0] == 9)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand9 = DateTime.Now;
                        PersistentContainer.Instance.Save();
                    }
                    if (_c[0] == 10)
                    {
                        PersistentContainer.Instance.Players[_cInfo.playerId, true].CustomCommand10 = DateTime.Now;
                        PersistentContainer.Instance.Save();
                    }
                }
            }
            return null;
        }
    }
}