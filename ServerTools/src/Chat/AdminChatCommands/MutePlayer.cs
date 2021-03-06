﻿using System;
using System.Collections.Generic;

namespace ServerTools
{
    public class MutePlayer
    {
        public static bool IsEnabled = false;
        public static List<string> Mutes = new List<string>();
        private static string[] _cmd = { "mute" };

        public static void Add(ClientInfo _cInfo, string _playerName)
        {
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_cmd, _cInfo.playerId))
            {
                string _phrase200;
                if (!Phrases.Dict.TryGetValue(200, out _phrase200))
                {
                    _phrase200 = "{PlayerName} you do not have permissions to use this command.";
                }
                _phrase200 = _phrase200.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase200), Config.Server_Response_Name, false, "ServerTools", false));
            }
            else
            {
                _playerName = _playerName.Replace("mute ", "");
                ClientInfo _PlayertoMute = ConsoleHelper.ParseParamIdOrName(_playerName);
                if (_PlayertoMute == null)
                {
                    string _phrase201;
                    if (!Phrases.Dict.TryGetValue(201, out _phrase201))
                    {
                        _phrase201 = "{AdminPlayerName} player {PlayerName} was not found.";
                    }
                    _phrase201 = _phrase201.Replace("{AdminPlayerName}", _cInfo.playerName);
                    _phrase201 = _phrase201.Replace("{PlayerName}", _playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase201, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                }
                else
                {
                    Player p = PersistentContainer.Instance.Players[_PlayertoMute.playerId, false];
                    if (p == null)
                    {
                        Mute(_cInfo, _PlayertoMute);
                    }
                    else
                    {
                        if (p.MuteTime > 0 || p.MuteTime == -1)
                        {
                            string _phrase202;
                            if (!Phrases.Dict.TryGetValue(202, out _phrase202))
                            {
                                _phrase202 = "{AdminPlayerName} player {MutedPlayerName} is already muted.";
                            }
                            _phrase202 = _phrase202.Replace("{AdminPlayerName}", _cInfo.playerName);
                            _phrase202 = _phrase202.Replace("{MutedPlayerName}", _PlayertoMute.playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase202, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                        else
                        {
                            Mute(_cInfo, _PlayertoMute);
                        }
                    }
                }
            }
        }

        public static void Mute (ClientInfo _admin, ClientInfo _player)
        {
            Mutes.Add(_player.playerId);
            PersistentContainer.Instance.Players[_player.playerId, true].MuteTime = 60;
            PersistentContainer.Instance.Players[_player.playerId, true].MuteName = _player.playerName;
            PersistentContainer.Instance.Players[_player.playerId, true].MuteDate = DateTime.Now;
            PersistentContainer.Instance.Save();
            string _phrase203;
            if (!Phrases.Dict.TryGetValue(203, out _phrase203))
            {
                _phrase203 = "{AdminPlayerName} you have muted {MutedPlayerName} for 60 minutes.";
            }
            _phrase203 = _phrase203.Replace("{AdminPlayerName}", _admin.playerName);
            _phrase203 = _phrase203.Replace("{MutedPlayerName}", _player.playerName);
            _admin.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase203, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
        }

        public static void Remove(ClientInfo _cInfo, string _playerName)
        {
            if (!GameManager.Instance.adminTools.CommandAllowedFor(_cmd, _cInfo.playerId))
            {
                string _phrase200;
                if (!Phrases.Dict.TryGetValue(200, out _phrase200))
                {
                    _phrase200 = "{PlayerName} you do not have permissions to use this command.";
                }
                _phrase200 = _phrase200.Replace("{PlayerName}", _cInfo.playerName);
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase200), Config.Server_Response_Name, false, "ServerTools", false));
            }
            else
            {
                _playerName = _playerName.Replace("unmute ", "");
                ClientInfo _PlayertoUnMute = ConsoleHelper.ParseParamIdOrName(_playerName);
                if (_PlayertoUnMute == null)
                {
                    string _phrase201;
                    if (!Phrases.Dict.TryGetValue(201, out _phrase201))
                    {
                        _phrase201 = "{AdminPlayerName} player {PlayerName} was not found online.";
                    }
                    _phrase201 = _phrase201.Replace("{AdminPlayerName}", _cInfo.playerName);
                    _phrase201 = _phrase201.Replace("{PlayerName}", _playerName);
                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase201, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                }
                else
                {
                    Player p = PersistentContainer.Instance.Players[_PlayertoUnMute.playerId, false];
                    if (p == null)
                    {
                        string _phrase204;
                        if (!Phrases.Dict.TryGetValue(204, out _phrase204))
                        {
                            _phrase204 = "{AdminPlayerName} player {PlayerName} is not muted.";
                        }
                        _phrase204 = _phrase204.Replace("{AdminPlayerName}", _cInfo.playerName);
                        _phrase204 = _phrase204.Replace("{PlayerName}", _PlayertoUnMute.playerName);
                        _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase204, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                    }
                    else
                    {
                        if (!Mutes.Contains(_PlayertoUnMute.playerId))
                        {
                            string _phrase204;
                            if (!Phrases.Dict.TryGetValue(204, out _phrase204))
                            {
                                _phrase204 = "{AdminPlayerName} player {PlayerName} is not muted.";
                            }
                            _phrase204 = _phrase204.Replace("{AdminPlayerName}", _cInfo.playerName);
                            _phrase204 = _phrase204.Replace("{PlayerName}", _PlayertoUnMute.playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase204, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                        else
                        {
                            Mutes.Remove(_PlayertoUnMute.playerId);
                            PersistentContainer.Instance.Players[_PlayertoUnMute.playerId, true].MuteTime = 0;
                            PersistentContainer.Instance.Save();
                            string _phrase205;
                            if (!Phrases.Dict.TryGetValue(205, out _phrase205))
                            {
                                _phrase205 = "{AdminPlayerName} you have unmuted {UnMutedPlayerName}.";
                            }
                            _phrase205 = _phrase205.Replace("{AdminPlayerName}", _cInfo.playerName);
                            _phrase205 = _phrase205.Replace("{UnMutedPlayerName}", _PlayertoUnMute.playerName);
                            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{1}{0}[-]", _phrase205, Config.Chat_Response_Color), Config.Server_Response_Name, false, "ServerTools", false));
                        }
                    }
                }
            }
        }

        public static void MuteList()
        {
            for (int i = 0; i < PersistentContainer.Instance.Players.SteamIDs.Count; i++)
            {
                string _id = PersistentContainer.Instance.Players.SteamIDs[i];
                Player p = PersistentContainer.Instance.Players[_id, false];
                {
                    if (p.MuteTime > 0 || p.MuteTime == -1)
                    {
                        if (p.MuteTime == -1)
                        {
                            Mutes.Add(_id);
                            break;
                        }
                        else
                        {
                            TimeSpan varTime = DateTime.Now - p.MuteDate;
                            double fractionalMinutes = varTime.TotalMinutes;
                            int _timepassed = (int)fractionalMinutes;
                            if (_timepassed < p.MuteTime)
                            {
                                Mutes.Add(_id);
                            }
                            else
                            {
                                PersistentContainer.Instance.Players[_id, true].MuteTime = 0;
                                PersistentContainer.Instance.Save();
                            }
                        }
                    }
                }
            }
        }

        public static void Clear()
        {
            for (int i = 0; i < Mutes.Count; i++)
            {
                string _id = Mutes[i];
                Player p = PersistentContainer.Instance.Players[_id, false];
                {
                    if (p.MuteTime != -1)
                    {
                        TimeSpan varTime = DateTime.Now - p.MuteDate;
                        double fractionalMinutes = varTime.TotalMinutes;
                        int _timepassed = (int)fractionalMinutes;
                        if (_timepassed >= p.MuteTime)
                        {
                            Mutes.Remove(_id);
                            PersistentContainer.Instance.Players[_id, true].MuteTime = 0;
                            PersistentContainer.Instance.Save();
                        }
                    }
                }
            }
        }
    }
}