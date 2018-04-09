﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerTools
{
    public class CommandJail : ConsoleCmdAbstract
    {

        public override string GetDescription()
        {
            return "[ServerTools]-Puts a player in jail.";
        }
        public override string GetHelp()
        {
            return "Usage:\n" +
                "  1. jail add <steamId/entityId>\n" +
                "  2. jail add <steamId/entityId> <time>\n" +
                "  3. jail remove <steamId>" +
                "  4. jail list\n" +
                "1. Adds a steam Id to the jail list for 60 minutes\n" +
                "2. Adds a steam Id to the jail list for a specific time\n" +
                "3. Removes a steam Id from the jail list\n" +
                "4. Lists all steam Id in the jail list" +
                "*Note Use -1 for time to jail indefinitely*";
        }
        public override string[] GetCommands()
        {
            return new string[] { "st-Jail", "jail", string.Empty };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count < 1 || _params.Count > 3)
                {
                    SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1 to 3, found {0}.", _params.Count));
                    return;
                }
                if (_params[0].ToLower().Equals("add"))
                {
                    if (_params.Count < 2 || _params.Count > 3)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2 or 3, found {0}.", _params.Count));
                        return;
                    }
                    if (_params[1].Length < 1 || _params[1].Length > 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not add Id: Invalid Id {0}.", _params[1]));
                        return;
                    }
                    if (Jail.Jailed.Contains(_params[1]))
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not add Id. {0} is already in the Jail list.", _params[1]));
                        return;
                    }
                    if (Jail.Jail_Position == "0,0,0")
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not put a player in jail: Jail position has not been set."));
                        return;
                    }
                    else
                    {
                        ClientInfo _cInfo = ConsoleHelper.ParseParamIdOrName(_params[1]);
                        if (_cInfo != null)
                        {
                            int _jailTime = 60;
                            if (Jail.Jailed.Contains(_cInfo.playerId))
                            {
                                SdtdConsole.Instance.Output(string.Format("Player with Id {0} is already in jail.", _params[1]));
                                return;
                            }
                            else
                            {
                                if (_params[2] != null)
                                {
                                    int.TryParse(_params[2], out _jailTime);
                                }
                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                if (_player.IsSpawned())
                                {
                                    int x, y, z;
                                    string[] _cords = Jail.Jail_Position.Split(',');
                                    int.TryParse(_cords[0], out x);
                                    int.TryParse(_cords[1], out y);
                                    int.TryParse(_cords[2], out z);
                                    _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(x, y, z), false));
                                }
                                Jail.Jailed.Add(_cInfo.playerId);
                                if (_jailTime >= 0)
                                {
                                    string _phrase500;
                                    if (!Phrases.Dict.TryGetValue(500, out _phrase500))
                                    {
                                        _phrase500 = "{PlayerName} you have been sent to jail.";
                                    }
                                    _phrase500 = _phrase500.Replace("{PlayerName}", _cInfo.playerName);
                                    _phrase500 = _phrase500.Replace("{Minutes}", _jailTime.ToString());
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase500), Config.Server_Response_Name, false, "ServerTools", false));
                                    SdtdConsole.Instance.Output(string.Format("You have put {0} in jail for {1} minutes.", _cInfo.playerName, _jailTime));
                                }
                                if (_jailTime == -1)
                                {
                                    string _phrase500;
                                    if (!Phrases.Dict.TryGetValue(500, out _phrase500))
                                    {
                                        _phrase500 = "{PlayerName} you have been sent to jail.";
                                    }
                                    _phrase500 = _phrase500.Replace("{PlayerName}", _cInfo.playerName);
                                    _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase500), Config.Server_Response_Name, false, "ServerTools", false));
                                    SdtdConsole.Instance.Output(string.Format("You have put {0} in jail for life.", _cInfo.playerName));
                                }
                                PersistentContainer.Instance.Players[_cInfo.playerId, true].JailDate = DateTime.Now;
                                PersistentContainer.Instance.Players[_cInfo.playerId, true].JailTime = _jailTime;
                                PersistentContainer.Instance.Players[_cInfo.playerId, true].JailName = _cInfo.playerName;
                                PersistentContainer.Instance.Save();
                            }
                        }
                        else
                        {
                            SdtdConsole.Instance.Output(string.Format("Player with Id {0} can not be found.", _params[1]));
                            return;
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("remove"))
                {
                    if (_params.Count != 2)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 2, found {0}.", _params.Count));
                        return;
                    }
                    if (_params[1].Length != 17)
                    {
                        SdtdConsole.Instance.Output(string.Format("Can not add player Id: Invalid Id {0}.", _params[1]));
                        return;
                    }
                    else
                    {
                        if (!Jail.Jailed.Contains(_params[1]))
                        {
                            SdtdConsole.Instance.Output(string.Format("Player with Id {0} is not in jail. ", _params[1]));
                            return;
                        }
                        else
                        {
                            ClientInfo _cInfo = ConnectionManager.Instance.GetClientInfoForPlayerId(_params[1]);
                            if (_cInfo != null)
                            {
                                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                                EntityBedrollPositionList _position = _player.SpawnPoints;
                                Jail.Jailed.Remove(_cInfo.playerId);
                                if (_position.Count > 0)
                                {
                                    _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(_position[0].x, -1, _position[0].z), false));
                                }
                                else
                                {
                                    Vector3[] _pos = GameManager.Instance.World.GetRandomSpawnPointPositions(1);
                                    _cInfo.SendPackage(new NetPackageTeleportPlayer(new Vector3(_pos[0].x, -1, _pos[0].z), false));
                                }
                                string _phrase501;
                                if (!Phrases.Dict.TryGetValue(501, out _phrase501))
                                {
                                    _phrase501 = "{PlayerName} you have been released from jail.";
                                }
                                _phrase501 = _phrase501.Replace("{PlayerName}", _cInfo.playerName);
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", Config.Chat_Response_Color, _phrase501), Config.Server_Response_Name, false, "ServerTools", false));
                                PersistentContainer.Instance.Players[_cInfo.playerId, true].JailTime = 0;
                                PersistentContainer.Instance.Save();
                                SdtdConsole.Instance.Output(string.Format("You have released a player with id {0} from jail. ", _params[1]));
                                return;
                            }
                            else
                            {
                                Jail.Jailed.Remove(_cInfo.playerId);
                                PersistentContainer.Instance.Players[_cInfo.playerId, true].JailTime = 0;
                                PersistentContainer.Instance.Save();
                                SdtdConsole.Instance.Output(string.Format("Player with Id {0} has been removed from the jail list.", _params[1]));
                                return;
                            }
                        }
                    }
                }
                else if (_params[0].ToLower().Equals("list"))
                {
                    if (_params.Count != 1)
                    {
                        SdtdConsole.Instance.Output(string.Format("Wrong number of arguments, expected 1, found {0}.", _params.Count));
                        return;
                    }
                    if (Jail.Jailed.Count == 0)
                    {
                        SdtdConsole.Instance.Output("There are no Ids on the Jail list.");
                        return;
                    }
                    else
                    {
                        for (int i = 0; i < Jail.Jailed.Count; i++)
                        {
                            string _id = Jail.Jailed[i];
                            Player p = PersistentContainer.Instance.Players[_id, false];
                            {
                                SdtdConsole.Instance.Output(string.Format("Jailed player: steam Id {0} named {1}.", _id, p.JailName));
                            }
                        }
                    }
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("Invalid argument {0}.", _params[0]));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in CommandJail.Run: {0}.", e));
            }
        }
    }
}